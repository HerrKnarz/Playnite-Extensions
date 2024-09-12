using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeRegion : BaseListType
    {
        private readonly IItemCollection<Region> _collection = API.Instance.Database.Regions;

        public TypeRegion(bool adoptEvents = false)
        {
            if (!adoptEvents)
            {
                return;
            }

            API.Instance.Database.Regions.ItemUpdated += ItemUpdated;
        }

        public override event RenameObjectEventHandler RenameObject;

        public override bool CanBeAdded => false;

        public override bool CanBeDeleted => false;

        public override bool CanBeModified => false;

        public override int Count => _collection?.Count ?? 0;

        public override string LabelPlural => ResourceProvider.GetString("LOCRegionsLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCRegionLabel");

        public override FieldType Type => FieldType.Region;

        public override Guid AddDbObject(string name) => AddDbObject(name, _collection);

        public override bool DbObjectExists(string name) => DbObjectExists(name, _collection);

        public override bool DbObjectExists(Guid id) => DbObjectExists(id, _collection);

        public override Guid GetDbObjectId(string name) => GetDbObjectId(name, _collection);

        public override List<DatabaseObject> LoadAllMetadata() => LoadAllMetadata(_collection);

        public override List<DatabaseObject> LoadGameMetadata(Game game) => LoadGameMetadata(game.Regions);

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => LoadUnusedMetadata(ignoreHiddenGames, _collection);

        public override bool NameExists(string name, Guid id) => NameExists(name, id, _collection);

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) => RemoveDbObject(id, checkIfUsed, _collection);

        public override void UpdateDbObject(Guid id, string name) => UpdateDbObject(id, name, _collection);

        internal override List<Guid> GameGuids(Game game, bool writeable = false) =>
            writeable
                ? game.RegionIds ?? (game.RegionIds = new List<Guid>())
                : game.RegionIds ?? new List<Guid>();

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<Region> args)
        {
            if (RenameObject == null)
            {
                return;
            }

            foreach (ItemUpdateEvent<Region> item in args.UpdatedItems.Where(item => item.OldData != null && item.OldData.Name != item.NewData.Name))
            {
                RenameObject?.Invoke(this, item.OldData.Name, item.NewData.Name);
            }
        }
    }
}