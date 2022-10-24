using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a website link, that's both used as an item in the Links list and 
    /// </summary>
    public abstract class Link : ILink, ILinkAction
    {
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get; } = string.Empty;
        /// <summary>
        /// URL to use the search function of the website
        /// </summary>
        public virtual string SearchUrl { get; } = string.Empty;
        /// <summary>
        /// Specifys, if the link can be added without a search dialog (e.g. an add function via BaseUrl is implemented)
        /// </summary>
        public virtual bool IsAddable { get { return !string.IsNullOrWhiteSpace(BaseUrl); } }
        /// <summary>
        /// Specifys, if the link is searchable (e.g. a search function via SearchUrl is implemented)
        /// </summary>
        public virtual bool IsSearchable { get { return !string.IsNullOrWhiteSpace(SearchUrl); } }
        public virtual string LinkUrl { get; set; } = string.Empty;
        public string ProgressMessage { get; } = "LOCLinkUtilitiesLinkProgress";
        public string ResultMessage { get; } = "LOCLinkUtilitiesAddedMessage";
        public LinkUtilitiesSettings Settings { get; set; }
        /// <summary>
        /// Results of the last search for the link. Is used to get the right link after closing the search dialog, because the dialog
        /// only returns a GenericItemOption, that doesn't have an URL.
        /// </summary>
        public List<SearchResult> SearchResults = new List<SearchResult>();

        /// <summary>
        /// Adds a link via search dialog.
        /// </summary>
        /// <param name="game">Game the link will be searched for and added to</param>
        /// <returns>True, if a link was added</returns>
        public virtual bool AddSearchedLink(Game game)
        {
            GenericItemOption result = API.Instance.Dialogs.ChooseItemWithSearch(
                new List<GenericItemOption>(),
                (a) => SearchLink(a),
                game.Name,
                ResourceProvider.GetString("LOCLinkUtilitiesSearchGame"));

            if (result != null)
            {
                return LinkHelper.AddLink(game, LinkName, SearchResults.Find(x => x.Name == result.Name).Url, Settings);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Searches the website and returns a list of found games via GenericItemOption. An extended list with Url is also written to
        /// the list SearchResults. Must be implemented in the derived class or the result will be an empty list.
        /// </summary>
        /// <param name="searchTerm">Term to be searched for. Is usually the name of the game.</param>
        /// <returns>List with all found games. Is an empty list in the base class.</returns>
        public virtual List<GenericItemOption> SearchLink(string searchTerm)
        {
            return new List<GenericItemOption>(SearchResults.Select(x => new GenericItemOption(x.Name, x.Description)));
        }

        public virtual bool AddLink(Game game)
        {
            return false;
        }

        public virtual bool Execute(Game game, string actionModifier = "")
        {
            if (actionModifier == "add")
            {
                return AddLink(game);
            }
            else
            {
                return AddSearchedLink(game);
            }
        }

        public Link(LinkUtilitiesSettings settings)
        {
            Settings = settings;
        }
    }
}
