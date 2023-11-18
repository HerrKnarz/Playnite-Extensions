using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to add a link to all available websites in the Links list, if a definitive link was found.
    /// </summary>
    internal class AddWebsiteLinks : LinkAction
    {
        private static AddWebsiteLinks _instance;
        private static readonly object _mutex = new object();

        private List<BaseClasses.Linker> _linkers;

        private AddWebsiteLinks() => Links = new Links();

        public Links Links { get; }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressWebsiteLink";
        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public static AddWebsiteLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AddWebsiteLinks();
                }
            }

            return _instance;
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

            ConcurrentQueue<Link> linksQueue = new ConcurrentQueue<Link>();

            Parallel.ForEach(_linkers, linker =>
            {
                linker.FindLinks(game, out List<Link> innerLinks);

                foreach (Link innerLink in innerLinks)
                {
                    linksQueue.Enqueue(innerLink);
                }
            });

            return links.AddMissing(linksQueue.Distinct());
        }

        /// <summary>
        ///     Adds links to all configured websites
        /// </summary>
        /// <param name="game">game the links will be added to.</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if new links were added.</returns>
        private bool AddLinks(Game game, bool isBulkAction = true)
        {
            bool Add(Game g) => FindLinks(game, out List<Link> links) && (links?.Any() ?? false) && LinkHelper.AddLinks(g, links);

            if (isBulkAction)
            {
                return Add(game);
            }

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}",
                true
            )
            {
                IsIndeterminate = true
            };

            bool result = false;

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    result = Add(game);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            return result;
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
            bool result = false;

            if (isBulkAction)
            {
                foreach (BaseClasses.Linker link in _linkers)
                {
                    result |= link.AddSearchedLink(game, actionModifier == ActionModifierTypes.SearchMissing, false);
                }
            }
            else
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}", true)
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
        /// <param name="actionModifier">Kind of search (e.g. Search or SearchMissing)</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if pages were opened.</returns>
        private bool SearchLinksInBrowser(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result = false;

            if (isBulkAction)
            {
                foreach (BaseClasses.Linker link in _linkers.Where(link => !LinkHelper.LinkExists(game, link.LinkName)))
                {
                    link.StartBrowserSearch(game);
                    result = true;
                }
            }
            else
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}", true)
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = _linkers.Count(link => !LinkHelper.LinkExists(game, link.LinkName));

                        foreach (BaseClasses.Linker link in _linkers.Where(link => !LinkHelper.LinkExists(game, link.LinkName)))
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
                SelectedLinksViewModel viewModel = new SelectedLinksViewModel(Links, add);
                Window window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesSelectLinksWindowName"));
                SelectedLinksView view = new SelectedLinksView(window) { DataContext = viewModel };

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
                Log.Error(exception, "Error during initializing ReviewDuplicatesView", true);

                return false;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }
        }

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
                    return SearchLinksInBrowser(game, actionModifier, isBulkAction);
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }
        }
    }
}