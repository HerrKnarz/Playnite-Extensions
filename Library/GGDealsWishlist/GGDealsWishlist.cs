using GGDealsWishlist.ViewModels;
using GGDealsWishlist.Views;
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
        public GGDealsWishlist(IPlayniteAPI api) : base(api)
        {
            Settings = new SettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "ggdDiscountIcon", "\xefdd" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }

            DataHandler = new GGDealsDataHandler(Settings.Settings);
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
        public static Guid PluginId { get; } = Guid.Parse("ea4636ef-91da-441c-9efb-99dc751c5189");
        public override LibraryClient Client { get; } = new GGDealsWishlistClient();
        public GGDealsDataHandler DataHandler { get; }
        public override Guid Id => PluginId;
        public override string LibraryIcon => Icon;

        public override string Name => "GG.deals Wishlist";

        private SettingsViewModel Settings { get; set; }

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

            DataHandler.RefreshGames();

            Log.Debug(Settings.Settings.DebugMode, "### FINISHED RETRIEVING NEW GAMES ########################################");

            return DataHandler.Games.GetNewGames(Settings.Settings.MaxGamesToImport);
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new GGDealsWishlistInstallController(args.Game);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCGGDealsWishlistPluginName");

            var menuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCGGDealsWishlistMenuDiscountView"),
                    MenuSection = $"@{menuSection}",
                    Icon = "ggdDiscountIcon",
                    Action = a => DiscountViewModel.ShowWindow(this)
                }
            };

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
    }
}

//TODO: Add option to remove games that aren't on the wishlist anymore. Optionally on library update or manually.
