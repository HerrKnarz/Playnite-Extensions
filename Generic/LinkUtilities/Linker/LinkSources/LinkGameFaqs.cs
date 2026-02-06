using KNARZhelper;
using KNARZhelper.WebCommon;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using LinkUtilities.Settings;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker.LinkSources
{
    internal class LinkGameFaqs : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://gamefaqs.gamespot.com/";
        public override string BrowserSearchUrl => $"{BaseUrl}search?game=";
        public override string LinkName => "GameFAQs";
        public override string SearchUrl => $"{BaseUrl}ajax/home_game_search?term=&term=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var searchResults = LinkWorker.GetJsonFromApi<List<GameFaqsSearchResult>>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName, GlobalSettings.Instance().DebugMode)
                    .Where(n => n.GameName?.Length > 0).ToList();

                return !searchResults?.Any() ?? true
                    ? base.GetSearchResults(searchTerm)
                    : new List<GenericItemOption>(searchResults.Select(n => new SearchResult
                    {
                        Name = n.GameName,
                        Url = $"{BaseUrl}{n.Url}",
                        Description = n.DateReleased
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