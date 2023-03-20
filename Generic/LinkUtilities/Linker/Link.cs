using KNARZhelper;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
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
        public virtual bool CanBeSearched { get { return !string.IsNullOrWhiteSpace(SearchUrl); } }
        public LinkSourceSetting Settings { get; set; }
        public virtual bool AllowRedirects { get; set; } = true;
        public string ProgressMessage { get; } = "LOCLinkUtilitiesProgressLink";
        public string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        private readonly LinkUtilities _plugin;
        public LinkUtilities Plugin { get { return _plugin; } }
        public List<SearchResult> SearchResults { get; set; } = new List<SearchResult>();

        public virtual bool AddSearchedLink(Game game)
        {
            GenericItemOption result = API.Instance.Dialogs.ChooseItemWithSearch(
                new List<GenericItemOption>(),
                (a) => SearchLink(a),
                game.Name,
                $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            if (result != null)
            {
                return LinkHelper.AddLink(game, LinkName, ((SearchResult)result).Url, _plugin, false);
            }

            return false;
        }

        public virtual List<GenericItemOption> SearchLink(string searchTerm) => SearchResults.ToList<GenericItemOption>();

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
                    return LinkHelper.AddLink(game, LinkName, LinkUrl, _plugin);
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

                            if (baseName == gameName)
                            {
                                return TryToFindPerfectMatchingUrl(gameName) ??
                                    string.Empty;
                            }
                            else
                            {
                                return TryToFindPerfectMatchingUrl(gameName) ??
                                    TryToFindPerfectMatchingUrl(baseName) ??
                                    string.Empty;
                            }
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
            _ = SearchLink(gameName);

            string searchName = gameName.RemoveSpecialChars().Replace(" ", "");

            SearchResult foundGame = SearchResults.Where(r => r.Name.RemoveSpecialChars().Replace(" ", "") == searchName).FirstOrDefault();

            if (foundGame != null)
            {
                return foundGame.Url;
            }
            else if (SearchResults.Count() == 1)
            {
                return SearchResults[0].Url;
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

        public Link(LinkUtilities plugin)
        {
            this._plugin = plugin;
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
