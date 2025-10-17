using KNARZhelper.Enum;
using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Base type for object metadata fields.
    /// </summary>
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

        public virtual bool IsDefaultToCopy => true;

        public abstract bool IsList { get; }

        public abstract string LabelPlural { get; }

        public abstract string LabelSingular { get; }

        public abstract FieldType Type { get; }

        public ItemValueType ValueType => ItemValueType.ItemList;

        public abstract Guid AddDbObject(string name);

        public abstract bool AddValueToGame<T>(Game game, T value);

        public abstract bool DbObjectExists(string name);

        public abstract bool DbObjectExists(Guid id);

        public abstract bool DbObjectInGame(Game game, Guid id);

        public abstract bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false);

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is Guid id && DbObjectInGame(game, id);

        public abstract Guid GetDbObjectId(string name);

        public int GetGameCount(Guid id, bool ignoreHiddenGames = false) =>
            GetGameCount(API.Instance.Database.Games.ToList(), id, ignoreHiddenGames);

        public abstract int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false);

        public abstract List<Game> GetGames(Guid id, bool ignoreHiddenGames = false);

        public abstract List<DatabaseObject> LoadAllMetadata(HashSet<Guid> itemsToIgnore);

        public abstract List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore = null);

        public abstract List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        public abstract bool NameExists(string name, Guid id);

        public abstract bool RemoveDbObject(Guid id);

        public abstract bool RemoveObjectFromGame(Game game, List<Guid> ids);

        public abstract bool RemoveObjectFromGame(Game game, Guid id);

        public abstract IEnumerable<Guid> RemoveObjectFromGames(List<Game> games, Guid id);

        public abstract IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, IEditableObjectType newType = null, Guid? newId = null);

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

        /// <summary>
        /// Adds a database object to the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="name">Name of the database object</param>
        /// <param name="collection">Collection to add the database object to</param>
        /// <returns>ID of the added database object</returns>
        internal Guid AddDbObject<T>(string name, IItemCollection<T> collection) where T : DatabaseObject => collection.Add(name).Id;

        /// <summary>
        /// Checks if a database object with the specified name exists in the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="name">Name of the database object</param>
        /// <param name="collection">Collection to check for the the database object in</param>
        /// <returns>True if the object was found, otherwise false</returns>
        internal bool DbObjectExists<T>(string name, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Name == name) ?? false;

        /// <summary>
        /// Checks if a database object with the specified ID exists in the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="id">Id of the database object</param>
        /// <param name="collection">Collection to check for the the database object in</param>
        /// <returns>True if the object was found, otherwise false</returns>
        internal bool DbObjectExists<T>(Guid id, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Id == id) ?? false;

        /// <summary>
        /// Gets the ID of a database object with the specified name from the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="name">Name of the database object</param>
        /// <param name="collection">Collection to get the database object from</param>
        /// <returns></returns>
        internal Guid GetDbObjectId<T>(string name, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        /// <summary>
        /// Loads all metadata objects of this type from the specified collection, ignoring the ones in the ignore list.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="collection">Collection to load the metadata from</param>
        /// <param name="itemsToIgnore">List of IDs to ignore</param>
        /// <returns>List of database objects</returns>
        internal List<DatabaseObject> LoadAllMetadata<T>(IItemCollection<T> collection, HashSet<Guid> itemsToIgnore) where T : DatabaseObject =>
            collection.Where(x => !itemsToIgnore.Contains(x.Id)).Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        /// <summary>
        /// Checks if a name exists in the specified collection, excluding the object with the specified ID.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="name">Name of the database object</param>
        /// <param name="id">Id of the database object</param>
        /// <param name="collection">Collection to check for the the database object in</param>
        /// <returns>True if the object was found, otherwise false</returns>
        internal bool NameExists<T>(string name, Guid id, IItemCollection<T> collection) where T : DatabaseObject =>
            collection?.Any(x => x.Name == name && x.Id != id) ?? false;

        /// <summary>
        /// Removes a database object with the specified ID from the specified collection if it exists and is not in use.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="id">Id of the database object</param>
        /// <param name="collection">Collection to remove the database object from</param>
        /// <returns>True if the object was removed, otherwise false</returns>
        internal bool RemoveDbObject<T>(Guid id, IItemCollection<T> collection) where T : DatabaseObject =>
            DbObjectExists(id) && !DbObjectInUse(id) && API.Instance.MainView.UIDispatcher.Invoke(() => collection?.Remove(id) ?? false);

        /// <summary>
        /// Updates the name of a database object with the specified ID in the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the database object</typeparam>
        /// <param name="id">Id of the database object</param>
        /// <param name="name">New name of the database object</param>
        /// <param name="collection">Collection to update the database object in</param>
        internal void UpdateDbObject<T>(Guid id, string name, IItemCollection<T> collection) where T : DatabaseObject
        {
            var item = collection?.FirstOrDefault(x => x.Id == id);

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

        public abstract bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false);
    }
}