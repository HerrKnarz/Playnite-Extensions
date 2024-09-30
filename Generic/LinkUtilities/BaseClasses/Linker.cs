using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable VirtualMemberCallInConstructor

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

        public virtual bool AddLink(Game game) => FindLinks(game, out var links) && LinkHelper.AddLinks(game, links);

        public virtual bool AddSearchedLink(Game game, bool skipExistingLinks = false, bool cleanUpAfterAdding = true)
        {
            if (skipExistingLinks && LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var result = GlobalSettings.Instance().OnlyATest
                ? GetSearchResults(game.Name)?.FirstOrDefault() ?? new SearchResult()
                : API.Instance.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    GetSearchResults,
                    game.Name,
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            return result != null && AddLinkFromSearch(game, (SearchResult)result, cleanUpAfterAdding);
        }

        public virtual LinkAddTypes AddType => LinkAddTypes.UrlMatch;
        public virtual bool AllowRedirects { get; set; } = true;
        public virtual string BaseUrl => string.Empty;
        public virtual string BrowserSearchUrl => SearchUrl;
        public virtual bool CanBeBrowserSearched => !string.IsNullOrWhiteSpace(BrowserSearchUrl);
        public virtual bool CanBeSearched => !string.IsNullOrWhiteSpace(SearchUrl);

        public virtual bool CheckLink(string link) => LinkHelper.IsUrlOk(link, AllowRedirects, ReturnsSameUrl, WrongTitle);

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
                case ActionModifierTypes.AppLink:
                case ActionModifierTypes.DontRename:
                case ActionModifierTypes.Name:
                case ActionModifierTypes.None:
                case ActionModifierTypes.SortOrder:
                case ActionModifierTypes.WebLink:
                default:
                    return false;
            }
        }

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
                    var gameName = GetGamePath(game);

                    if (string.IsNullOrEmpty(gameName))
                    {
                        break;
                    }

                    if (!NeedsToBeChecked || CheckLink($"{BaseUrl}{gameName}"))
                    {
                        LinkUrl = $"{BaseUrl}{gameName}";
                    }
                    else
                    {
                        var baseName = game.Name.RemoveEditionSuffix();

                        if (baseName == game.Name)
                        {
                            break;
                        }

                        gameName = GetGamePath(game, baseName);

                        if (!NeedsToBeChecked || CheckLink($"{BaseUrl}{gameName}"))
                        {
                            LinkUrl = $"{BaseUrl}{gameName}";
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

        public virtual string GetBrowserSearchLink(string searchTerm) => BrowserSearchUrl + searchTerm.UrlEncode();

        public virtual string GetGamePath(Game game, string gameName = null)
        {
            if (gameName is null)
            {
                gameName = game.Name;
            }

            if (string.IsNullOrEmpty(gameName))
            {
                return string.Empty;
            }

            switch (AddType)
            {
                case LinkAddTypes.UrlMatch:
                    return gameName;
                case LinkAddTypes.SingleSearchResult:
                    if (!CanBeSearched)
                    {
                        return string.Empty;
                    }

                    var baseName = gameName.RemoveEditionSuffix();

                    return baseName == gameName
                        ? TryToFindPerfectMatchingUrl(gameName) ?? string.Empty
                        : TryToFindPerfectMatchingUrl(gameName) ??
                          TryToFindPerfectMatchingUrl(baseName) ?? string.Empty;

                case LinkAddTypes.None:
                    return string.Empty;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual List<GenericItemOption> GetSearchResults(string searchTerm) => new List<GenericItemOption>();

        public abstract string LinkName { get; }
        public virtual string LinkUrl { get; set; } = string.Empty;
        public virtual bool NeedsToBeChecked { get; set; } = true;

        public virtual bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => true;

        public string ProgressMessage => "LOCLinkUtilitiesProgressLink";
        public string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";
        public virtual bool ReturnsSameUrl { get; set; } = false;
        public virtual string SearchUrl => string.Empty;
        public LinkSourceSetting Settings { get; set; }
        public virtual void StartBrowserSearch(Game game) => Process.Start(GetBrowserSearchLink(game.Name));

        public virtual bool AddLinkFromSearch(Game game, SearchResult result, bool cleanUpAfterAdding = true) => LinkHelper.AddLink(game, LinkName, result.Url, false, cleanUpAfterAdding);

        /// <summary>
        ///     Searches for a game by name and looks for a matching search result.
        /// </summary>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Url of the game. Returns null if no match was found.</returns>
        private string TryToFindPerfectMatchingUrl(string gameName)
        {
            var searchResults = GetSearchResults(gameName);

            var searchName = gameName.RemoveSpecialChars().Replace(" ", "");

            var foundGame = (SearchResult)searchResults.FirstOrDefault(r => r.Name.RemoveSpecialChars().Replace(" ", "") == searchName);

            return foundGame != null ? foundGame.Url : searchResults.Count == 1 ? ((SearchResult)searchResults[0]).Url : null;
        }
    }
}