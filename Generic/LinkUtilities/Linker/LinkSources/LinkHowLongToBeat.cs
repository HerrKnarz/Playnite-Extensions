using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.HowLongToBeat;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to HowLongToBeat.
    /// </summary>
    internal class LinkHowLongToBeat : BaseClasses.Link
    {
        public override string LinkName { get; } = "HowLongToBeat";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://howlongtobeat.com/api/search";
        public override string BaseUrl { get; } = "https://howlongtobeat.com/game/";
        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("User-Agent", "Playnite LinkUtilities");
                httpClient.DefaultRequestHeaders.Add("origin", "https://howlongtobeat.com");
                httpClient.DefaultRequestHeaders.Add("referer", "https://howlongtobeat.com");

                HttpRequestMessage requestMessage = new HttpRequestMessage();

                string[] searchTerms = searchTerm.RemoveDiacritics().RemoveSpecialChars().Split(' ');

                string request = "{\"searchType\":\"games\",\"searchTerms\":[" + string.Join(",", searchTerms.Select(x => "\"" + x + "\"")) + "],\"searchPage\":1,\"size\":20,\"searchOptions\":{\"games\":{\"userId\":0,\"platform\":\"\",\"sortCategory\":\"popular\",\"rangeCategory\":\"main\",\"rangeTime\":{\"min\":0,\"max\":0},\"gameplay\":{\"perspective\":\"\",\"flow\":\"\",\"genre\":\"\"},\"modifier\":\"\"},\"users\":{\"sortCategory\":\"postcount\"},\"filter\":\"\",\"sort\":0,\"randomizer\":0}}";

                requestMessage.Content = new StringContent(request, Encoding.UTF8, "application/json");

                HttpResponseMessage response = httpClient.PostAsync(SearchUrl, requestMessage.Content).Result;

                string jsonResult = response.Content.ReadAsStringAsync().Result;

                HowLongToBeatSearchResult searchResult = JsonConvert.DeserializeObject<HowLongToBeatSearchResult>(jsonResult);

                if (searchResult.Data?.Any() ?? false)
                {
                    foreach (Datum result in searchResult.Data)
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = result.GameName,
                            Url = $"{BaseUrl}{result.GameId}",
                            Description = $"{result.ReleaseWorld} {result.ProfileDev} - {result.ProfilePlatform}"
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkHowLongToBeat() : base()
        {
        }
    }
}