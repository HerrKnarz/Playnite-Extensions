using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinkUtilities.BaseClasses
{
    /// <summary>
    ///     Base class for a website link
    /// </summary>
    public abstract class Linker : ILinker, ILinkAction
    {
        protected Linker() => Settings = new LinkSourceSetting
        {
            LinkName = LinkName,
            IsAddable = AddType != LinkAddTypes.None ? true : (bool?)null,
            IsSearchable = CanBeSearched ? true : (bool?)null,
            ShowInMenus = true,
            ApiKey = null,
            NeedsApiKey = false
        };

        public virtual string WrongTitle { get; set; } = string.Empty;

        public string ProgressMessage => "LOCLinkUtilitiesProgressLink";
        public string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public virtual bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => true;

        public virtual bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!isBulkAction)
            {
                if (!Prepare(actionModifier, false))
                {
                    return false;
                }
            }

            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                case ActionModifierTypes.AddSelected:
                    return AddLink(game);
                case ActionModifierTypes.Search:
                case ActionModifierTypes.SearchSelected:
                    return AddSearchedLink(game);
                case ActionModifierTypes.SearchMissing:
                    return AddSearchedLink(game, true);
                case ActionModifierTypes.SearchInBrowser:
                    StartBrowserSearch(game);
                    return true;
                case ActionModifierTypes.None:
                case ActionModifierTypes.Name:
                case ActionModifierTypes.SortOrder:
                default:
                    return false;
            }
        }

        public abstract string LinkName { get; }
        public virtual string BaseUrl => string.Empty;
        public virtual string SearchUrl => string.Empty;
        public virtual string BrowserSearchUrl => SearchUrl;
        public virtual string LinkUrl { get; set; } = string.Empty;
        public virtual LinkAddTypes AddType => LinkAddTypes.UrlMatch;
        public virtual bool CanBeSearched => !string.IsNullOrWhiteSpace(SearchUrl);
        public virtual bool CanBeBrowserSearched => !string.IsNullOrWhiteSpace(BrowserSearchUrl);
        public LinkSourceSetting Settings { get; set; }
        public virtual bool AllowRedirects { get; set; } = true;
        public virtual bool ReturnsSameUrl { get; set; } = false;

        public virtual bool AddSearchedLink(Game game, bool skipExistingLinks = false, bool cleanUpAfterAdding = true)
        {
            if (skipExistingLinks && LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            GenericItemOption result = GlobalSettings.Instance().OnlyATest
                ? GetSearchResults(game.Name)?.FirstOrDefault() ?? new SearchResult()
                : API.Instance.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    GetSearchResults,
                    game.Name,
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            return result != null && AddLinkFromSearch(game, (SearchResult)result, cleanUpAfterAdding);
        }

        public virtual List<GenericItemOption> GetSearchResults(string searchTerm) => new List<GenericItemOption>();

        public virtual bool AddLink(Game game) => FindLinks(game, out List<Link> links) && LinkHelper.AddLinks(game, links);

        public virtual bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            switch (AddType)
            {
                case LinkAddTypes.SingleSearchResult:
                    LinkUrl = GetGamePath(game);
                    break;
                case LinkAddTypes.UrlMatch:
                    string gameName = GetGamePath(game);

                    if (!string.IsNullOrEmpty(gameName))
                    {
                        if (CheckLink($"{BaseUrl}{gameName}"))
                        {
                            LinkUrl = $"{BaseUrl}{gameName}";
                        }
                        else
                        {
                            string baseName = game.Name.RemoveEditionSuffix();

                            if (baseName != game.Name)
                            {
                                gameName = GetGamePath(game, baseName);

                                if (CheckLink($"{BaseUrl}{gameName}"))
                                {
                                    LinkUrl = $"{BaseUrl}{gameName}";
                                }
                            }
                        }
                    }

                    break;
                case LinkAddTypes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrEmpty(LinkUrl))
            {
                return false;
            }

            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        public virtual bool CheckLink(string link) => LinkHelper.IsUrlOk(link, AllowRedirects, ReturnsSameUrl, WrongTitle);

        public virtual string GetGamePath(Game game, string gameName = null)
        {
            if (gameName is null)
            {
                gameName = game.Name;
            }

            if (!string.IsNullOrEmpty(gameName))
            {
                switch (AddType)
                {
                    case LinkAddTypes.UrlMatch:
                        return gameName;
                    case LinkAddTypes.SingleSearchResult:
                        if (CanBeSearched)
                        {
                            string baseName = gameName.RemoveEditionSuffix();

                            return baseName == gameName
                                ? TryToFindPerfectMatchingUrl(gameName) ??
                                  string.Empty
                                : TryToFindPerfectMatchingUrl(gameName) ??
                                  TryToFindPerfectMatchingUrl(baseName) ??
                                  string.Empty;
                        }

                        break;
                }
            }

            return string.Empty;
        }

        public string GetBrowserSearchLink(string searchTerm) => BrowserSearchUrl + searchTerm.UrlEncode();
        public void StartBrowserSearch(Game game) => Process.Start(GetBrowserSearchLink(game.Name));

        public virtual bool AddLinkFromSearch(Game game, SearchResult result, bool cleanUpAfterAdding = true) => LinkHelper.AddLink(game, LinkName, result.Url, false, cleanUpAfterAdding);

        /// <summary>
        ///     Searches for a game by name and looks for a matching search result.
        /// </summary>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Url of the game. Returns null if no match was found.</returns>
        private string TryToFindPerfectMatchingUrl(string gameName)
        {
            List<GenericItemOption> searchResults = GetSearchResults(gameName);

            string searchName = gameName.RemoveSpecialChars().Replace(" ", "");

            SearchResult foundGame = (SearchResult)searchResults.FirstOrDefault(r => r.Name.RemoveSpecialChars().Replace(" ", "") == searchName);

            return foundGame != null ? foundGame.Url : searchResults.Count == 1 ? ((SearchResult)searchResults[0]).Url : null;
        }
    }
}