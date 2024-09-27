using KNARZhelper;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    /// Provides Functionality to find a game on Wikipedia.
    /// </summary>
    internal class GameFinder
    {
        private readonly bool _useAdvancedSearchResultSorting;

        public GameFinder(bool useAdvancedSearchResultSorting) => _useAdvancedSearchResultSorting = useAdvancedSearchResultSorting;

        /// <summary>
        /// Prepares different strings for search functions
        /// </summary>
        /// <param name="searchTerm">Term to search for</param>
        /// <returns> nameVideoGame = search term with " (video game)" added
        /// compareName = search term without special characters and whitespaces to compare with results
        /// startName = the first five characters of the search term to order by those.</returns>
        private static (string nameVideoGame, string compareName, string startName) PrepareSearchTerms(string searchTerm)
        {
            var compareName = searchTerm.RemoveSpecialChars().ToLower().Replace(" ", "");

            return
            (
                $"{searchTerm} (video game)".RemoveSpecialChars().ToLower().Replace(" ", ""),
                compareName,
                new string(compareName.Take(5).ToArray())
            );
        }

        /// <summary>
        /// Tries to find a single game based on the given name.
        /// </summary>
        /// <param name="gameName">Name of the game to find</param>
        /// <returns>Found game as a json result. Returns null if no confident single result was found.</returns>
        public Page FindGame(string gameName)
        {
            var (nameVideoGame, compareName, _) = PrepareSearchTerms(gameName);

            var searchName = gameName.RemoveEditionSuffix();

            // We search for the game name on Wikipedia
            var searchResult = WikipediaApiCaller.GetSearchResults(searchName);

            var searchNameVideoGame = (searchName + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
            searchName = searchName.RemoveSpecialChars().ToLower().Replace(" ", "");

            if (!(searchResult.Pages?.Any() ?? false))
            {
                return null;
            }

            // Since name games have names, that aren't exclusive to video games, often "(video game)" is added to the
            // page title, so we try that first, before searching the name itself. Only if we get a 100% match, we'll
            // use the page in background mode. The description also needs to have the words "video game" in it to
            // avoid cases like "Doom", where a completely wrong page would be returned.
            var foundPages = searchResult.Pages
                .Where(p => p.Description != null && p.Description.ToLower().Contains("video game")).ToList();

            return foundPages.FirstOrDefault(p => p.KeyMatch == nameVideoGame) ??
                   foundPages.FirstOrDefault(p => p.KeyMatch == compareName) ??
                   foundPages.FirstOrDefault(p => p.KeyMatch == searchNameVideoGame) ??
                   foundPages.FirstOrDefault(p => p.KeyMatch == searchName);
        }

        /// <summary>
        /// Searches for a game on Wikipedia and returns a list of results
        /// </summary>
        /// <param name="searchTerm">Term to search for</param>
        /// <returns>List of found results</returns>
        public List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var (nameVideoGame, compareName, startName) = PrepareSearchTerms(searchTerm);

            // We search for the game name on Wikipedia
            var searchResult = WikipediaApiCaller.GetSearchResults(searchTerm);

            if (_useAdvancedSearchResultSorting)
            {
                // When displaying the search results, we order them differently to hopefully get the actual game as one
                // of the first results. First we order by containing "video game" in the short description, then by
                // titles starting with the game name, then by titles starting with the first five characters of the game
                // name and at last by page title itself.
                return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult)
                    .OrderByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(nameVideoGame))
                    .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(startName))
                    .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").Contains(compareName))
                    .ThenByDescending(o => o.Description != null && o.Description.Contains("video game"))
                    .ToList<GenericItemOption>();
            }

            return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult).ToList<GenericItemOption>();
        }
    }
}