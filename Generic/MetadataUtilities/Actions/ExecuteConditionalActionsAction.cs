using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class ExecuteConditionalActionsAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static ExecuteConditionalActionsAction _instance;

        private ExecuteConditionalActionsAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressExecutingConditionalActions");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogExecutedConditionalActions";

        public Settings Settings { get; set; }

        public static ExecuteConditionalActionsAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new ExecuteConditionalActionsAction(plugin);
                }
            }

            return _instance;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "easier to understand")]
        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            if (item != null)
            {
                return ((ConditionalAction)item).CheckAndExecute(game.Game, actionModifier == ActionModifierType.IsManual);
            }

            return Settings.ConditionalActions.Where(x => x.Enabled).Aggregate(false,
                (current, conditionalAction) => current | conditionalAction.CheckAndExecute(game.Game, actionModifier == ActionModifierType.IsManual));
        }
    }
}