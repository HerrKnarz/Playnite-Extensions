using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    internal class RemoveUnwantedAction : BaseAction
    {
        private static RemoveUnwantedAction _instance;

        private readonly Dictionary<FieldType, ItemList> _types = new Dictionary<FieldType, ItemList>();

        private RemoveUnwantedAction()
        {
            foreach (var type in FieldTypeHelper.GetItemListTypes())
            {
                _types.Add(type.Type, new ItemList
                {
                    ObjectType = type,
                    Items = new List<Guid>()
                });
            }
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressRemovingUnwantedMessage");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogRemovedUnwantedMessage";

        public static RemoveUnwantedAction Instance() => _instance ?? (_instance = new RemoveUnwantedAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var mustUpdate = _types.Values.Aggregate(false, (current, type) =>
                current | type.ObjectType.RemoveObjectFromGame(game.Game, type.Items));

            if (mustUpdate && actionModifier != ActionModifierType.IsCombi)
            {
                _gamesAffected.Add(game.Game);
            }

            return mustUpdate;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            base.FollowUp(actionModifier, item, isBulkAction);

            foreach (var metaDataItem in Settings.UnwantedItems)
            {
                if (!metaDataItem.IsUsed())
                {
                    metaDataItem.RemoveFromDb();
                }
            }

            ClearLists();
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Prepare(actionModifier, item, isBulkAction))
            {
                return false;
            }

            ClearLists();

            if (Settings.UnwantedItems.Count == 0)
            {
                return false;
            }

            foreach (var metaDataItem in Settings.UnwantedItems)
            {
                if (_types.TryGetValue(metaDataItem.Type, out var type))
                {
                    type.Items.Add(metaDataItem.Id);
                }
            }

            return true;
        }

        private void ClearLists()
        {
            foreach (var type in _types)
            {
                type.Value.Items.Clear();
            }
        }
    }
}