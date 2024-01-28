using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class AddDefaultsAction : BaseAction
    {
        private static AddDefaultsAction _instance;
        private static readonly object _mutex = new object();

        private AddDefaultsAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage { get; } = "LOCMetadataUtilitiesDialogAddedDefaultsMessage";

        public override string ResultMessage { get; } = "LOCMetadataUtilitiesDialogAddedMessage";

        public Settings Settings { get; set; }

        public static AddDefaultsAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AddDefaultsAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            //TODO: Maybe get IDs of all items in prepare and just add them in execute, without trying to insert them every time?

            bool mustUpdate = Settings.DefaultCategories.Aggregate(false, (current, category) => current | DatabaseObjectHelper.AddDbObjectToGame(game, category.Type, category.Name));

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.TagIds?.Any() ?? false))
            {
                mustUpdate = Settings.DefaultTags.Aggregate(mustUpdate, (current, tag) => current | DatabaseObjectHelper.AddDbObjectToGame(game, tag.Type, tag.Name));
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}