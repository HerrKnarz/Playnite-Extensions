using KNARZhelper;
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
        private ObservableCollectionFast<CheckLink> _links = new ObservableCollectionFast<CheckLink>();

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
        }

        private void Check(Game game, bool hideOkOnLinkCheck)
        {
            if (!game.Links?.Any() ?? true)
            {
                return;
            }

            ConcurrentQueue<CheckLink> linksQueue = new ConcurrentQueue<CheckLink>();

            Parallel.ForEach(game.Links, link =>
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

            Links.AddRange(linksQueue);
        }

        public void Remove(CheckLink checkLink)
        {
            checkLink.Remove();

            Links.Remove(checkLink);
        }

        public void Replace(CheckLink checkLink) => checkLink.Replace();
    }
}