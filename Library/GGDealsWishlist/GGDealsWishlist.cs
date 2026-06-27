using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace GGDealsWishlist
{
    public class GGDealsWishlist : LibraryPlugin
    {
        private readonly GGDealsDataHandler _dataHandler;

        public GGDealsWishlist(IPlayniteAPI api) : base(api)
        {
            Settings = new GGDealsWishlistSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };

            _dataHandler = new GGDealsDataHandler(Settings.Settings);
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
        public static Guid PluginId { get; } = Guid.Parse("ea4636ef-91da-441c-9efb-99dc751c5189");
        public override LibraryClient Client { get; } = new GGDealsWishlistClient();
        public override Guid Id => PluginId;
        public override string LibraryIcon => Icon;

        public override string Name => "GG.deals Wishlist";

        private GGDealsWishlistSettingsViewModel Settings { get; set; }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            if (string.IsNullOrEmpty(Settings.Settings.WishlistUrl))
            {
                var notificationMessage = new NotificationMessage(
                    ResourceProvider.GetString("LOCGGDealsWishlistPluginName"),
                    ResourceProvider.GetString("LOCGGDealsWishlistConfigNotice"),
                    NotificationType.Error,
                    () => OpenSettingsView());

                API.Instance.Notifications.Add(notificationMessage);

                return null;
            }

            Log.Debug(Settings.Settings.DebugMode, "### STARTED RETRIEVING NEW GAMES ########################################");

            _dataHandler.RetrieveGames();

            Log.Debug(Settings.Settings.DebugMode, "### FINISHED RETRIEVING NEW GAMES ########################################");

            return _dataHandler.Games.GetNewGames(Settings.Settings.MaxGamesToImport);
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new GGDealsWishlistInstallController(args.Game);
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new GGDealsWishlistSettingsView();
    }
}

//TODO: Add option to remove games that aren't on the wishlist anymore. Optionally on library update or manually.
