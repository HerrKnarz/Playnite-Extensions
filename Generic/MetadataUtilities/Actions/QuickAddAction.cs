using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using MetadataUtilities.Enums;

namespace MetadataUtilities.Actions
{
    public class QuickAddAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static QuickAddAction _instance;
        private ActionModifierType _action = ActionModifierType.Add;
        private FieldType _type = FieldType.Category;

        private QuickAddAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage => string.Format(ResourceProvider.GetString($"LOCMetadataUtilitiesProgressQuickAdd{_action}"), ResourceProvider.GetString($"LOC{_type}Label"));

        public override string ResultMessage => $"LOCMetadataUtilitiesDialogQuickAddSuccess{_action}";

        public Settings Settings { get; set; }

        public static QuickAddAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new QuickAddAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            MetadataObject metaDataItem = (MetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            bool mustUpdate;

            switch (actionModifier)
            {
                case ActionModifierType.Add:
                    mustUpdate = metaDataItem.AddToGame(game);
                    break;

                case ActionModifierType.Remove:
                    mustUpdate = metaDataItem.RemoveFromGame(game);
                    break;

                case ActionModifierType.Toggle:
                    mustUpdate = metaDataItem.ExistsInGame(game) ?
                        metaDataItem.RemoveFromGame(game) :
                        metaDataItem.AddToGame(game);
                    break;

                case ActionModifierType.None:
                case ActionModifierType.IsManual:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            _action = actionModifier;

            MetadataObject metaDataItem = (MetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            _type = metaDataItem.Type;

            return true;
        }
    }
}