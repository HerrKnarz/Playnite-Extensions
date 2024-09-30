using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to HowLongToBeat.
    /// </summary>
    internal class LinkHowLongToBeat : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://howlongtobeat.com/game/";
        public override string BrowserSearchUrl => "https://howlongtobeat.com/?q=";
        public override string LinkName => "HowLongToBeat";
        public override string SearchUrl => "https://howlongtobeat.com/api/search";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("User-Agent", "Playnite LinkUtilities");
                httpClient.DefaultRequestHeaders.Add("origin", "https://howlongtobeat.com");
                httpClient.DefaultRequestHeaders.Add("referer", "https://howlongtobeat.com");

                var requestMessage = new HttpRequestMessage();

                var searchTerms = searchTerm.RemoveDiacritics().RemoveSpecialChars().Split(' ');

                var request = @"{""searchType"":""games"",""searchTerms"":[" +
                                 string.Join(",", searchTerms.Select(x => "\"" + x + "\"")) +
                                 @"],""searchPage"":1,""size"":20,""searchOptions"":{""games"":{""userId"":0,""platform"":"""",""sortCategory"":""popular"",""rangeCategory"":""main"",""rangeTime"":{""min"":0,""max"":0},""gameplay"":{""perspective"":"""",""flow"":"""",""genre"":""""},""modifier"":""""},""users"":{""sortCategory"":""postcount""},""filter"":"""",""sort"":0,""randomizer"":0}}";

                requestMessage.Content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync(SearchUrl, requestMessage.Content).Result;

                var jsonResult = response.Content.ReadAsStringAsync().Result;

                var searchResult = JsonConvert.DeserializeObject<HowLongToBeatSearchResult>(jsonResult);

                if (searchResult.Data?.Any() ?? false)
                {
                    return new List<GenericItemOption>(searchResult.Data.Select(d => new SearchResult
                    {
                        Name = d.GameName,
                        Url = $"{BaseUrl}{d.GameId}",
                        Description = $"{d.ReleaseWorld} {d.ProfileDev} - {d.ProfilePlatform}"
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}