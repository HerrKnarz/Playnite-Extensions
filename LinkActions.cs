using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities
{
    /// <summary>
    /// Interface for classes, that can be used as a link action. Contains texts for the progress bar, result dialog and the action to
    /// execute.
    /// </summary>
    public interface ILinkAction
    {
        /// <summary>
        /// Ressource for the localized text in the progress bar
        /// </summary>
        string ProgressMessage { get; set; }
        /// <summary>
        /// Ressource for the localized text in the result dialog. Should contain placeholder for the number of affected games.
        /// </summary>
        string ResultMessage { get; set; }
        /// <summary>
        /// Settings to use for the action
        /// </summary>
        LinkUtilitiesSettings Settings { get; set; }

        /// <summary>
        /// Executes the action on a game.
        /// </summary>
        /// <param name="game">The game to be processed</param>
        /// <returns>true, if the action was successful</returns>
        bool Execute(Game game);
    }

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

    /// <summary>
    /// Adds a link to the game store page of the library (e.g. steam or gog) the game is part of.
    /// </summary>
    public class AddLibraryLinks : ILinkAction
    {
        /// <summary>
        /// contains all game libraries that have a link to a store page that can be added.
        /// </summary>
        private readonly Libraries libraries;

        public string ProgressMessage { get; set; }
        public string ResultMessage { get; set; }
        public LinkUtilitiesSettings Settings { get; set; }

        public AddLibraryLinks(LinkUtilitiesSettings settings)
        {
            ProgressMessage = "LOCLinkUtilitiesLibraryLinkProgress";
            ResultMessage = "LOCLinkUtilitiesAddedMessage";
            Settings = settings;

            libraries = new Libraries(Settings);
        }

        public bool Execute(Game game)
        {
            ILinkAssociation library;
            bool result = false;

            // Find the library of the game and add a link, if possible.
            library = libraries.Find(x => x.AssociationId == game.PluginId);

            if (library is object)
            {
                result = library.AddLink(game);
            }
            return result;
        }
    }
}
