using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a linker to all available websites in the Links list, if a definitive linker was found.
    /// </summary>
    internal class AddWebsiteLinks : LinkAction
    {
        private static AddWebsiteLinks _instance = null;
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

        private static bool AddLink(Game game, ILinker linker, ActionModifierTypes actionModifier)
        {
            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    return linker.AddLink(game);
                case ActionModifierTypes.Search:
                    return linker.AddSearchedLink(game);
                default:
                    return false;
            }
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result = false;

            List<ILinker> links = null;

            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    links = Links.Where(x => x.Settings.IsAddable == true).ToList();
                    break;
                case ActionModifierTypes.Search:
                    links = Links.Where(x => x.Settings.IsSearchable == true).ToList();
                    break;
            }

            if (isBulkAction)
            {
                result = links?.Aggregate(false, (current, link) => current | AddLink(game, link, actionModifier)) ?? false;
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
                        if (links == null)
                        {
                            return;
                        }

                        activateGlobalProgress.ProgressMaxValue = links.Count;

                        foreach (ILinker link in links)
                        {
                            activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(ProgressMessage)}{Environment.NewLine}{link.LinkName}";

                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            result |= AddLink(game, link, actionModifier);

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
    }
}