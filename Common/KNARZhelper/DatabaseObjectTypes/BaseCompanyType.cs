using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseCompanyType : BaseListType
    {
        private readonly IItemCollection<Company> _collection = API.Instance.Database.Companies;

        protected BaseCompanyType(bool adoptEvents = false)
        {
            if (!adoptEvents)
            {
                return;
            }

            API.Instance.Database.Companies.ItemUpdated += ItemUpdated;
        }

        public override event RenameObjectEventHandler RenameObject;

        // The field isn't really read only, but since developers and publishers both use the
        // metadata company we won't support modifying or deleting them for now. Adding new ones and
        // adding/removing from games is fine.
        public override bool CanBeAdded => false;

        public override bool CanBeDeleted => false;

        public override bool CanBeModified => false;

        public override int Count => _collection?.Count ?? 0;

        public abstract override string LabelSingular { get; }

        public abstract override FieldType Type { get; }

        public override Guid AddDbObject(string name) => AddDbObject(name, _collection);

        public override bool DbObjectExists(string name) => DbObjectExists(name, _collection);

        public override Guid GetDbObjectId(string name) => GetDbObjectId(name, _collection);

        public override List<DatabaseObject> LoadAllMetadata() => LoadAllMetadata(_collection);

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) =>
            _collection.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.DeveloperIds?.Contains(x.Id) ?? false) && (g.PublisherIds?.Contains(x.Id) ?? false)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) => NameExists(name, id, _collection);

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) => false;

        public override void UpdateDbObject(Guid id, string name)
        {
        }

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<Company> args)
        {
            if (RenameObject == null)
            {
                return;
            }

            foreach (ItemUpdateEvent<Company> item in args.UpdatedItems.Where(item => item.OldData != null && item.OldData.Name != item.NewData.Name))
            {
                RenameObject?.Invoke(this, item.OldData.Name, item.NewData.Name);
            }
        }
    }
}