using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IObjectType : IMetadataFieldType, IGameInfoType
    {
        int Count { get; }
        bool IsList { get; }

        bool DbObjectExists(string name);

        bool DbObjectExists(Guid id);

        bool DbObjectInGame(Game game, Guid id);

        bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false);

        Guid GetDbObjectId(string name);

        List<DatabaseObject> LoadAllMetadata(HashSet<Guid> itemsToIgnore);

        List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore = null);

        List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        bool NameExists(string name, Guid id);
    }
}