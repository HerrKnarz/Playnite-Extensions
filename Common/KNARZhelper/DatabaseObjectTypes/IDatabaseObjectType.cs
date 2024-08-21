using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IDatabaseObjectType
    {
        bool CanBeAdded { get; }
        bool CanBeDeleted { get; }
        bool CanBeEmptyInGame { get; }
        bool CanBeModified { get; }
        bool CanBeSetInGame { get; }
        bool IsList { get; }
        string Label { get; }
        FieldType Type { get; }
        ItemValueType ValueType { get; }

        Guid AddDbObject(string name);

        bool AddDbObjectToGame(Game game, List<Guid> idList);

        bool AddDbObjectToGame(Game game, Guid id);

        bool AddDbObjectToGame(Game game, string name);

        bool DbObjectExists(string name);

        bool DbObjectInGame(Game game, Guid id);

        bool DbObjectInUse(Guid id);

        void EmptyFieldInGame(Game game);

        bool FieldInGameIsEmpty(Game game);

        Guid GetDbObjectId(string name);

        int GetGameCount(Guid id, bool ignoreHidden = false);

        List<DatabaseObject> LoadAllMetadata();

        List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        bool NameExists(string name, Guid id);

        bool RemoveDbObject(Guid id, bool checkIfUsed = true);

        bool RemoveObjectFromGame(Game game, List<Guid> ids);

        bool RemoveObjectFromGame(Game game, Guid id);

        IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id,
            FieldType? newType = null, Guid? newId = null, bool removeAfter = true);

        void UpdateDbObject(Guid id, string name);

        DbInteractionResult UpdateName(Guid id, string oldName, string newName);
    }
}