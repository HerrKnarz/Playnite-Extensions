using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MetadataListObject : DatabaseObject
    {
        private string _editName;
        private int _gameCount;
        private bool _selected;
        private FieldType _type;

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
        public string EditName
        {
            get => _editName;
            set
            {
                if (_editName != null && _editName != value)
                {
                    DbInteractionResult res = DatabaseObjectHelper.UpdateName(Type, Id, _editName, value);

                    switch (res)
                    {
                        case DbInteractionResult.Updated:
                            _editName = value;
                            Name = value;
                            break;
                        case DbInteractionResult.IsDuplicate:
                            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                                Type.GetEnumDisplayName("MetadataUtilities")));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    _editName = value;
                    Name = value;
                }

                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public string TypeAndName => $"{Type.GetEnumDisplayName("MetadataUtilities")}: {EditName}";

        [DontSerialize]
        public string TypeLabel => Type.GetEnumDisplayName("MetadataUtilities");

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
    }
}