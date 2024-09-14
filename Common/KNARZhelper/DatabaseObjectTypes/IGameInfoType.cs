using System;
using System.Collections.Generic;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IGameInfoType
    {
        int GetGameCount(Guid id, bool ignoreHidden = false);

        int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false);

        List<Game> GetGames(Guid id, bool ignoreHidden = false);
    }
}