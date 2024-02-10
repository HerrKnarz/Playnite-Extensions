using KNARZhelper;
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
        private string _name = string.Empty;
        private string _prefix = string.Empty;
        private bool _selected;
        private bool _showGrouped;
        private FieldType _type;

        public MetadataObject(Settings settings) => _settings = settings;

        [DontSerialize]
        public new Guid Id { get; set; }

        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        [DontSerialize]
        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }

        public FieldType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

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
                if (_editName != null && _editName != value)
                {
                    DbInteractionResult res = DatabaseObjectHelper.UpdateName(Type, Id, Prefix + _editName, value);

                    switch (res)
                    {
                        case DbInteractionResult.Updated:
                            _editName = value;
                            _name = Prefix + value;
                            break;
                        case DbInteractionResult.IsDuplicate:
                            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                                Type.GetEnumDisplayName()));
                            break;
                        case DbInteractionResult.Created:
                        case DbInteractionResult.Error:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    _editName = value;
                    _name = Prefix + value;
                }

                CleanedUpName = Name.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");

                OnPropertyChanged();
            }
        }

        public new string Name
        {
            get => _name;
            set
            {
                _name = value;

                _prefix = GetPrefix();

                _editName = _prefix == string.Empty ? value : value.RemoveFirst(_prefix);

                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public string Prefix
        {
            get => _prefix;
            set
            {
                SetValue(ref _prefix, value);
                _name = Prefix + EditName;
            }
        }

        [DontSerialize]
        public string TypeAndName => $"{Type.GetEnumDisplayName()}: {Name}";

        [DontSerialize]
        public string TypeLabel => Type.GetEnumDisplayName();

        [DontSerialize]
        public bool ShowGrouped
        {
            get => _showGrouped;
            set => SetValue(ref _showGrouped, value);
        }

        public string GetPrefix()
        {
            if (_settings?.Prefixes == null)
            {
                return string.Empty;
            }

            foreach (string prefix in _settings.Prefixes)
            {
                if (Name.StartsWith(prefix))
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        public void GetGameCount(bool ignoreHiddenGames = false)
        {
            switch (Type)
            {
                case FieldType.Category:
                    GameCount = API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.CategoryIds?.Any(t => t == Id) ?? false));
                    break;
                case FieldType.Feature:
                    GameCount = API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.FeatureIds?.Any(t => t == Id) ?? false));
                    break;
                case FieldType.Genre:
                    GameCount = API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.GenreIds?.Any(t => t == Id) ?? false));
                    break;
                case FieldType.Series:
                    GameCount = API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.SeriesIds?.Any(t => t == Id) ?? false));
                    break;
                case FieldType.Tag:
                    GameCount = API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.TagIds?.Any(t => t == Id) ?? false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CheckGroup(List<MetadataObject> metadataList)
            => ShowGrouped = metadataList.Any(x => x.CleanedUpName == CleanedUpName && !x.Equals(this));
    }
}