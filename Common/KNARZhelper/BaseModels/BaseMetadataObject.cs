using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.BaseModels
{
    public class BaseMetadataObject : DatabaseObject
    {
        private string _cleanedUpName;
        private string _editName;
        private int _gameCount;
        internal Guid _id;
        private string _name = string.Empty;
        private string _prefix = string.Empty;
        private bool _selected;
        private bool _showGrouped;
        internal FieldType _type;
        private IMetadataFieldType _typeManager;

        public BaseMetadataObject(IMetadataFieldType typeManager, FieldType type, string name = default)
        {
            TypeManager = typeManager;
            Type = type;
            Name = name;
        }

        public BaseMetadataObject(FieldType type, string name = default)
        {
            Type = type;
            Name = name;
        }

        [DontSerialize]
        public string CleanedUpName
        {
            get => _cleanedUpName;
            set => SetValue(ref _cleanedUpName, value);
        }

        [DontSerialize]
        public string DisplayName { get; private set; }

        [DontSerialize]
        public string EditName
        {
            get => _editName;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_editName != value && UpdateItem(Prefix + value))
                {
                    SetValue(ref _editName, value);
                    _name = Prefix + value;
                    CleanedUpName = GetCleanedUpName();
                    DisplayName = GetDisplayName();
                }

                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        [DontSerialize]
        public new Guid Id
        {
            get
            {
                if (_id != default || !(TypeManager is IObjectType type))
                {
                    return _id;
                }

                _id = type.GetDbObjectId(Name);

                NotifyMissingItem();

                return _id;
            }
            set => SetValue(ref _id, value);
        }

        public new string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                SetValue(ref _name, value);

                _prefix = GetPrefix();

                if (value.Equals(_prefix))
                {
                    _prefix = string.Empty;
                    _editName = value;
                }
                else
                {
                    _editName = _prefix == string.Empty ? value : value.RemoveFirst(_prefix);
                }

                CleanedUpName = GetCleanedUpName();

                DisplayName = GetDisplayName();
            }
        }

        [DontSerialize]
        public virtual bool IgnoreHiddenGamesInCount => false;

        [DontSerialize]
        public string Prefix
        {
            get => _prefix;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (_prefix == value || !UpdateItem(value + EditName))
                {
                    return;
                }

                SetValue(ref _prefix, value);
                _name = value + EditName;
            }
        }

        [DontSerialize]
        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }

        [DontSerialize]
        public bool ShowGrouped
        {
            get => _showGrouped;
            set => SetValue(ref _showGrouped, value);
        }

        public virtual FieldType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

        [DontSerialize]
        public string TypeAndName => Name.Any() ? $"{TypeLabel}: {Name}" : TypeLabel;

        [DontSerialize]
        public string TypeLabel => TypeManager.LabelSingular;

        [DontSerialize]
        public IMetadataFieldType TypeManager
        {
            get => _typeManager;
            set => SetValue(ref _typeManager, value);
        }

        public Guid AddToDb()
        {
            if (Name == null || Name == string.Empty)
            {
                return Guid.Empty;
            }

            if (TypeManager is IEditableObjectType editableType)
            {
                Id = editableType.AddDbObject(Name);
            }
            // If we can't add the item, for consistency we still get the id, since adding also returns the id.
            else
            {
                RefreshId();
            }

            return Id;
        }

        public bool AddToGame(Game game) => TypeManager is IEditableObjectType type && type.AddValueToGame(game, Id);

        public void CheckGroup(Dictionary<string, int> groups) => ShowGrouped = groups.TryGetValue(CleanedUpName, out var count) && count > 1;

        public bool ExistsInDb() => TypeManager is IObjectType type && type.DbObjectExists(Name);

        public bool ExistsInGame(Game game) => TypeManager is IValueType type && type.GameContainsValue(game, Id);

        public string GetCleanedUpName()
        {
            switch (Type)
            {
                case FieldType.Developer:
                case FieldType.Publisher:
                    if (string.IsNullOrEmpty(EditName))
                    {
                        return EditName;
                    }

                    var newName = EditName;

                    if (newName.EndsWith(")"))
                    {
                        newName = newName.Remove(newName.Length - 1);
                    }

                    newName = MiscHelper.CompanyFormRegex.Replace(newName, string.Empty).CollapseWhitespaces().Trim();

                    if (EditName.EndsWith(")"))
                    {
                        newName = $"{newName})";
                    }

                    return newName;
                default:
                    return EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
            }
        }

        public virtual string GetDisplayName() => Name;

        public void GetGameCount()
        {
            if (TypeManager is IObjectType type)
            {
                GameCount = type.GetGameCount(Id, IgnoreHiddenGamesInCount);
            }
        }

        public List<Game> GetGames()
        {
            return TypeManager is IObjectType type
                ? type.GetGames(Id, IgnoreHiddenGamesInCount)
                : new List<Game>();
        }

        public virtual string GetPrefix() => string.Empty;

        public bool IsInGame(Game game) => TypeManager is IObjectType type && type.DbObjectInGame(game, Id);

        public bool IsUsed(bool ignoreHiddenGames = false) => TypeManager is IObjectType type && type.DbObjectInUse(Id, ignoreHiddenGames);

        public void RefreshId()
        {
            if (!(TypeManager is IObjectType type))
            {
                return;
            }

            Id = type.GetDbObjectId(Name);

            NotifyMissingItem();
        }

        public bool RemoveFromGame(Game game) => TypeManager is IEditableObjectType type && type.RemoveObjectFromGame(game, Id);

        public event RenameObjectEventHandler RenameObject;

        public bool UpdateItem(string newName) => Id == default || (RenameObject == null) || (RenameObject?.Invoke(this, Name, newName) ?? true);

        public DbInteractionResult UpdateName(string newName)
        {
            return TypeManager is IEditableObjectType type
                ? type.UpdateName(Id, Name, newName)
                : DbInteractionResult.Error;
        }

        public virtual void NotifyMissingItem()
        {
            // Implementation for notifying about a missing item
        }
    }
}
