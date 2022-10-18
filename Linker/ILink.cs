using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Interface for all the websites a link can be added to
    /// </summary>
    interface ILink
    {
        /// <summary>
        /// Name the link will have in the games link collection
        /// </summary>
        string LinkName { get; set; }
        /// <summary>
        /// Base URL of the link before adding the specific path to the game itself. Only used if applicable.
        /// </summary>
        string LinkUrl { get; set; }
        /// <summary>
        /// Settings to be used
        /// </summary>
        LinkUtilitiesSettings Settings { get; set; }

        /// <summary>
        /// Adds a link to the specific game page of the specified website.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        bool AddLink(Game game);
    }
}
