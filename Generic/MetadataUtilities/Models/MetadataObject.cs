using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MetadataObject : DatabaseObject
    {
        private string _cleanedUpName;
        private string _editName;
        private int _gameCount;
        private Guid _id;
        private string _name = string.Empty;
        private string _prefix = string.Empty;
        private bool _selected;
        private bool _showGrouped;
        private FieldType _type;
        private IMetadataFieldType _typeManager;

        public MetadataObject(Settings settings) => Settings = settings;

        [DontSerialize]
        public string CleanedUpName
        {
            get => _cleanedUpName;
            set => SetValue(ref _cleanedUpName, value);
        }

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
                    CleanedUpName = EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
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
                if (_id == default && TypeManager is IObjectType type)
                {
                    _id = type.GetDbObjectId(Name);
                }

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

                CleanedUpName = EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
            }
        }

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
        public Settings Settings { get; }

        [DontSerialize]
        public bool ShowGrouped
        {
            get => _showGrouped;
            set => SetValue(ref _showGrouped, value);
        }

        public FieldType Type
        {
            get => _type;
            set
            {
                SetValue(ref _type, value);

                TypeManager = value.GetTypeManager();
            }
        }

        [DontSerialize]
        public string TypeAndName => Name.Any() ? $"{TypeLabel}: {Name}" : TypeLabel;

        [DontSerialize]
        public string TypeLabel => TypeManager.LabelSingular;

        [DontSerialize]
        public IMetadataFieldType TypeManager
        { get => _typeManager; set => SetValue(ref _typeManager, value); }

        public Guid AddToDb()
        {
            if (TypeManager is IEditableObjectType type)
            {
                Id = type.AddDbObject(Name);
            }

            return Id;
        }

        public bool AddToGame(Game game) => TypeManager is IEditableObjectType type && type.AddValueToGame(game, Id);

        public void CheckGroup(List<MetadataObject> metadataList)
            => ShowGrouped = metadataList.Any(x => x.CleanedUpName == CleanedUpName && !x.Equals(this));

        public bool ExistsInDb() => TypeManager is IObjectType type && type.DbObjectExists(Name);

        public bool ExistsInGame(Game game) => TypeManager is IValueType type && type.GameContainsValue(game, Id);

        public void GetGameCount()
        {
            if (TypeManager is IObjectType type)
            {
                GameCount = type.GetGameCount(Id, Settings.IgnoreHiddenGamesInGameCount);
            }
        }

        public List<Game> GetGames() =>
            TypeManager is IObjectType type
                ? type.GetGames(Id, Settings.IgnoreHiddenGamesInGameCount)
                : new List<Game>();

        public string GetPrefix()
        {
            if (Settings?.Prefixes == null)
            {
                return string.Empty;
            }

            foreach (var prefix in Settings.Prefixes)
            {
                if (Name?.StartsWith(prefix) ?? false)
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        public bool IsInGame(Game game) => TypeManager is IObjectType type && type.DbObjectInGame(game, Id);

        public bool IsUsed(bool ignoreHiddenGames = false) => TypeManager is IObjectType type && type.DbObjectInUse(Id, ignoreHiddenGames);

        public void RefreshId()
        {
            if (TypeManager is IObjectType type)
            {
                Id = type.GetDbObjectId(Name);
            }
        }

        public bool RemoveFromDb(bool checkIfUsed = true)
        {
            if (!(TypeManager is IEditableObjectType type))
            {
                return false;
            }

            if (checkIfUsed)
            {
                var gamesAffected = type.RemoveObjectFromGames(API.Instance.Database.Games.ToList(), Id);

                MetadataFunctions.UpdateGames(gamesAffected.ToList());
            }

            if (!type.RemoveDbObject(Id))
            {
                return false;
            }

            Id = default;

            return true;
        }

        public bool RemoveFromGame(Game game) => TypeManager is IEditableObjectType type && type.RemoveObjectFromGame(game, Id);

        public IEnumerable<Guid> ReplaceInDb(List<Game> games, FieldType? newType = null, Guid? newId = null)
        {
            IEnumerable<Guid> gameIds = new List<Guid>();

            if (!(TypeManager is IEditableObjectType type))
            {
                return gameIds;
            }

            gameIds = newType?.GetTypeManager() is IEditableObjectType newTypeManager
                ? type.ReplaceDbObject(games, Id, newTypeManager, newId)
                : type.ReplaceDbObject(games, Id);

            return gameIds;
        }

        public bool UpdateItem(string newName)
        {
            // If we don't have an id, the item is new and doesn't need to be updated.
            if (Id == default)
            {
                return true;
            }

            var res = UpdateName(newName);

            if (res == DbInteractionResult.IsDuplicate)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                    TypeLabel));
            }

            return res == DbInteractionResult.Updated;
        }

        public DbInteractionResult UpdateName(string newName) =>
            TypeManager is IEditableObjectType type
                ? type.UpdateName(Id, Name, newName)
                : DbInteractionResult.Error;
    }

    public class MetadataObjectEqualityComparer : IEqualityComparer<MetadataObject>
    {
        public bool Equals(MetadataObject x, MetadataObject y) => x?.TypeAndName.Equals(y?.TypeAndName) ?? false;

        public int GetHashCode(MetadataObject obj) => obj.TypeAndName.GetHashCode();
    }
}