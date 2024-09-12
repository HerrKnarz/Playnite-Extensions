using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseObjectType : BaseType
    {
        public override bool CanBeAdded => true;

        public override bool CanBeClearedInGame => true;

        public override bool CanBeDeleted => true;

        public override bool CanBeEmptyInGame => true;

        public override bool CanBeModified => true;

        public override bool CanBeSetByMetadataAddOn => true;

        public override bool CanBeSetInGame => true;

        public abstract override bool IsList { get; }

        public abstract override string LabelSingular { get; }

        public abstract override FieldType Type { get; }

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            GetGameCount(API.Instance.Database.Games.ToList(), id, ignoreHidden);

        internal Guid AddDbObject<T>(string name, IItemCollection<T> collection) where T : DatabaseObject => collection.Add(name).Id;

        internal bool DbObjectExists<T>(string name, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Name == name) ?? false;

        internal bool DbObjectExists<T>(Guid id, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Id == id) ?? false;

        internal Guid GetDbObjectId<T>(string name, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        internal List<DatabaseObject> LoadAllMetadata<T>(IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        internal bool NameExists<T>(string name, Guid id, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Name == name && x.Id != id) ?? false;

        internal bool RemoveDbObject<T>(Guid id, bool checkIfUsed, IItemCollection<T> collection) where T : DatabaseObject =>
            // If we need to check first, we can simply call the replace method, that removes the
            // item itself, if no item is entered to replace the old one.
            DbObjectExists(id) && (checkIfUsed
                ? ReplaceDbObject(API.Instance.Database.Games.ToList(), id)?.Count() > 0
                : API.Instance.MainView.UIDispatcher.Invoke(() => collection?.Remove(id) ?? false));

        internal void UpdateDbObject<T>(Guid id, string name, IItemCollection<T> collection) where T : DatabaseObject
        {
            T item = collection?.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return;
            }

            item.Name = name;

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                collection.Update(item);
            });
        }
    }
}