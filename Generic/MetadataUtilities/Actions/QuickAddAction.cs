using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System;

namespace MetadataUtilities.Actions
{
    public class QuickAddAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static QuickAddAction _instance;
        private ActionModifierTypes _action = ActionModifierTypes.Add;
        private SettableFieldType _type = SettableFieldType.Category;

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

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            SettableMetadataObject metaDataItem = (SettableMetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            bool mustUpdate;

            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    mustUpdate = DatabaseObjectHelper.AddDbObjectToGame(game, metaDataItem.Type, metaDataItem.Id);
                    break;

                case ActionModifierTypes.Remove:
                    mustUpdate = DatabaseObjectHelper.RemoveObjectFromGame(game, metaDataItem.Type, metaDataItem.Id);
                    break;

                case ActionModifierTypes.Toggle:
                    mustUpdate = DatabaseObjectHelper.DbObjectInGame(game, (FieldType)metaDataItem.Type, metaDataItem.Id) ?
                        DatabaseObjectHelper.RemoveObjectFromGame(game, metaDataItem.Type, metaDataItem.Id) :
                        DatabaseObjectHelper.AddDbObjectToGame(game, metaDataItem.Type, metaDataItem.Id);
                    break;

                case ActionModifierTypes.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            _action = actionModifier;

            SettableMetadataObject metaDataItem = (SettableMetadataObject)item;

            if (metaDataItem == null)
            {
                return false;
            }

            _type = metaDataItem.Type;

            if (metaDataItem.Id == Guid.Empty)
            {
                metaDataItem.Id = DatabaseObjectHelper.GetDbObjectId(metaDataItem.Name, (FieldType)metaDataItem.Type);
            }

            return true;
        }
    }
}