using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseType : IDatabaseObjectType
    {
        public virtual event RenameObjectEventHandler RenameObject
        {
            add { }
            remove { }
        }

        public abstract bool CanBeAdded { get; }
        public abstract bool CanBeClearedInGame { get; }
        public abstract bool CanBeDeleted { get; }
        public abstract bool CanBeEmptyInGame { get; }
        public abstract bool CanBeModified { get; }
        public virtual bool CanBeSetByMetadataAddOn => true;
        public abstract bool CanBeSetInGame { get; }
        public virtual int Count => 1;
        public abstract bool IsList { get; }
        public virtual string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public virtual ItemValueType ValueType => ItemValueType.ItemList;

        public virtual Guid AddDbObject(string name) => Guid.Empty;

        public virtual bool AddDbObjectToGame(Game game, List<Guid> idList) => false;

        public virtual bool AddDbObjectToGame(Game game, Guid id) => false;

        public virtual bool AddValueToGame<T>(Game game, T value) => false;

        public virtual bool DbObjectExists(string name) => false;

        public virtual bool DbObjectExists(Guid id) => false;

        public virtual bool DbObjectInGame(Game game, Guid id) => false;

        public virtual bool DbObjectInUse(Guid id) => false;

        public virtual void EmptyFieldInGame(Game game)
        { }

        public virtual bool FieldInGameIsEmpty(Game game) => false;

        public virtual Guid GetDbObjectId(string name) => default;

        public virtual int GetGameCount(Guid id, bool ignoreHidden = false) => 0;

        public virtual int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) => 0;

        public virtual List<Game> GetGames(Guid id, bool ignoreHidden = false) => new List<Game>();

        public virtual bool IsBiggerThan<T>(Game game, T value) => false;

        public virtual bool IsSmallerThan<T>(Game game, T value) => false;

        public virtual List<DatabaseObject> LoadAllMetadata() => new List<DatabaseObject>();

        public virtual List<DatabaseObject> LoadGameMetadata(Game game) => new List<DatabaseObject>();

        public virtual List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => new List<DatabaseObject>();

        public virtual bool NameExists(string name, Guid id) => false;

        public virtual bool RemoveDbObject(Guid id, bool checkIfUsed = true) => false;

        public virtual bool RemoveObjectFromGame(Game game, List<Guid> ids) => false;

        public virtual bool RemoveObjectFromGame(Game game, Guid id) => false;

        public virtual IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id,
            FieldType? newType = null, Guid? newId = null, bool removeAfter = true) => new List<Guid>();

        public virtual void UpdateDbObject(Guid id, string name)
        {
        }

        public DbInteractionResult UpdateName(Guid id, string oldName, string newName)
        {
            if (oldName != null && oldName != newName && NameExists(newName, id))
            {
                return DbInteractionResult.IsDuplicate;
            }

            UpdateDbObject(id, newName);

            return DbInteractionResult.Updated;
        }
    }
}