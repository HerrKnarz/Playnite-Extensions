using System;
using System.Collections.Generic;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IObjectType : IFieldType
    {
        int Count { get; }
        bool IsList { get; }

        bool DbObjectExists(string name);

        bool DbObjectExists(Guid id);

        bool DbObjectInGame(Game game, Guid id);

        bool DbObjectInUse(Guid id);

        Guid GetDbObjectId(string name);

        int GetGameCount(Guid id, bool ignoreHidden = false);

        int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false);

        List<Game> GetGames(Guid id, bool ignoreHidden = false);

        List<DatabaseObject> LoadAllMetadata();

        List<DatabaseObject> LoadGameMetadata(Game game);

        List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        bool NameExists(string name, Guid id);
    }
}