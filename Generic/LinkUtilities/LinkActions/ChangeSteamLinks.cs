using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to change steam links between https and app-urls.
    /// </summary>
    internal class ChangeSteamLinks : LinkAction
    {
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
            => SteamHelper.ChangeSteamLinks(game, actionModifier == ActionModifierTypes.AppLink, updateDb, GlobalSettings.Instance().OnlyATest);
    }
}