using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.GiantBomb;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Giant Bomb.
    /// </summary>
    internal class LinkGiantBomb : BaseClasses.Linker
    {
        public override string LinkName => "Giant Bomb";
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string SearchUrl => "https://www.giantbomb.com/api/search/?api_key={0}&format=json&query={1}&resources=game&field_list=name,platforms,site_detail_url,original_release_date&limit=50";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return base.GetSearchResults(searchTerm);
            }

            GiantBombSearchResult giantBombSearchResult = ParseHelper.GetJsonFromApi<GiantBombSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

            if (giantBombSearchResult is null || giantBombSearchResult.Error != "OK" || giantBombSearchResult.NumberOfTotalResults <= 0)
            {
                return base.GetSearchResults(searchTerm);
            }

            List<GenericItemOption> searchResults = new List<GenericItemOption>();

            foreach (Result result in giantBombSearchResult.Results)
            {
                string description = result.OriginalReleaseDate;

                if (result.Platforms?.Any() ?? false)
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        description += Environment.NewLine;
                    }

                    description += result.Platforms.Select(platform => platform.Name).Aggregate((total, part) => total + ", " + part);
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

        public LinkGiantBomb() => Settings.NeedsApiKey = true;
    }
}