using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.GiantBomb;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Giant Bomb.
    /// </summary>
    internal class LinkGiantBomb : Link
    {
        public override string LinkName { get; } = "Giant Bomb";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://www.giantbomb.com/api/search/?api_key={0}&format=json&query={1}&resources=game&field_list=name,platforms,site_detail_url,original_release_date&limit=50";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                try
                {
                    string apiUrl = string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode());

                    WebClient client = new WebClient();

                    client.Headers.Add("user-agent", "Playnite LinkUtilities AddOn");

                    string jsonResult = client.DownloadString(apiUrl);

                    GiantBombSearchResult giantBombSearchResult = Serialization.FromJson<GiantBombSearchResult>(jsonResult);

                    if (giantBombSearchResult.Error == "OK" && giantBombSearchResult.NumberOfTotalResults > 0)
                    {
                        foreach (Result result in giantBombSearchResult.Results)
                        {
                            string description = result.OriginalReleaseDate;


                            if (result.Platforms != null && result.Platforms.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(description))
                                {
                                    description += Environment.NewLine;
                                }

                                description += result.Platforms.Select(platform => platform.Name).
                                Aggregate((total, part) => total + ", " + part);
                            }

                            SearchResults.Add(new SearchResult
                            {
                                Name = result.Name,
                                Url = $"{result.SiteDetailUrl}",
                                Description = description
                            }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error loading data from {LinkName}");
                }
            }

            return base.SearchLink(searchTerm);
        }

        public LinkGiantBomb(LinkUtilities plugin) : base(plugin)
        {
            Settings.NeedsApiKey = true;
        }
    }
}
