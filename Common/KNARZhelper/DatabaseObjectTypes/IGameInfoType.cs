using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IGameInfoType
    {
        int GetGameCount(Guid id, bool ignoreHiddenGames = false);

        int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false);

        List<Game> GetGames(Guid id, bool ignoreHiddenGames = false);
    }
}