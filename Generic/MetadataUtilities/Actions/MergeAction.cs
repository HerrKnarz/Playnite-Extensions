using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class MergeAction : BaseAction
    {
        private static MergeAction _instance;
        private static readonly object _mutex = new object();
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly List<Guid> _tagIds = new List<Guid>();

        private MergeAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage { get; } = "LOCMetadataUtilitiesDialogMergingItems";

        public override string ResultMessage { get; } = "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public Settings Settings { get; set; }

        public static MergeAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new MergeAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            // We refresh the id of all items, so we always have the actual existing ones and can merge them easier.
            foreach (MergeRule mergeRule in Settings.MergeRules)
            {
                mergeRule.Id = DatabaseObjectHelper.AddDbObject(mergeRule.Type, mergeRule.Name);

                foreach (MetadataListObject item in mergeRule.SourceObjects)
                {
                    item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);
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

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"{game.Name} START =================================");
                if (game.Categories?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Categories = {string.Join(",", game.Categories.Select(x => x.Name))}");
                }

                if (game.Features?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Features = {string.Join(",", game.Features.Select(x => x.Name))}");
                }

                if (game.Genres?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Genres = {string.Join(",", game.Genres.Select(x => x.Name))}");
                }

                if (game.Series?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Series = {string.Join(",", game.Series.Select(x => x.Name))}");
                }

                if (game.Tags?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Tags = {string.Join(",", game.Tags.Select(x => x.Name))}");
                }
            }

            bool mustUpdate = false;

            foreach (MergeRule mergeRule in Settings.MergeRules.Where(x => x.SourceObjects.Any(i => i.Id != x.Id && i.Id != Guid.Empty)).ToList())
            {
                bool needToAdd = false;

                foreach (MetadataListObject item in mergeRule.SourceObjects)
                {
                    if (item.Id != mergeRule.Id && item.Id != Guid.Empty)
                    {
                        needToAdd |= DatabaseObjectHelper.RemoveObjectFromGame(game, item.Type, item.Id);
                    }
                }

                if (needToAdd)
                {
                    DatabaseObjectHelper.AddDbObjectToGame(game, mergeRule.Type, mergeRule.Id);
                }

                mustUpdate |= needToAdd;
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"{game.Name} END =================================");
                if (game.Categories?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Categories = {string.Join(",", game.Categories.Select(x => x.Name))}");
                }

                if (game.Features?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Features = {string.Join(",", game.Features.Select(x => x.Name))}");
                }

                if (game.Genres?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Genres = {string.Join(",", game.Genres.Select(x => x.Name))}");
                }

                if (game.Series?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Series = {string.Join(",", game.Series.Select(x => x.Name))}");
                }

                if (game.Tags?.Any() ?? false)
                {
                    Log.Debug($"{game.Name}: Tags = {string.Join(",", game.Tags.Select(x => x.Name))}");
                }
            }

            return mustUpdate;
        }
    }
}