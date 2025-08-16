using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IValueType
    {
        bool AddValueToGame<T>(Game game, T value);

        bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false);

        bool GameContainsValue<T>(Game game, T value);

        bool IsDefaultToCopy { get; }
    }
}