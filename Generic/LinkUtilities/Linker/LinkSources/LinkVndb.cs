using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to RAWG.io.
    /// </summary>
    internal class LinkVndb : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://vndb.org/";
        public override string BrowserSearchUrl => $"{BaseUrl}v?sq=";
        public override string LinkName => "VNDB";
        public override string SearchUrl => "https://api.vndb.org/kana/vn";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

                var searchRequest = JsonConvert.SerializeObject(new VndbSearchRequest
                {
                    Filters = new List<string> { "search", "=", searchTerm },
                    Fields = "id,title,released",
                    Results = 50,
                    Sort = "searchrank"
                }, settings);

                var vndbSearchResult = ApiHelper.GetJsonFromApi<VndbSearchResult>(SearchUrl, LinkName, null, searchRequest);

                return !vndbSearchResult?.Results?.Any() ?? true
                    ? base.GetSearchResults(searchTerm)
                    : new List<GenericItemOption>(vndbSearchResult.Results.Select(n => new SearchResult
                    {
                        Name = n.Title,
                        Url = $"{BaseUrl}{n.Id}",
                        Description = n.Released
                    }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}