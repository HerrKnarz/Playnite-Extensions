using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities
{
    /// <summary>
    /// Defines the way a link can be added.
    /// </summary>
    public enum LinkAddTypes
    {
        None,
        SingleSearchResult,
        UrlMatch
    }

    /// <summary>
    /// Interface for all the websites a link can be added to
    /// </summary>
    internal interface ILinker
    {
        /// <summary>
        /// Name the link will have in the games link collection
        /// </summary>
        string LinkName { get; }

        /// <summary>
        /// Base URL of the link before adding the specific path to the game itself. Only used if applicable.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// URL to use the search function of the website
        /// </summary>
        string SearchUrl { get; }

        /// <summary>
        /// The final URL for the link
        /// </summary>
        string LinkUrl { get; set; }

        /// <summary>
        /// Specifies, if and how the link can be added without a search dialog
        /// </summary>
        LinkAddTypes AddType { get; }

        /// <summary>
        /// Specifies, if the link is searchable (e.g. a search function via SearchUrl is implemented)
        /// </summary>
        bool CanBeSearched { get; }

        /// <summary>
        /// represents the settings for this specific link
        /// </summary>
        LinkSourceSetting Settings { get; set; }

        /// <summary>
        /// Specifies, if a redirect is allowed while checking the URL. Some sites redirect to the homepage if the link isn't valid.
        /// In that case this should be set to false.
        /// </summary>
        bool AllowRedirects { get; set; }

        /// <summary>
        /// Specifies, if the returned url must be the same as the searched one while checking the URL. Some sites always redirect and
        /// then return OK, even if the link isn't valid. In that case this should be set to true, if the url needs to be the same.
        /// </summary>
        bool ReturnsSameUrl { get; set; }

        /// <summary>
        /// Adds a link via search dialog.
        /// </summary>
        /// <param name="game">Game the link will be searched for and added to</param>
        /// <param name="skipExistingLinks">When true already existing links will be skipped.</param>
        /// <returns>True, if a link was added</returns>
        bool AddSearchedLink(Game game, bool skipExistingLinks = false);

        /// <summary>
        /// Searches the website and returns a list of found games via GenericItemOption. An extended list with URL is also written to
        /// the list SearchResults. Must be implemented in the derived class or the result will be an empty list.
        /// </summary>
        /// <param name="searchTerm">Term to be searched for. Is usually the name of the game.</param>
        /// <returns>List with all found games. Is an empty list in the base class.</returns>
        List<GenericItemOption> GetSearchResults(string searchTerm);

        /// <summary>
        /// Adds a link to the specific game page of the specified website.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        bool AddLink(Game game);

        /// <summary>
        /// Finds a link without user interaction
        /// </summary>
        /// <param name="game">Game the link will be found for</param>
        /// <param name="link">The found link will be returned here</param>
        /// <returns>True, if a link was found</returns>
        bool FindLink(Game game, out Link link);

        /// <summary>
        /// Checks if the link is valid.
        /// </summary>
        /// <param name="link">The link to check</param>
        /// <returns>True, if the link is valid</returns>
        bool CheckLink(string link);

        /// <summary>
        /// Determines the game path part of the link.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="gameName">string we want to search for, if it's something else than the game name</param>
        /// <returns>Path that can be added to the BaseUrl to get the full link</returns>
        string GetGamePath(Game game, string gameName = null);
    }
}