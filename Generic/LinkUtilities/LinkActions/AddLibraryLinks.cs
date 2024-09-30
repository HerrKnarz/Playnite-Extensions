using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Linker;
using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Adds a link to the game store page of the library (e.g. steam or gog) the game is part of.
    /// </summary>
    internal class AddLibraryLinks : LinkAction
    {
        private static AddLibraryLinks _instance;

        /// <summary>
        /// contains all game LibraryLinks that have a link to a store page that can be added.
        /// </summary>
        public readonly LibraryLinks LibraryLinks;

        private AddLibraryLinks() => LibraryLinks = new LibraryLinks();

        public override string ProgressMessage => "LOCLinkUtilitiesProgressLibraryLink";
        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public static AddLibraryLinks Instance() => _instance ?? (_instance = new AddLibraryLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               LibraryLinks.TryGetValue(game.PluginId, out var lib) &&
               lib.FindLibraryLink(game, out var links) &&
               LinkHelper.AddLinks(game, links);
    }
}