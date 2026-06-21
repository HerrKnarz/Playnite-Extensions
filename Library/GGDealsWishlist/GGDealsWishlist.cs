using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
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

        public static Guid PluginId { get; } = Guid.Parse("ea4636ef-91da-441c-9efb-99dc751c5189");
        public override LibraryClient Client { get; } = new GGDealsWishlistClient();
        public override Guid Id => PluginId;

        public override string Name => "GG.deals Wishlist";

        private GGDealsWishlistSettingsViewModel Settings { get; set; }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            _dataHandler.RetrieveGames();

            return _dataHandler.Games.GetNewGames();
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new GGDealsWishlistSettingsView();
    }
}
