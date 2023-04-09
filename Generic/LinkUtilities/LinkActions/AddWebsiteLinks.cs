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

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a linker to all available websites in the Links list, if a definitive linker was found.
    /// </summary>
    internal class AddWebsiteLinks : LinkAction
    {
        private static AddWebsiteLinks _instance;
        private static readonly object _mutex = new object();

        private AddWebsiteLinks() => Links = new Links();

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

        public Links Links { get; }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressWebsiteLink";
        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        /// <summary>
        /// Finds links to all configured websites. The links are added asynchronously to a ConcurrentBag and then returned as a distinct list.
        /// </summary>
        /// <param name="game">game the links will be found for.</param>
        /// <param name="links">Returns a list of found links</param>
        /// <returns>True, if links were found.</returns>
        private bool FindLinks(Game game, out List<Link> links)
        {
            List<BaseClasses.Linker> linkers = Links.Where(x => x.Settings.IsAddable == true).ToList();

            links = new List<Link>();

            ConcurrentQueue<Link> linksQueue = new ConcurrentQueue<Link>();

            Parallel.ForEach(linkers, linker =>
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
        /// Adds links to all configured websites
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

            API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
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
        /// Searches links for all configured websites
        /// </summary>
        /// <param name="game">game the links will be searched for.</param>
        /// <param name="actionModifier">Kind of search (e.g. Search or SearchMissing)</param>
        /// <param name="isBulkAction">If true, the method already is used in a progress bar and no new one has to be started.</param>
        /// <returns>True, if new links were added.</returns>
        private bool SearchLinks(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result = false;

            List<BaseClasses.Linker> links = Links.Where(x => x.Settings.IsSearchable == true).ToList();

            if (isBulkAction)
            {
                foreach (BaseClasses.Linker link in links)
                {
                    result |= link.AddSearchedLink(game, actionModifier == ActionModifierTypes.SearchMissing);
                }
            }
            else
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}", true)
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = links.Count;

                        foreach (ILinker link in links)
                        {
                            activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}{Environment.NewLine}{link.LinkName}";

                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            result |= link.AddSearchedLink(game, actionModifier == ActionModifierTypes.SearchMissing);

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

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result;

            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    result = AddLinks(game, isBulkAction);
                    break;
                case ActionModifierTypes.Search:
                case ActionModifierTypes.SearchMissing:
                    result = SearchLinks(game, actionModifier, isBulkAction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }

            return result && DoAfterChange.Instance().Execute(game, actionModifier, isBulkAction);
        }
    }
}