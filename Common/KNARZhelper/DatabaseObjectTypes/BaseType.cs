﻿using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseType : IDatabaseObjectType
    {
        public virtual bool CanBeAdded => true;
        public virtual bool CanBeDeleted => true;
        public virtual bool CanBeEmptyInGame => true;
        public virtual bool CanBeModified => true;
        public virtual bool CanBeSetInGame => true;
        public virtual int Count => 1;
        public abstract bool IsList { get; }
        public virtual string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public virtual ItemValueType ValueType => ItemValueType.ItemList;

        public virtual Guid AddDbObject(string name) => Guid.Empty;

        public virtual bool AddDbObjectToGame(Game game, List<Guid> idList) => false;

        public virtual bool AddDbObjectToGame(Game game, Guid id) => false;

        public bool AddDbObjectToGame(Game game, string name) => AddDbObjectToGame(game, AddDbObject(name));

        public virtual bool DbObjectExists(string name) => false;

        public virtual bool DbObjectExists(Guid id) => false;

        public virtual bool DbObjectInGame(Game game, Guid id) => false;

        public virtual bool DbObjectInUse(Guid id) => false;

        public virtual void EmptyFieldInGame(Game game)
        { }

        public abstract bool FieldInGameIsEmpty(Game game);

        public virtual Guid GetDbObjectId(string name) => default;

        public virtual int GetGameCount(Guid id, bool ignoreHidden = false) => 0;

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