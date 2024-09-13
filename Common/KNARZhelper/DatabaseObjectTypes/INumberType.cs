using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface INumberType
    {
        bool IsBiggerThan<T>(Game game, T value);

        bool IsSmallerThan<T>(Game game, T value);
    }
}