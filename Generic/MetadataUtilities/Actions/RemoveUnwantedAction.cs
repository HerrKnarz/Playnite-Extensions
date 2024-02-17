using KNARZhelper;
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
        private static RemoveUnwantedAction _instance;
        private static readonly object _mutex = new object();
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly List<Guid> _featureIds = new List<Guid>();
        private readonly List<Guid> _genreIds = new List<Guid>();
        private readonly List<Guid> _seriesIds = new List<Guid>();
        private readonly List<Guid> _tagIds = new List<Guid>();

        private RemoveUnwantedAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage { get; } = "LOCMetadataUtilitiesProgressRemovingUnwantedMessage";

        public override string ResultMessage { get; } = "LOCMetadataUtilitiesDialogRemovedUnwantedMessage";

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

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            _categoryIds.Clear();
            _featureIds.Clear();
            _genreIds.Clear();
            _seriesIds.Clear();
            _tagIds.Clear();

            if (!Settings.UnwantedItems.Any())
            {
                return false;
            }

            foreach (MetadataObject item in Settings.UnwantedItems)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);

                switch (item.Type)
                {
                    case FieldType.Category:
                        _categoryIds.Add(item.Id);
                        break;
                    case FieldType.Feature:
                        _featureIds.Add(item.Id);
                        break;
                    case FieldType.Genre:
                        _genreIds.Add(item.Id);
                        break;
                    case FieldType.Series:
                        _seriesIds.Add(item.Id);
                        break;
                    case FieldType.Tag:
                        _tagIds.Add(item.Id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = DatabaseObjectHelper.RemoveObjectFromGame(game, FieldType.Category, _categoryIds);
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
    }
}