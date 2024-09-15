using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseObjectType : IEditableObjectType, IClearAbleType
    {
        public virtual event RenameObjectEventHandler RenameObject
        {
            add { }
            remove { }
        }

        public virtual bool CanBeAdded => true;

        public bool CanBeClearedInGame => true;

        public virtual bool CanBeDeleted => true;

        public bool CanBeEmptyInGame => true;

        public virtual bool CanBeModified => true;

        public virtual bool CanBeSetByMetadataAddOn => true;

        public bool CanBeSetInGame => true;

        public abstract int Count { get; }

        public abstract bool IsList { get; }

        public abstract string LabelPlural { get; }

        public abstract string LabelSingular { get; }

        public abstract FieldType Type { get; }

        public ItemValueType ValueType => ItemValueType.ItemList;

        public abstract Guid AddDbObject(string name);

        public abstract bool AddDbObjectToGame(Game game, List<Guid> idList);

        public abstract bool AddDbObjectToGame(Game game, Guid id);

        public abstract bool DbObjectExists(string name);

        public abstract bool DbObjectExists(Guid id);

        public abstract bool DbObjectInGame(Game game, Guid id);

        public abstract bool DbObjectInUse(Guid id);

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public abstract Guid GetDbObjectId(string name);

        public int GetGameCount(Guid id, bool ignoreHidden = false) =>
            GetGameCount(API.Instance.Database.Games.ToList(), id, ignoreHidden);

        public abstract int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false);

        public abstract List<Game> GetGames(Guid id, bool ignoreHidden = false);

        public abstract List<DatabaseObject> LoadAllMetadata();

        public abstract List<DatabaseObject> LoadGameMetadata(Game game);

        public abstract List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        public abstract bool NameExists(string name, Guid id);

        public abstract bool RemoveDbObject(Guid id, bool checkIfUsed = true);

        public abstract bool RemoveObjectFromGame(Game game, List<Guid> ids);

        public abstract bool RemoveObjectFromGame(Game game, Guid id);

        public abstract IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id,
            FieldType? newType = null, Guid? newId = null, bool removeAfter = true);

        public abstract void UpdateDbObject(Guid id, string name);

        public DbInteractionResult UpdateName(Guid id, string oldName, string newName)
        {
            if (oldName != null && oldName != newName && NameExists(newName, id))
            {
                return DbInteractionResult.IsDuplicate;
            }

            UpdateDbObject(id, newName);

            return DbInteractionResult.Updated;
        }

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