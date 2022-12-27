using LinkUtilities.Linker;
using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Adds a link to the game store page of the library (e.g. steam or gog) the game is part of.
    /// </summary>
    public class AddLibraryLinks : LinkAction
    {
        /// <summary>
        /// contains all game libraries that have a link to a store page that can be added.
        /// </summary>
        private readonly Libraries libraries;

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressLibraryLink";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        public AddLibraryLinks(LinkUtilities plugin) : base(plugin)
        {
            libraries = new Libraries(Plugin);
        }

        public override bool Execute(Game game, string actionModifier = "", bool isBulkAction = true)
        {
            LibraryLink library;
            bool result = false;

            // Find the library of the game and add a link, if possible.
            library = libraries.Find(x => x.LibraryId == game.PluginId);

            if (library is object)
            {
                result = library.AddLibraryLink(game);
            }
            return result;
        }
    }
}
