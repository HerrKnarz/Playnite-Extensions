using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    public class SortLinks : ILinkAction
    {
        public string ProgressMessage { get; } = "LOCLinkUtilitiesLSortLinksProgress";
        public string ResultMessage { get; } = "LOCLinkUtilitiesSortedMessage";
        public LinkUtilitiesSettings Settings { get; set; }

        public SortLinks(LinkUtilitiesSettings settings)
        {
            Settings = settings;
        }

        public bool Execute(Game game, string actionModifier = "")
        {
            return LinkHelper.SortLinks(game);
        }
    }
}
