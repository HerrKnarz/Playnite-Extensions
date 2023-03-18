using KNARZhelper;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    /// Provides Functionality to find a game on Wikipedia.
    /// </summary>
    public class GameFinder
    {
        private readonly PluginSettings settings;
        private string wikiNameVideoGame;
        private string wikiName;
        private string wikiStart;

        public GameFinder(PluginSettings settings)
        {
            this.settings = settings;
        }

        internal void PrepareSearchTerms(string searchTerm)
        {
            wikiNameVideoGame = (searchTerm + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
            wikiName = searchTerm.RemoveSpecialChars().ToLower().Replace(" ", "");
            wikiStart = wikiName.Substring(0, (wikiName.Length > 5) ? 5 : wikiName.Length);
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

            RegexOptions regExOptions = RegexOptions.ExplicitCapture;
            regExOptions |= RegexOptions.Compiled;
            Regex ignoredEndWordsRegex = new Regex(@"(\s*[:-])?(\s+([a-z']+\s+(edition|cut)|hd|collection|remaster(ed)?|remake|ultimate|anthology|game of the))+$", regExOptions | RegexOptions.IgnoreCase);
            Match match = ignoredEndWordsRegex.Match(gameName);
            string searchName = gameName.Remove(match.Index).Trim();

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
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == wikiNameVideoGame).FirstOrDefault() ??
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == searchNameVideoGame).FirstOrDefault() ??
                    searchResult.Pages.Where(p => p.Description != null && p.Description.ToLower().Contains("video game") && p.KeyMatch == wikiName).FirstOrDefault() ??
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

            if (settings.AdvancedSearchResultSorting)
            {
                // When displaying the search results, we order them differently to hopefully get the actual game as one
                // of the first results. First we order by containing "video game" in the short description, then by
                // titles starting with the game name, then by titles starting with the first five characters of the game
                // name and at last by page title itself.
                return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult)
                        .OrderByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiNameVideoGame))
                        .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiStart))
                        .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").Contains(wikiName))
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
