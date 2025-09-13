using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    /// <summary>
    /// Defines functionality for number types that can be compared to other values.
    /// </summary>
    public interface INumberType
    {
        /// <summary>
        /// Checks if the value in the game is bigger than the specified value.
        /// </summary>
        /// <typeparam name="T">Type of the value to compare with.</typeparam>
        /// <param name="game">Game instance containing the value to compare.</param>
        /// <param name="value">Value to compare against.</param>
        /// <returns><see langword="true"/> if the value in the specified game is bigger than the specified value; otherwise, <see langword="false"/>.</returns>
        bool IsBiggerThan<T>(Game game, T value);

        /// <summary>
        /// Checks if the value in the game is smaller than the specified value.
        /// </summary>
        /// <typeparam name="T">Type of the value to compare with.</typeparam>
        /// <param name="game">Game instance containing the value to compare.</param>
        /// <param name="value">Value to compare against.</param>
        /// <returns><see langword="true"/> if the value in the specified game is smaller than the specified value; otherwise, <see langword="false"/>.</returns>
        bool IsSmallerThan<T>(Game game, T value);
    }
}