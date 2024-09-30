using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using LinkUtilities.ViewModels;
using LinkUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to remove duplicate links.
    /// </summary>
    internal class RemoveDuplicates : LinkAction
    {
        private static RemoveDuplicates _instance;
        private RemoveDuplicates() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRemoveDuplicates";

        public bool RemoveDuplicatesAfterChange { get; set; } = false;

        public DuplicateTypes RemoveDuplicatesType { get; set; } = DuplicateTypes.NameAndUrl;

        public override string ResultMessage => "LOCLinkUtilitiesDialogRemovedMessage";

        public static RemoveDuplicates Instance() => _instance ?? (_instance = new RemoveDuplicates());

        public static void ShowReviewDuplicatesView(List<Game> games)
        {
            try
            {
                var viewModel = new ReviewDuplicatesViewModel(games);

                if (!viewModel.ReviewDuplicates?.Any() ?? true)
                {
                    API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCLinkUtilitiesDialogNoDuplicatesFound"));
                    return;
                }

                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesReviewDuplicatesWindowName"));
                var view = new ReviewDuplicatesView { DataContext = viewModel };

                window.Content = view;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing ReviewDuplicatesView", true);
            }
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) && RemoveDuplicateLinks(game, RemoveDuplicatesType);

        /// <summary>
        ///     Removes duplicate links from a game
        /// </summary>
        /// <param name="game">Game in which the duplicates will be removed.</param>
        /// <param name="duplicateType">Specifies, if the duplicates will be identified by name, URL or both.</param>
        /// <returns>True, if duplicates were removed. Returns false if there weren't duplicates to begin with.</returns>
        private static bool RemoveDuplicateLinks(Game game, DuplicateTypes duplicateType)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var linkCount = game.Links.Count;

            ObservableCollection<Link> newLinks;

            switch (duplicateType)
            {
                case DuplicateTypes.NameAndUrl:
                    newLinks = new ObservableCollection<Link>(game.Links
                        .GroupBy(x => new { x.Name, url = LinkHelper.CleanUpUrl(x.Url) }).Select(x => x.First()));
                    break;
                case DuplicateTypes.Name:
                    newLinks = new ObservableCollection<Link>(
                        game.Links.GroupBy(x => x.Name).Select(x => x.First()));
                    break;
                case DuplicateTypes.Url:
                    newLinks = new ObservableCollection<Link>(game.Links.GroupBy(x => LinkHelper.CleanUpUrl(x.Url))
                        .Select(x => x.First()));
                    break;
                default:
                    return false;
            }

            if (newLinks.Count >= linkCount)
            {
                return false;
            }

            game.Links = newLinks;

            if (!GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }
    }
}