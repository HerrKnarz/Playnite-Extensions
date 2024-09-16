using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
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
        private static readonly object _mutex = new object();
        private static RemoveUnwantedAction _instance;

        private readonly Dictionary<FieldType, ItemList> _types = new Dictionary<FieldType, ItemList>();

        private RemoveUnwantedAction(Settings settings) : base(settings)
        {
            foreach (IEditableObjectType type in FieldTypeHelper.GetItemListTypes())
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

        public static RemoveUnwantedAction Instance(Settings settings)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new RemoveUnwantedAction(settings);
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

            bool mustUpdate = _types.Values.Aggregate(false, (current, type) =>
                current | type.ObjectType.RemoveObjectFromGame(game.Game, type.Items));

            if (mustUpdate && actionModifier != ActionModifierType.IsCombi)
            {
                API.Instance.Database.Games.Update(game.Game);
            }

            return mustUpdate;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            foreach (MetadataObject metaDataItem in Settings.UnwantedItems)
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
            ClearLists();

            if (Settings.UnwantedItems.Count == 0)
            {
                return false;
            }

            foreach (MetadataObject metaDataItem in Settings.UnwantedItems)
            {
                if (_types.TryGetValue(metaDataItem.Type, out ItemList type))
                {
                    type.Items.Add(metaDataItem.Id);
                }
            }

            return true;
        }

        private void ClearLists()
        {
            foreach (KeyValuePair<FieldType, ItemList> type in _types)
            {
                type.Value.Items.Clear();
            }
        }
    }
}