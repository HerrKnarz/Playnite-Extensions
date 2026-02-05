using KNARZhelper;
using KNARZhelper.WebCommon;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker.Libraries
{
    /// <summary>
    /// Adds a link to Steam.
    /// </summary>
    internal class LibraryLinkSteam : LibraryLink
    {
        private const string _steamAppPrefix = "steam://openurl/";
        private const string _urlAchievements = "https://steamcommunity.com/stats/{0}/achievements";
        private const string _urlCommunity = "https://steamcommunity.com/app/{0}";
        private const string _urlDiscussion = "https://steamcommunity.com/app/{0}/discussions/";
        private const string _urlGuides = "https://steamcommunity.com/app/{0}/guides/";
        private const string _urlNews = "https://store.steampowered.com/news/?appids={0}";
        private const string _urlStorePage = "https://store.steampowered.com/app/{0}";
        private const string _urlWorkshop = "https://steamcommunity.com/app/{0}/workshop/";
        public bool AddAchievementLink { get; set; } = false;
        public bool AddCommunityLink { get; set; } = false;
        public bool AddDiscussionLink { get; set; } = false;
        public bool AddGuidesLink { get; set; } = false;
        public bool AddNewsLink { get; set; } = false;
        public bool AddStorePageLink { get; set; } = true;
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public bool AddWorkshopLink { get; set; } = false;
        public override bool AllowRedirects => false;
        public override string BrowserSearchUrl => "https://store.steampowered.com/search/?term=";

        /// <summary>
        /// ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = SteamHelper.SteamId;

        public override string LinkName => "Steam";
        public string NameAchievementLink { get; set; } = "Achievements";
        public string NameCommunityLink { get; set; } = "Community Hub";
        public string NameDiscussionLink { get; set; } = "Discussion";
        public string NameGuidesLink { get; set; } = "Guides";
        public string NameNewsLink { get; set; } = "News";
        public string NameStorePageLink { get; set; } = "Store Page";
        public string NameWorkshopLink { get; set; } = "Workshop";

        public override string SearchUrl => "https://steamcommunity.com/actions/SearchApps/";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;
        public bool UseAppLinks { get; set; } = false;

        public override bool AddLinkFromSearch(Game game, SearchResult result, bool cleanUpAfterAdding = true)
            => AddLinks(game, out var links, result.Url) && LinkHelper.AddLinks(game, links, cleanUpAfterAdding);

        public override bool FindLibraryLink(Game game, out List<Link> links) => AddLinks(game, out links, game.GameId);

        public override bool FindLinks(Game game, out List<Link> links)
        {
            if (game.PluginId == Id)
            {
                return FindLibraryLink(game, out links);
            }

            links = new List<Link>();

            var steamId = SteamHelper.GetSteamId(game);

            if (!string.IsNullOrEmpty(steamId))
            {
                return AddLinks(game, out links, steamId);
            }

            LinkUrl = string.Empty;

            LinkUrl = GetGamePath(game);

            return LinkUrl.Length > 0 && AddLinks(game, out links, LinkUrl);
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var games = new List<SteamSearchResult>();

            if (LinkWorker != null)
            {
                games = LinkWorker.GetJsonFromApi<List<SteamSearchResult>>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName, GlobalSettings.Instance().DebugMode);
            }
            else
            {
                using (LinkWorker = new LinkWorker())
                {
                    games = LinkWorker.GetJsonFromApi<List<SteamSearchResult>>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName, GlobalSettings.Instance().DebugMode);
                }
            }

            return games?.Any() ?? false
                ? new List<GenericItemOption>(games.Select(g => new SearchResult
                {
                    Name = g.Name,
                    Url = g.Appid,
                    Description = g.Appid
                }))
                : base.GetSearchResults(searchTerm);
        }

        private void AddLink(Game game, ICollection<Link> links, string gameId, string url, string name, bool checkLink = false)
        {
            if (LinkHelper.LinkExists(game, name))
            {
                return;
            }

            if (checkLink && !CheckLink($"{string.Format(url, gameId)}"))
            {
                return;
            }

            links.Add(new Link(name, $"{GetPrefix()}{string.Format(url, gameId)}"));
        }

        private bool AddLinks(Game game, out List<Link> links, string gameId)
        {
            AddWebsiteLinks.Instance().SteamId = gameId;

            links = new List<Link>();

            if (AddAchievementLink)
            {
                AddLink(game, links, gameId, _urlAchievements, NameAchievementLink);
            }

            if (AddCommunityLink)
            {
                AddLink(game, links, gameId, _urlCommunity, NameCommunityLink);
            }

            if (AddDiscussionLink)
            {
                AddLink(game, links, gameId, _urlDiscussion, NameDiscussionLink);
            }

            if (AddGuidesLink)
            {
                AddLink(game, links, gameId, _urlGuides, NameGuidesLink);
            }

            if (AddNewsLink)
            {
                AddLink(game, links, gameId, _urlNews, NameNewsLink);
            }

            if (AddStorePageLink)
            {
                AddLink(game, links, gameId, _urlStorePage, NameStorePageLink);
            }

            // TODO: Find a more reliable way to check for workshop existence - probably an API call
            if (AddWorkshopLink)
            {
                AddLink(game, links, gameId, _urlWorkshop, NameWorkshopLink, true);
            }

            if (links.Any())
            {
                return true;
            }

            AddLink(game, links, gameId, _urlStorePage, LinkName);

            return links.Any();
        }

        private string GetPrefix() => UseAppLinks ? _steamAppPrefix : string.Empty;
    }
}