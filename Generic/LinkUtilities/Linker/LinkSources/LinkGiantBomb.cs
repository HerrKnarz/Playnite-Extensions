using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Giant Bomb.
    /// </summary>
    internal class LinkGiantBomb : BaseClasses.Linker
    {
        //TODO: Reactivate once GiantBomb switched to MediaWiki.
        public LinkGiantBomb()
        {
            Settings.NeedsApiKey = false;
        }

        public override LinkAddTypes AddType => LinkAddTypes.None;
        public override string BrowserSearchUrl => "https://www.giantbomb.com/search/?i=&q=";
        public override bool CanBeSearched => false;
        public override string LinkName => "Giant Bomb";
        public override string SearchUrl => "https://www.giantbomb.com/api/search/?api_key={0}&format=json&query={1}&resources=game&field_list=name,platforms,site_detail_url,original_release_date&limit=50";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return base.GetSearchResults(searchTerm);
            }

            var giantBombSearchResult = ApiHelper.GetJsonFromApi<GiantBombSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

            if (giantBombSearchResult is null || giantBombSearchResult.Error != "OK" || giantBombSearchResult.NumberOfTotalResults <= 0)
            {
                return base.GetSearchResults(searchTerm);
            }

            var searchResults = new List<GenericItemOption>();

            foreach (var result in giantBombSearchResult.Results)
            {
                var description = result.OriginalReleaseDate;

                if (result.Platforms?.Any() ?? false)
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        description += Environment.NewLine;
                    }

                    description += result.Platforms.Select(platform => platform.Name)
                        .Aggregate((total, part) => total + ", " + part);
                }

                searchResults.Add(new SearchResult
                {
                    Name = result.Name,
                    Url = $"{result.SiteDetailUrl}",
                    Description = description
                });
            }

            return searchResults;
        }
    }
}