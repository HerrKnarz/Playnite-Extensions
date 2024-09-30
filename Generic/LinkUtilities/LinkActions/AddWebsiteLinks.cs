using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using LinkUtilities.ViewModels;
using LinkUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to add a link to all available websites in the Links list, if a definitive link was found.
    /// </summary>
    internal class AddWebsiteLinks : LinkAction
    {
        private static AddWebsiteLinks _instance;

        private readonly List<CustomLinkProfile> _customLinkProfiles = new List<CustomLinkProfile>();

        private List<BaseClasses.Linker> _linkers;

        private AddWebsiteLinks() => Links = new Links();

        public List<CustomLinkProfile> CustomLinkProfiles
        {
            get => _customLinkProfiles;
            set
            {
                _customLinkProfiles.Clear();
                _customLinkProfiles.AddRange(value);

                Links.RefreshCustomLinkProfiles(_customLinkProfiles);
            }
        }

        public Links Links { get; }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressWebsiteLink";
        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public static AddWebsiteLinks Instance() => _instance ?? (_instance = new AddWebsiteLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                case ActionModifierTypes.AddSelected:
                    return AddLinks(game, isBulkAction);
                case ActionModifierTypes.Search:
                case ActionModifierTypes.SearchMissing:
                case ActionModifierTypes.SearchSelected:
                    return SearchLinks(game, actionModifier, isBulkAction);
                case ActionModifierTypes.SearchInBrowser:
                    return SearchLinksInBrowser(game, isBulkAction);
                case ActionModifierTypes.AppLink:
                case ActionModifierTypes.DontRename:
                case ActionModifierTypes.Name:
                case ActionModifierTypes.None:
                case ActionModifierTypes.SortOrder:
                case ActionModifierTypes.WebLink:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    _linkers = Links.Where(x => x.Settings.IsAddable == true).ToList();
                    return true;
                case ActionModifierTypes.AddSelected:
                    return SelectLinks();
                case ActionModifierTypes.Search:
                case ActionModifierTypes.SearchMissing:
                    _linkers = Links.Where(x => x.Settings.IsSearchable == true).ToList();
                    return true;
                case ActionModifierTypes.SearchInBrowser:
                    _linkers = Links.Where(x => x.CanBeBrowserSearched).ToList();
                    return true;
                case ActionModifierTypes.SearchSelected:
                    return SelectLinks(false);
                case ActionModifierTypes.AppLink:
                case ActionModifierTypes.DontRename:
                case ActionModifierTypes.Name:
                case ActionModifierTypes.None:
                case ActionModifierTypes.SortOrder:
                case ActionModifierTypes.WebLink:
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }
        }

        private bool Add(Game g, Game game) => FindLinks(game, out var links) && (links?.Any() ?? false) && LinkHelper.AddLinks(g, links);

        /// <summary>
        ///     Adds links to all configured websites
        /// </summary>
        /// <param name="game">game the links will be added to.</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if new links were added.</returns>
        private bool AddLinks(Game game, bool isBulkAction = true)
        {
            if (isBulkAction)
            {
                return Add(game, game);
            }

            var globalProgressOptions = new GlobalProgressOptions(
                $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}",
                true
            )
            {
                IsIndeterminate = true
            };

            var result = false;

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    result = Add(game, game);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            return result;
        }

        /// <summary>
        ///     Finds links to all configured websites. The links are added asynchronously to a ConcurrentBag and then returned as
        ///     a distinct list.
        /// </summary>
        /// <param name="game">game the links will be found for.</param>
        /// <param name="links">Returns a list of found links</param>
        /// <returns>True, if links were found.</returns>
        private bool FindLinks(Game game, out List<Link> links)
        {
            links = new List<Link>();

            var linksQueue = new ConcurrentQueue<Link>();

            Parallel.ForEach(_linkers, linker =>
            {
                linker.FindLinks(game, out var innerLinks);

                foreach (var innerLink in innerLinks)
                {
                    linksQueue.Enqueue(innerLink);
                }
            });

            return links.AddMissing(linksQueue.Distinct());
        }

        /// <summary>
        ///     Searches links for all configured websites
        /// </summary>
        /// <param name="game">game the links will be searched for.</param>
        /// <param name="actionModifier">Kind of search (e.g. Search or SearchMissing)</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if new links were added.</returns>
        private bool SearchLinks(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            var result = false;

            if (isBulkAction)
            {
                result = _linkers.Aggregate(false,
                    (current, link) =>
                        current | link.AddSearchedLink(game, actionModifier == ActionModifierTypes.SearchMissing,
                            false));
            }
            else
            {
                var globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}", true)
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = _linkers.Count;

                        foreach (ILinker link in _linkers)
                        {
                            activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}{Environment.NewLine}{link.LinkName}";

                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            result |= link.AddSearchedLink(game, actionModifier == ActionModifierTypes.SearchMissing, false);

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            if (result)
            {
                DoAfterChange.Instance().Execute(game, actionModifier, isBulkAction);
            }

            return result;
        }

        /// <summary>
        ///     Opens the search page for all configured websites in the standard web browser
        /// </summary>
        /// <param name="game">game the links will be searched for.</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if pages were opened.</returns>
        private bool SearchLinksInBrowser(Game game, bool isBulkAction = true)
        {
            var result = false;

            if (isBulkAction)
            {
                foreach (var link in _linkers.Where(link => !LinkHelper.LinkExists(game, link.LinkName)))
                {
                    link.StartBrowserSearch(game);
                    result = true;
                }
            }
            else
            {
                var globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}", true)
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = _linkers.Count(link => !LinkHelper.LinkExists(game, link.LinkName));

                        foreach (var link in _linkers.Where(link => !LinkHelper.LinkExists(game, link.LinkName)))
                        {
                            activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}{Environment.NewLine}{link.LinkName}";

                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            link.StartBrowserSearch(game);
                            result = true;

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            return result;
        }

        private bool SelectLinks(bool add = true)
        {
            try
            {
                var viewModel = new SelectedLinksViewModel(Links, add);
                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesSelectLinksWindowName"));
                var view = new SelectedLinksView(window) { DataContext = viewModel };

                window.Content = view;
                if (window.ShowDialog() != true)
                {
                    return false;
                }

                _linkers = viewModel.Links.Where(x => x.Selected).Select(x => x.Linker).ToList();

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing SelectedLinksView", true);

                return false;
            }
        }
    }
}