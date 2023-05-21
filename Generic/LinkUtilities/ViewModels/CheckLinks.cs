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
        private ObservableCollectionFast<CheckedLink> _links = new ObservableCollectionFast<CheckedLink>();

        public CheckLinks(List<Game> games, bool hideOkOnLinkCheck)
        {
            _games = games;
            Check(hideOkOnLinkCheck);
        }

        public ObservableCollectionFast<CheckedLink> Links
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

            ConcurrentQueue<CheckedLink> linksQueue = new ConcurrentQueue<CheckedLink>();

            Parallel.ForEach(game.Links, link =>
            {
                LinkCheckResult linkCheckResult = LinkHelper.CheckUrl(link.Url);

                if (!hideOkOnLinkCheck || linkCheckResult.StatusCode != HttpStatusCode.OK)
                {
                    linksQueue.Enqueue(new CheckedLink
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

        public void Remove(CheckedLink checkedLink)
        {
            checkedLink.Remove();

            Links.Remove(checkedLink);
        }

        public void Replace(CheckedLink checkedLink)
        {
            checkedLink.Replace();
        }
    }
}