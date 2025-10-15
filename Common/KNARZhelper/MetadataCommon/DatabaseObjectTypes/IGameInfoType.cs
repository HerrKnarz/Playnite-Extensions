using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Defines functionality for retrieving game-related information, such as game counts and game lists.
    /// </summary>
    public interface IGameInfoType
    {
        /// <summary>
        /// Retrieves the count of games associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the field value.</param>
        /// <param name="ignoreHiddenGames">If set to <see langword="true"/>, hidden games will be excluded from the count.</param>
        /// <returns>The count of games associated with the specified identifier.</returns>
        int GetGameCount(Guid id, bool ignoreHiddenGames = false);

        /// <summary>
        /// Retrieves the count of games associated with the specified identifier from a provided list of games.
        /// </summary>
        /// <param name="games">The list of games to search.</param>
        /// <param name="id">The unique identifier of the field value.</param>
        /// <param name="ignoreHiddenGames">If set to <see langword="true"/>, hidden games will be excluded from the count.</param>
        /// <returns>The count of games associated with the specified identifier.</returns>
        int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false);

        /// <summary>
        /// Retrieves a list of games associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the field value.</param>
        /// <param name="ignoreHiddenGames">If set to <see langword="true"/>, hidden games will be excluded from the list.</param>
        /// <returns>A list of games associated with the specified identifier.</returns>
        List<Game> GetGames(Guid id, bool ignoreHiddenGames = false);
    }
}