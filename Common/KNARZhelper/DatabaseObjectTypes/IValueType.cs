using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IValueType
    {
        bool AddValueToGame<T>(Game game, T value);

        bool GameContainsValue<T>(Game game, T value);
    }
}