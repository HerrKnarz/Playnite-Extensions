using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IClearAbleType
    {
        void EmptyFieldInGame(Game game);

        bool FieldInGameIsEmpty(Game game);
    }
}