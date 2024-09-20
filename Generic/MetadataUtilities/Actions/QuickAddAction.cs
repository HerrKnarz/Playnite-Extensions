using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;

namespace MetadataUtilities.Actions
{
    public class QuickAddAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static QuickAddAction _instance;
        private ActionModifierType _action = ActionModifierType.Add;
        private FieldType _type = FieldType.Category;

        private QuickAddAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => string.Format(ResourceProvider.GetString($"LOCMetadataUtilitiesProgressQuickAdd{_action}"), ResourceProvider.GetString($"LOC{_type}Label"));

        public override string ResultMessage => $"LOCMetadataUtilitiesDialogQuickAddSuccess{_action}";

        public static QuickAddAction Instance(Settings settings)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new QuickAddAction(settings);
                }
            }

            return _instance;
        }

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var metaDataItem = (MetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            bool mustUpdate;

            switch (actionModifier)
            {
                case ActionModifierType.Add:
                    mustUpdate = metaDataItem.AddToGame(game.Game);
                    break;

                case ActionModifierType.Remove:
                    mustUpdate = metaDataItem.RemoveFromGame(game.Game);
                    break;

                case ActionModifierType.Toggle:
                    mustUpdate = metaDataItem.ExistsInGame(game.Game) ?
                        metaDataItem.RemoveFromGame(game.Game) :
                        metaDataItem.AddToGame(game.Game);
                    break;

                case ActionModifierType.None:
                case ActionModifierType.IsManual:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game.Game);
            }

            return mustUpdate;
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            _action = actionModifier;

            var metaDataItem = (MetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            _type = metaDataItem.Type;

            return true;
        }
    }
}