using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.BaseClasses
{
    /// <summary>
    /// Base class for a website link 
    /// </summary>
    internal abstract class Link : ILink, ILinkAction
    {
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get; } = string.Empty;
        public virtual string SearchUrl { get; } = string.Empty;
        public virtual string LinkUrl { get; set; } = string.Empty;
        public virtual LinkAddTypes AddType { get; } = LinkAddTypes.UrlMatch;
        public virtual bool CanBeSearched => !string.IsNullOrWhiteSpace(SearchUrl);
        public LinkSourceSetting Settings { get; set; }
        public virtual bool AllowRedirects { get; set; } = true;
        public string ProgressMessage { get; } = "LOCLinkUtilitiesProgressLink";
        public string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        public virtual bool AddSearchedLink(Game game)
        {
            GenericItemOption result = GlobalSettings.Instance().OnlyATest
                ? GetSearchResults(game.Name)?.FirstOrDefault() ?? new SearchResult()
                : API.Instance.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    (a) => GetSearchResults(a),
                    game.Name,
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            return result != null && LinkHelper.AddLink(game, LinkName, ((SearchResult)result).Url, false);
        }

        public virtual List<GenericItemOption> GetSearchResults(string searchTerm) => new List<GenericItemOption>();

        public virtual bool AddLink(Game game)
        {
            LinkUrl = string.Empty;

            if (!LinkHelper.LinkExists(game, LinkName))
            {
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
                }

                if (!string.IsNullOrEmpty(LinkUrl))
                {
                    return LinkHelper.AddLink(game, LinkName, LinkUrl);
                }
            }

            return false;
        }

        public virtual bool CheckLink(string link) => LinkHelper.CheckUrl(link, AllowRedirects);

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

        /// <summary>
        /// Searches for a game by name and looks for a matching search result.
        /// </summary>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Url of the game. Returns null if no match was found.</returns>
        private string TryToFindPerfectMatchingUrl(string gameName)
        {
            List<GenericItemOption> searchResults = GetSearchResults(gameName);

            string searchName = gameName.RemoveSpecialChars().Replace(" ", "");

            SearchResult foundGame = (SearchResult)searchResults.FirstOrDefault(r => r.Name.RemoveSpecialChars().Replace(" ", "") == searchName);

            if (foundGame != null)
            {
                return foundGame.Url;
            }
            else if (searchResults.Count() == 1)
            {
                return ((SearchResult)searchResults[0]).Url;
            }

            return null;
        }

        public virtual bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    return AddLink(game);
                case ActionModifierTypes.Search:
                    return AddSearchedLink(game);
                default:
                    return false;
            }
        }

        public Link()
        {
            Settings = new LinkSourceSetting()
            {
                LinkName = LinkName,
                IsAddable = AddType != LinkAddTypes.None ? true : (bool?)null,
                IsSearchable = CanBeSearched ? true : (bool?)null,
                ShowInMenus = true,
                ApiKey = null,
                NeedsApiKey = false
            };
        }
    }
}
