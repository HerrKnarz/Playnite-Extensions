using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

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
        /// Specifys, if the link can be added without a search dialog (e.g. an add function via BaseUrl is implemented)
        /// </summary>
        bool CanBeAdded { get; }
        /// <summary>
        /// Specifys, if the link is searchable (e.g. a search function via SearchUrl is implemented)
        /// </summary>
        bool CanBeSearched { get; }
        /// <summary>
        /// represents the settings for this specific link
        /// </summary>
        LinkSourceSettings Settings { get; set; }
        /// <summary>
        /// Specifys, if a redirect is allowed while checking the url. Some sites redirect to the homepage if the link isn't valid.
        /// In that case this should be set to false.
        /// </summary>
        bool AllowRedirects { get; set; }
        /// <summary>
        /// Results of the last search for the link. Is used to get the right link after closing the search dialog, because the dialog
        /// only returns a GenericItemOption, that doesn't have an URL.
        /// </summary>
        List<SearchResult> SearchResults { get; set; }
        /// <summary>
        /// instance of the extension to access settings etc.
        /// </summary>
        LinkUtilities Plugin { get; }

        /// <summary>
        /// Adds a link via search dialog.
        /// </summary>
        /// <param name="game">Game the link will be searched for and added to</param>
        /// <returns>True, if a link was added</returns>
        bool AddSearchedLink(Game game);
        /// <summary>
        /// Searches the website and returns a list of found games via GenericItemOption. An extended list with Url is also written to
        /// the list SearchResults. Must be implemented in the derived class or the result will be an empty list.
        /// </summary>
        /// <param name="searchTerm">Term to be searched for. Is usually the name of the game.</param>
        /// <returns>List with all found games. Is an empty list in the base class.</returns>
        List<GenericItemOption> SearchLink(string searchTerm);
        /// <summary>
        /// Adds a link to the specific game page of the specified website.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        bool AddLink(Game game);
        /// <summary>
        /// Determines the game path part of the link.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>Path that can be added to the BaseUrl to get the full link</returns>
        string GetGamePath(Game game);
    }
}
