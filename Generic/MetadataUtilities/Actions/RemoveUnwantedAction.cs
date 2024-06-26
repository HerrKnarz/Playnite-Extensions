﻿using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    internal class RemoveUnwantedAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static RemoveUnwantedAction _instance;
        private readonly List<Guid> _ageRatingIds = new List<Guid>();
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly List<Guid> _featureIds = new List<Guid>();
        private readonly List<Guid> _genreIds = new List<Guid>();
        private readonly List<Guid> _seriesIds = new List<Guid>();
        private readonly List<Guid> _tagIds = new List<Guid>();

        private RemoveUnwantedAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressRemovingUnwantedMessage");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogRemovedUnwantedMessage";

        public Settings Settings { get; set; }

        public static RemoveUnwantedAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new RemoveUnwantedAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.AgeRating, _ageRatingIds);
            mustUpdate |= DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Category, _categoryIds);
            mustUpdate |= DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Feature, _featureIds);
            mustUpdate |= DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Genre, _genreIds);
            mustUpdate |= DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Series, _seriesIds);
            mustUpdate |= DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Tag, _tagIds);

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        public override void FollowUp(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            foreach (MetadataObject metaDataItem in Settings.UnwantedItems)
            {
                if (!DatabaseObjectHelper.DbObjectInUse(metaDataItem.Type, metaDataItem.Id))
                {
                    DatabaseObjectHelper.RemoveDbObject(metaDataItem.Type, metaDataItem.Id);
                }
            }
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            _ageRatingIds.Clear();
            _categoryIds.Clear();
            _featureIds.Clear();
            _genreIds.Clear();
            _seriesIds.Clear();
            _tagIds.Clear();

            if (!Settings.UnwantedItems.Any())
            {
                return false;
            }

            foreach (MetadataObject metaDataItem in Settings.UnwantedItems)
            {
                metaDataItem.Id = DatabaseObjectHelper.GetDbObjectId(metaDataItem.Name, metaDataItem.Type);

                switch (metaDataItem.Type)
                {
                    case FieldType.AgeRating:
                        _ageRatingIds.Add(metaDataItem.Id);
                        break;

                    case FieldType.Category:
                        _categoryIds.Add(metaDataItem.Id);
                        break;

                    case FieldType.Feature:
                        _featureIds.Add(metaDataItem.Id);
                        break;

                    case FieldType.Genre:
                        _genreIds.Add(metaDataItem.Id);
                        break;

                    case FieldType.Series:
                        _seriesIds.Add(metaDataItem.Id);
                        break;

                    case FieldType.Tag:
                        _tagIds.Add(metaDataItem.Id);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }
    }
}