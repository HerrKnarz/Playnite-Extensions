using LinkUtilities.Linker;

using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Adds a link to the game store page of the library (e.g. steam or gog) the game is part of.
    /// </summary>
    internal class AddLibraryLinks : BaseClasses.LinkAction
    {
        private static AddLibraryLinks _instance = null;
        private static readonly object _mutex = new object();

        private AddLibraryLinks() : base() => _libraries = new Libraries();

        public static AddLibraryLinks Instance()
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new AddLibraryLinks();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// contains all game _libraries that have a link to a store page that can be added.
        /// </summary>
        private readonly Libraries _libraries;

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressLibraryLink";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            BaseClasses.LibraryLink library;
            bool result = false;

            // Find the library of the game and add a link, if possible.
            library = _libraries.Find(x => x.LibraryId == game.PluginId);

            if (library is object)
            {
                result = library.AddLibraryLink(game);
            }

            return result;
        }
    }
}
