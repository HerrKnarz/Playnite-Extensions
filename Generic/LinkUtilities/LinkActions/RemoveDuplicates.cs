using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to remove duplicate links.
    /// </summary>
    internal class RemoveDuplicates : LinkAction
    {
        private static RemoveDuplicates _instance;
        private static readonly object _mutex = new object();
        private RemoveDuplicates() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRemoveDuplicates";

        public override string ResultMessage => "LOCLinkUtilitiesDialogRemovedMessage";

        public bool RemoveDuplicatesAfterChange { get; set; } = false;

        public DuplicateTypes RemoveDuplicatesType { get; set; } = DuplicateTypes.NameAndUrl;

        public static RemoveDuplicates Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new RemoveDuplicates();
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               LinkHelper.RemoveDuplicateLinks(game, RemoveDuplicatesType);

        public static void ShowReviewDuplicatesView(IEnumerable<Game> games)
        {
            try
            {
                IEnumerable<Game> enumerable = games.ToList();
                ReviewDuplicatesViewModel viewModel = new ReviewDuplicatesViewModel(enumerable);

                if (!viewModel.ReviewDuplicates.Duplicates?.Any() ?? false)
                {
                    API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCLinkUtilitiesDialogNoDuplicatesFound"));
                    return;
                }

                Window window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesReviewDuplicatesWindowName"));
                ReviewDuplicatesView view = new ReviewDuplicatesView { DataContext = viewModel };

                window.Content = view;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing ReviewDuplicatesView", true);
            }
        }
    }
}