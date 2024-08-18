using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;

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

            bool mustUpdate = new TypeAgeRating().RemoveObjectFromGame(game, _ageRatingIds);
            mustUpdate |= new TypeCategory().RemoveObjectFromGame(game, _categoryIds);
            mustUpdate |= new TypeFeature().RemoveObjectFromGame(game, _featureIds);
            mustUpdate |= new TypeGenre().RemoveObjectFromGame(game, _genreIds);
            mustUpdate |= new TypeSeries().RemoveObjectFromGame(game, _seriesIds);
            mustUpdate |= new TypeTag().RemoveObjectFromGame(game, _tagIds);

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
                if (!metaDataItem.IsUsed())
                {
                    metaDataItem.RemoveFromDb();
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

                    case FieldType.Developer:
                    case FieldType.Platform:
                    case FieldType.Publisher:
                    case FieldType.Source:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }
    }
}