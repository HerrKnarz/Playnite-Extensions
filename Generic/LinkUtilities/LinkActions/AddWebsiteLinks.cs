using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
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

        private async Task<FindLinkResult> FindLinks(Game game)
        {
            List<BaseClasses.Linker> linkers = Links.Where(x => x.Settings.IsAddable == true).ToList();

            List<Link> links = new List<Link>();

            bool success = false;

            List<Task<FindLinkResult>> tasks = linkers.Select(link => link.FindLinks(game)).ToList();

            await Task.WhenAll(tasks);

            foreach (Task<FindLinkResult> task in tasks.Where(task => task.Result.Success))
            {
                success |= links.AddMissing(task.Result.Links);
            }

            return new FindLinkResult() { Success = success, Links = links };
        }

        private bool AddLinks(Game game, bool isBulkAction = true)
        {
            bool Add(Game g)
            {
                FindLinkResult linkResult = FindLinks(g).GetAwaiter().GetResult();

                return linkResult.Success && (linkResult.Links?.Any() ?? false) && LinkHelper.AddLinks(g, linkResult.Links);
            }

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
            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    return AddLinks(game, isBulkAction);
                case ActionModifierTypes.Search:
                case ActionModifierTypes.SearchMissing:
                    return SearchLinks(game, actionModifier, isBulkAction);
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
            }
        }
    }
}