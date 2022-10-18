using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the links of a game.
    /// </summary>
    public class SortLinks : ILinkAction
    {
        public string ProgressMessage { get; set; }
        public string ResultMessage { get; set; }
        public LinkUtilitiesSettings Settings { get; set; }

        public SortLinks(LinkUtilitiesSettings settings)
        {
            ProgressMessage = "LOCLinkUtilitiesLSortLinksProgress";
            ResultMessage = "LOCLinkUtilitiesSortedMessage";
            Settings = settings;
        }

        public bool Execute(Game game)
        {
            return LinkHelper.SortLinks(game);
        }
    }
}
