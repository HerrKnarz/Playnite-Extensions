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
        private readonly PluginSettings _settings;
        private string _wikiNameVideoGame;
        private string _wikiName;
        private string _wikiStart;

        public GameFinder(PluginSettings settings)
        {
            _settings = settings;
        }

        private void PrepareSearchTerms(string searchTerm)
        {
            _wikiNameVideoGame = (searchTerm + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
            _wikiName = searchTerm.RemoveSpecialChars().ToLower().Replace(" ", "");
            _wikiStart = _wikiName.Substring(0, (_wikiName.Length > 5) ? 5 : _wikiName.Length);
        }

        /// <summary>
        /// Tries to find a single game based on the given name.
        /// </summary>
        /// <param name="gameName">Name of the game to find</param>
        /// <returns>Found game as a json result. Returns null if no confident single result was found.</returns>
        public Page FindGame(string gameName)
        {
            Page foundPage = null;

            PrepareSearchTerms(gameName);

            string searchName = gameName.RemoveEditionSuffix();

            // We search for the game name on Wikipedia
            WikipediaSearchResult searchResult = ApiCaller.GetSearchResults(searchName);

            string searchNameVideoGame = (searchName + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
            searchName = searchName.RemoveSpecialChars().ToLower().Replace(" ", "");

            if (searchResult.Pages != null && searchResult.Pages.Count > 0)
            {
                // Since name games have names, that aren't exclusive to video games, often "(video game)" is added to the
                // page title, so we try that first, before searching the name itself. Only if we get a 100% match, we'll
                // use the page in background mode. The description also needs to have the words "video game" in it to
                // avoid cases like "Doom", where a completely wrong page would be returned.
                foundPage =
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == _wikiNameVideoGame).FirstOrDefault() ??
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == searchNameVideoGame).FirstOrDefault() ??
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == _wikiName).FirstOrDefault() ??
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == searchName).FirstOrDefault();
            }
            return foundPage;
        }

        /// <summary>
        /// Searches for a game on Wikipedia and returns a list of results
        /// </summary>
        /// <param name="searchTerm">Term to search for</param>
        /// <returns>List of found results</returns>
        public List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            PrepareSearchTerms(searchTerm);

            // We search for the game name on Wikipedia
            WikipediaSearchResult searchResult = ApiCaller.GetSearchResults(searchTerm);

            if (_settings.AdvancedSearchResultSorting)
            {
                // When displaying the search results, we order them differently to hopefully get the actual game as one
                // of the first results. First we order by containing "video game" in the short description, then by
                // titles starting with the game name, then by titles starting with the first five characters of the game
                // name and at last by page title itself.
                return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult)
                        .OrderByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(_wikiNameVideoGame))
                        .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(_wikiStart))
                        .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").Contains(_wikiName))
                        .ThenByDescending(o => o.Description != null && o.Description.Contains("video game"))
                        .ToList<GenericItemOption>();
            }
            else
            {
                return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult).ToList<GenericItemOption>();
            }
        }
    }
}
