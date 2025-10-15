using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Defines functionality for value types that can be added, copied, and checked within a game's context.
    /// </summary>
    public interface IValueType
    {
        /// <summary>
        /// Adds a value to the specified game.
        /// </summary>
        /// <remarks>If the value already exists in the game, it will not be added again, and the method will return <see langword="false"/>.
        /// For list types the value will be added to the list, in all other cases it replaces the current value in the game.</remarks>
        /// <typeparam name="T">Type of the specified value</typeparam>
        /// <param name="game">Game the value will be added in</param>
        /// <param name="value">Value to be added</param>
        /// <returns><see langword="true"/> if the value was added successfully; otherwise, <see langword="false"/>.</returns>
        bool AddValueToGame<T>(Game game, T value);

        /// <summary>
        /// Copies the value from one game to another.
        /// </summary>
        /// <param name="sourceGame">The game to copy the value from.</param>
        /// <param name="targetGame">The game to copy the value to.</param>
        /// <param name="replaceValue">Whether to replace the existing value in the target game.</param>
        /// <param name="onlyIfEmpty">Whether to only copy the value if the field in the target game is empty.</param>
        /// <returns><see langword="true"/> if the value was copied successfully; otherwise, <see langword="false"/>.</returns>
        bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false);

        /// <summary>
        /// Checks if the specified value exists in the given game.
        /// </summary>
        /// <typeparam name="T">Type of the specified value</typeparam>
        /// <param name="game">Game to check the value in</param>
        /// <param name="value">Value to check for</param>
        /// <returns><see langword="true"/> if the value exists in the game; otherwise, <see langword="false"/>.</returns>
        bool GameContainsValue<T>(Game game, T value);

        /// <summary>
        /// Defines whether the field should be copied by default when copying values between games or if it should be skipped.
        /// </summary>
        bool IsDefaultToCopy { get; }
    }
}