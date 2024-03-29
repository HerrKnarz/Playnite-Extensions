﻿using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Adds a link to the game store page of the library (e.g. steam or gog) the game is part of.
    /// </summary>
    internal class AddLibraryLinks : LinkAction
    {
        private static AddLibraryLinks _instance = null;
        private static readonly object _mutex = new object();

        private AddLibraryLinks() => Libraries = new Libraries();

        public static AddLibraryLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AddLibraryLinks();
                }
            }

            return _instance;
        }

        /// <summary>
        /// contains all game Libraries that have a link to a store page that can be added.
        /// </summary>
        public readonly Libraries Libraries;

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressLibraryLink";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               Libraries.TryGetValue(game.PluginId, out LibraryLink lib) &&
               lib.FindLibraryLink(game, out List<Link> links) &&
               LinkHelper.AddLinks(game, links);
    }
}