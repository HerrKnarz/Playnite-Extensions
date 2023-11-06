using LinkUtilities.BaseClasses;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to rename links based on patterns.
    /// </summary>
    internal class ChangeSteamLinks : LinkAction
    {
        private const string _communityPattern = "steamcommunity.com";
        private const string _steamAppPrefix = "steam://openurl/";
        private const string _storePattern = "steampowered.com";
        private static ChangeSteamLinks _instance;
        private static readonly object _mutex = new object();
        private ChangeSteamLinks() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressChangeSteamLinks";

        public override string ResultMessage => "LOCLinkUtilitiesDialogSteamChangedMessage";

        public bool ChangeSteamLinksAfterChange { get; set; } = false;

        public static ChangeSteamLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new ChangeSteamLinks();
                }
            }

            return _instance;
        }

        private static bool Change(Game game, ActionModifierTypes actionModifier, bool updateDb = true)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            bool mustUpdate = false;

            foreach (Link link in game.Links)
            {
                if (!link.Url.Contains(_communityPattern) && !link.Url.Contains(_storePattern))
                {
                    continue;
                }

                string url = link.Url;

                if (actionModifier == ActionModifierTypes.AppLink && link.Url.StartsWith("http"))
                {
                    url = _steamAppPrefix + url;
                }

                if (actionModifier == ActionModifierTypes.WebLink && link.Url.StartsWith(_steamAppPrefix))
                {
                    url = url.Replace(_steamAppPrefix, string.Empty);
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

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               Change(game, actionModifier);
    }
}