﻿using LinkUtilities.BaseClasses;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to remove unwanted links based on patterns.
    /// </summary>
    internal class RemoveLinks : LinkAction
    {
        private static RemoveLinks _instance;
        private static readonly object _mutex = new object();
        private RemoveLinks() { }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressRemoveLinks";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogRemovedMessage";

        public bool RemoveLinksAfterChange { get; set; } = false;

        /// <summary>
        ///     List of patterns to find the links to delete based on URL or link name
        /// </summary>
        public LinkNamePatterns RemovePatterns { get; set; }

        public static RemoveLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new RemoveLinks();
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = false;

            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            List<Link> links = game.Links.ToList();

            foreach (Link link in links)
            {
                string linkName = link.Name;

                if (RemovePatterns.LinkMatch(ref linkName, link.Url))
                {
                    if (GlobalSettings.Instance().OnlyATest)
                    {
                        mustUpdate |= game.Links.Remove(link);
                    }
                    else
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            mustUpdate |= game.Links.Remove(link);
                        });
                    }
                }
            }

            if (mustUpdate && !GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}