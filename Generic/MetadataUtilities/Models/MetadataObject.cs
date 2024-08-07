﻿using KNARZhelper;
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

                if (_editName != value && UpdateItem(Name, Prefix + value))
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
        public new Guid Id { get; set; }

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

                if (_prefix == value || !UpdateItem(Name, value + EditName))
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
            set => SetValue(ref _type, value);
        }

        [DontSerialize]
        public string TypeAndName => $"{Type.GetEnumDisplayName()}: {Name}";

        [DontSerialize]
        public string TypeAsString => Type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeLabel => Type.GetEnumDisplayName();

        public void CheckGroup(List<MetadataObject> metadataList)
            => ShowGrouped = metadataList.Any(x => x.CleanedUpName == CleanedUpName && !x.Equals(this));

        public void GetGameCount()
        {
            switch (Type)
            {
                case FieldType.AgeRating:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.AgeRatingIds?.Contains(Id) ?? false));
                    break;

                case FieldType.Category:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.CategoryIds?.Contains(Id) ?? false));
                    break;

                case FieldType.Feature:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.FeatureIds?.Contains(Id) ?? false));
                    break;

                case FieldType.Genre:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.GenreIds?.Contains(Id) ?? false));
                    break;

                case FieldType.Series:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.SeriesIds?.Contains(Id) ?? false));
                    break;

                case FieldType.Tag:
                    GameCount = API.Instance.Database.Games.Count(g
                        => !(_settings.IgnoreHiddenGamesInGameCount && g.Hidden) &&
                           (g.TagIds?.Contains(Id) ?? false));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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

        public bool UpdateItem(string oldName, string newName)
        {
            // If we don't have an id, the item is new and doesn't need to be updated.
            if (Id == Guid.Empty)
            {
                return true;
            }

            DbInteractionResult res = DatabaseObjectHelper.UpdateName(Type, Id, oldName, newName);

            if (res == DbInteractionResult.IsDuplicate)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                    Type.GetEnumDisplayName()));
            }

            return res == DbInteractionResult.Updated;
        }
    }
}