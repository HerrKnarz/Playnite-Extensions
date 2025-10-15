using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Defines functionality to empty and check if a field in a game is empty.
    /// </summary>
    public interface IClearAbleType
    {
        /// <summary>
        /// Empties the field in the game.
        /// </summary>
        /// <param name="game">The game instance containing the field to empty. Cannot be <see langword="null"/>.</param>
        void EmptyFieldInGame(Game game);

        /// <summary>
        /// Determines whether the specified game field is empty.
        /// </summary>
        /// <param name="game">The game instance containing the field to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the field in the specified game is empty; otherwise, <see langword="false"/>.</returns>
        bool FieldInGameIsEmpty(Game game);
    }
}