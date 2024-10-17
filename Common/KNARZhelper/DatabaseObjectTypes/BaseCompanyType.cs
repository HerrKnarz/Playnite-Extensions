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

        public override int Count => _collection?.Count ?? 0;

        public abstract override string LabelSingular { get; }

        public abstract override FieldType Type { get; }

        public override Guid AddDbObject(string name) => AddDbObject(name, _collection);

        public override bool DbObjectExists(string name) => DbObjectExists(name, _collection);

        public override bool DbObjectExists(Guid id) => DbObjectExists(id, _collection);

        public override bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false) => API.Instance.Database.Games.Any(
            x => !(ignoreHiddenGames && x.Hidden) &&
                 ((x.DeveloperIds?.Contains(id) ?? false) || (x.PublisherIds?.Contains(id) ?? false)));

        public override Guid GetDbObjectId(string name) => GetDbObjectId(name, _collection);

        public override List<DatabaseObject> LoadAllMetadata() => LoadAllMetadata(_collection);

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) =>
            _collection.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && ((g.DeveloperIds?.Contains(x.Id) ?? false) || (g.PublisherIds?.Contains(x.Id) ?? false))))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) => NameExists(name, id, _collection);

        public override bool RemoveDbObject(Guid id) => RemoveDbObject(id, _collection);

        public override void UpdateDbObject(Guid id, string name) => UpdateDbObject(id, name, _collection);

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<Company> args)
        {
            if (RenameObject == null)
            {
                return;
            }

            foreach (var item in args.UpdatedItems.Where(item => item.OldData != null && item.OldData.Name != item.NewData.Name))
            {
                RenameObject?.Invoke(this, item.OldData.Name, item.NewData.Name);
            }
        }
    }
}