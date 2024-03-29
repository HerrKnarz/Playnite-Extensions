﻿using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LinkUtilities
{
    internal class CheckLinks : ViewModelBase
    {
        private readonly List<Game> _games;
        private ObservableCollectionFast<CheckLink> _filteredLinks = new ObservableCollectionFast<CheckLink>();
        private ObservableCollectionFast<CheckLink> _links = new ObservableCollectionFast<CheckLink>();
        private string _searchString = string.Empty;

        public CheckLinks(List<Game> games, bool hideOkOnLinkCheck)
        {
            _games = games;
            Check(hideOkOnLinkCheck);
        }

        public ObservableCollectionFast<CheckLink> Links
        {
            get => _links;
            set
            {
                _links = value;
                OnPropertyChanged("Links");
            }
        }

        public ObservableCollectionFast<CheckLink> FilteredLinks
        {
            get => _filteredLinks;
            set
            {
                _filteredLinks = value;
                OnPropertyChanged("FilteredLinks");
            }
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                FilterLinks();
                OnPropertyChanged("SearchString");
            }
        }

        public void FilterLinks()
        {
            FilteredLinks.Clear();

            FilteredLinks.AddRange(SearchString.Any() ? Links.Where(x => x.Link.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) : Links);
        }

        public void Check(bool hideOkOnLinkCheck)
        {
            Links.Clear();

            using (API.Instance.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
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

                        foreach (Game game in _games)
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

        private void Check(Game game, bool hideOkOnLinkCheck)
        {
            if (!game.Links?.Any() ?? true)
            {
                return;
            }

            ConcurrentQueue<CheckLink> linksQueue = new ConcurrentQueue<CheckLink>();

            Parallel.ForEach(game.Links.Where(x => !x.Url.StartsWith("steam")), link =>
            {
                LinkCheckResult linkCheckResult = LinkHelper.CheckUrl(link.Url);

                if (!hideOkOnLinkCheck || linkCheckResult.StatusCode != HttpStatusCode.OK)
                {
                    linksQueue.Enqueue(new CheckLink
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

        public void Remove(CheckLink checkLink)
        {
            checkLink.Remove();

            Links.Remove(checkLink);
            FilteredLinks.Remove(checkLink);
        }

        public void Replace(CheckLink checkLink) => checkLink.Replace();
    }
}