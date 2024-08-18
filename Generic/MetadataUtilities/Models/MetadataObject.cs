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
        private readonly Settings _settings;
        private string _cleanedUpName;
        private string _editName;
        private int _gameCount;
        private Guid _id;
        private string _name = string.Empty;
        private string _prefix = string.Empty;
        private bool _selected;
        private bool _showGrouped;
        private FieldType _type;
        private IDatabaseObjectType _typeManager;

        public MetadataObject(Settings settings) => _settings = settings;

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
                if (_id == default)
                {
                    _id = TypeManager.GetDbObjectId(Name);
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

                switch (value)
                {
                    case FieldType.AgeRating:
                        TypeManager = new TypeAgeRating();
                        break;

                    case FieldType.Category:
                        TypeManager = new TypeCategory();
                        break;

                    case FieldType.Developer:
                        TypeManager = new TypeDeveloper();
                        break;

                    case FieldType.Feature:
                        TypeManager = new TypeFeature();
                        break;

                    case FieldType.Genre:
                        TypeManager = new TypeGenre();
                        break;

                    case FieldType.Platform:
                        TypeManager = new TypePlatform();
                        break;

                    case FieldType.Publisher:
                        TypeManager = new TypePublisher();
                        break;

                    case FieldType.Series:
                        TypeManager = new TypeSeries();
                        break;

                    case FieldType.Source:
                        TypeManager = new TypeSource();
                        break;

                    case FieldType.Tag:
                        TypeManager = new TypeTag();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        [DontSerialize]
        public string TypeAndName => Name.Any() ? $"{_type.GetEnumDisplayName()}: {Name}" : _type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeAsString => _type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeLabel => _type.GetEnumDisplayName();

        [DontSerialize]
        public IDatabaseObjectType TypeManager
        { get => _typeManager; set => SetValue(ref _typeManager, value); }

        public Guid AddToDb()
        {
            Id = TypeManager.AddDbObject(Name);

            return Id;
        }

        public bool AddToGame(Game game) => TypeManager.AddDbObjectToGame(game, Id);

        public void CheckGroup(List<MetadataObject> metadataList)
            => ShowGrouped = metadataList.Any(x => x.CleanedUpName == CleanedUpName && !x.Equals(this));

        public bool ExistsInDb() => TypeManager.DbObjectExists(Name);

        public bool ExistsInGame(Game game) => TypeManager.DbObjectInGame(game, Id);

        public void GetGameCount() => GameCount = TypeManager.GetGameCount(Id, _settings.IgnoreHiddenGamesInGameCount);

        public string GetPrefix()
        {
            if (_settings?.Prefixes == null)
            {
                return string.Empty;
            }

            foreach (string prefix in _settings.Prefixes)
            {
                if (Name?.StartsWith(prefix) ?? false)
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        public bool IsUsed() => TypeManager.DbObjectInUse(Id);

        public bool NameExists() => TypeManager.NameExists(Name, Id);

        public bool RemoveFromDb(bool checkIfUsed = true) => TypeManager.RemoveDbObject(Id, checkIfUsed);

        public bool RemoveFromGame(Game game) => TypeManager.RemoveObjectFromGame(game, Id);

        public IEnumerable<Guid> ReplaceInDb(List<Game> games, FieldType? newType = null, Guid? newId = null,
            bool removeAfter = true) => TypeManager.ReplaceDbObject(games, Id, newType, newId, removeAfter);

        public bool UpdateItem(string newName)
        {
            // If we don't have an id, the item is new and doesn't need to be updated.
            if (Id == default)
            {
                return true;
            }

            DbInteractionResult res = UpdateName(newName);

            if (res == DbInteractionResult.IsDuplicate)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                    Type.GetEnumDisplayName()));
            }

            return res == DbInteractionResult.Updated;
        }

        public DbInteractionResult UpdateName(string newName) =>
                    TypeManager.UpdateName(Id, Name, newName);
    }
}