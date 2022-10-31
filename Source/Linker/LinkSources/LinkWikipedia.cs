using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.MediaWiki;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Wikipedia.
    /// </summary>
    class LinkWikipedia : Link
    {
        public override string LinkName { get; } = "Wikipedia";
        public override string BaseUrl { get; } = "https://en.wikipedia.org/wiki/";
        public override string SearchUrl { get; } = "https://en.wikipedia.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override string GetGamePath(Game game)
        {
            // Wikipedia Links need the game simply encoded.
            return game.Name.EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Accept", "application/xml");

                string xml = client.DownloadString(string.Format(SearchUrl, searchTerm.UrlEncode()));

                SearchSuggestion searchResults = xml.ParseXML<SearchSuggestion>();

                int counter = 0;

                foreach (SearchSuggestionItem item in searchResults.Section)
                {
                    counter++;

                    SearchResults.Add(new SearchResult
                    {
                        Name = $"{counter}. {item.Text.Value}",
                        Url = item.Url.Value,
                        Description = ""
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkWikipedia(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}