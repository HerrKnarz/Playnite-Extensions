using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to change steam links between https and app-urls.
    /// </summary>
    internal class ChangeSteamLinks : LinkAction
    {
        private const string _communityPattern = "steamcommunity.com";
        private const string _steamAppPrefix = "steam://openurl/";
        private const string _storePattern = "steampowered.com";
        private static ChangeSteamLinks _instance;
        private ChangeSteamLinks() { }

        public bool ChangeSteamLinksAfterChange { get; set; } = false;

        public override string ProgressMessage => "LOCLinkUtilitiesProgressChangeSteamLinks";

        public override string ResultMessage => "LOCLinkUtilitiesDialogSteamChangedMessage";

        public static ChangeSteamLinks Instance() => _instance ?? (_instance = new ChangeSteamLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               Change(game, actionModifier);

        private static bool Change(Game game, ActionModifierTypes actionModifier, bool updateDb = true)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var mustUpdate = false;

            foreach (var link in game.Links)
            {
                if (!link.Url.Contains(_communityPattern) && !link.Url.Contains(_storePattern))
                {
                    continue;
                }

                var url = link.Url;

                switch (actionModifier)
                {
                    case ActionModifierTypes.AppLink when link.Url.StartsWith("http"):
                        url = _steamAppPrefix + url;
                        break;
                    case ActionModifierTypes.WebLink when link.Url.StartsWith(_steamAppPrefix):
                        url = url.Replace(_steamAppPrefix, string.Empty);
                        break;
                    case ActionModifierTypes.Add:
                    case ActionModifierTypes.AddSelected:
                    case ActionModifierTypes.DontRename:
                    case ActionModifierTypes.Name:
                    case ActionModifierTypes.None:
                    case ActionModifierTypes.Search:
                    case ActionModifierTypes.SearchInBrowser:
                    case ActionModifierTypes.SearchMissing:
                    case ActionModifierTypes.SearchSelected:
                    case ActionModifierTypes.SortOrder:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(actionModifier), actionModifier, null);
                }

                if (url == link.Url)
                {
                    continue;
                }

                if (GlobalSettings.Instance().OnlyATest)
                {
                    link.Url = url;
                }
                else
                {
                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        link.Url = url;
                    });
                }

                mustUpdate = true;
            }

            if (!mustUpdate)
            {
                return false;
            }

            if (updateDb && !GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }
    }
}