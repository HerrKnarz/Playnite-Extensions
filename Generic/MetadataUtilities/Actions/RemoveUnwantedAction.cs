using KNARZhelper.Enum;
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
        private static readonly object _mutex = new object();
        private static RemoveUnwantedAction _instance;

        private readonly Dictionary<FieldType, ItemList> _types = new Dictionary<FieldType, ItemList>();

        private RemoveUnwantedAction(MetadataUtilities plugin)
        {
            Settings = plugin.Settings.Settings;

            foreach (FieldType type in FieldTypeHelper.ItemListFieldValues().Keys)
            {
                _types.Add(type, new ItemList
                {
                    ObjectType = type.GetTypeManager(),
                    Items = new List<Guid>()
                });
            }
        }

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

            bool mustUpdate = _types.Values.Aggregate(false, (current, type) =>
                current | type.ObjectType.RemoveObjectFromGame(game, type.Items));

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

            ClearLists();
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
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