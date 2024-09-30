using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LinkUtilities.ViewModels
{
    internal class CheckLinks : ObservableObject
    {
        private readonly List<Game> _games;
        private ObservableCollectionFast<CheckGameLink> _filteredLinks = new ObservableCollectionFast<CheckGameLink>();
        private ObservableCollectionFast<CheckGameLink> _links = new ObservableCollectionFast<CheckGameLink>();
        private string _searchString = string.Empty;

        public CheckLinks(List<Game> games, bool hideOkOnLinkCheck)
        {
            _games = games;
            Check(hideOkOnLinkCheck);
        }

        public ObservableCollectionFast<CheckGameLink> FilteredLinks
        {
            get => _filteredLinks;
            set => SetValue(ref _filteredLinks, value);
        }

        public ObservableCollectionFast<CheckGameLink> Links
        {
            get => _links;
            set => SetValue(ref _links, value);
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                SetValue(ref _searchString, value);
                FilterLinks();
            }
        }

        public void Check(bool hideOkOnLinkCheck)
        {
            Links.Clear();

            using (API.Instance.Database.BufferedUpdate())
            {
                var globalProgressOptions = new GlobalProgressOptions(
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesName")} - {ResourceProvider.GetString("LOCLinkUtilitiesProgressCheckingLinks")}",
                    true
                )
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = _games.Count;

                        foreach (var game in _games)
                        {
                            activateGlobalProgress.Text =
                                $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString("LOCLinkUtilitiesProgressCheckingLinks")}{Environment.NewLine}{game.Name}";

                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            Check(game, hideOkOnLinkCheck);

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            FilterLinks();
        }

        public void FilterLinks()
        {
            FilteredLinks.Clear();

            FilteredLinks.AddRange(SearchString.Any() ? Links.Where(x => x.Link.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) : Links);
        }

        public void Remove(CheckGameLink checkGameLink)
        {
            checkGameLink.Remove();

            Links.Remove(checkGameLink);
            FilteredLinks.Remove(checkGameLink);
        }

        public void Replace(CheckGameLink checkGameLink) => checkGameLink.Replace();

        private void Check(Game game, bool hideOkOnLinkCheck)
        {
            if (!game.Links?.Any() ?? true)
            {
                return;
            }

            var linksQueue = new ConcurrentQueue<CheckGameLink>();

            Parallel.ForEach(game.Links.Where(x => !x.Url.StartsWith("steam")), link =>
            {
                var linkCheckResult = LinkHelper.CheckUrl(link.Url);

                if (!hideOkOnLinkCheck || linkCheckResult.StatusCode != HttpStatusCode.OK)
                {
                    linksQueue.Enqueue(new CheckGameLink
                    {
                        Game = game,
                        Link = link,
                        LinkCheckResult = linkCheckResult,
                        UrlIsEqual = LinkHelper.CleanUpUrl(linkCheckResult.ResponseUrl) == LinkHelper.CleanUpUrl(link.Url)
                    });
                }
            });

            Links.AddRange(linksQueue.OrderBy(x => x.Link.Name).ToList());
        }
    }
}