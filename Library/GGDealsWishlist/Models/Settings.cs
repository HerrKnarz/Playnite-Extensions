using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Windows.Media;

namespace GGDealsWishlist.Models
{
    public class Settings : ObservableObject
    {
        private bool _debugMode = false;
        private string _defaultCategory = string.Empty;
        private ImageOption _discountViewImage = ImageOption.GGDealsBanner;
        private bool _displaySidebarButton = false;
        private bool _displayTopPanelButton = true;
        private Brush _historicalLowBrush = new SolidColorBrush(Colors.DarkGreen);
        private Color _historicalLowColor = Colors.DarkGreen;
        private string _historicalLowColorHex = "#FF006400";
        private bool _importGamesAsInstalled = false;
        private bool _importGamesToLibrary = true;
        private int _maxGamesToImport = 100;
        private bool _onlyImportGames = true;
        private string _wishlistUrl = string.Empty;

        public bool DebugMode
        {
            get => _debugMode;
            set => SetValue(ref _debugMode, value);
        }

        public string DefaultCategory
        {
            get => _defaultCategory;
            set => SetValue(ref _defaultCategory, value);
        }

        public ImageOption DiscountViewImage
        {
            get => _discountViewImage;
            set => SetValue(ref _discountViewImage, value);
        }

        public DiscountViewSettings DiscountViewSettings { get; set; } = new DiscountViewSettings();

        public bool DisplaySidebarButton
        {
            get => _displaySidebarButton;
            set => SetValue(ref _displaySidebarButton, value);
        }

        public bool DisplayTopPanelButton
        {
            get => _displayTopPanelButton;
            set => SetValue(ref _displayTopPanelButton, value);
        }

        [DontSerialize]
        public Brush HistoricalLowBrush
        {
            get => _historicalLowBrush;
            set => SetValue(ref _historicalLowBrush, value);
        }

        [DontSerialize]
        public Color HistoricalLowColor
        {
            get => _historicalLowColor;
            set
            {
                SetValue(ref _historicalLowColor, value);

                HistoricalLowBrush = new SolidColorBrush(HistoricalLowColor);
            }
        }

        public string HistoricalLowColorHex
        {
            get => _historicalLowColorHex;
            set
            {
                SetValue(ref _historicalLowColorHex, value);

                HistoricalLowColor = string.IsNullOrEmpty(HistoricalLowColorHex)
                    ? Colors.DarkGreen
                    : (Color)ColorConverter.ConvertFromString(HistoricalLowColorHex);
            }
        }

        public bool ImportGamesAsInstalled
        {
            get => _importGamesAsInstalled;
            set => SetValue(ref _importGamesAsInstalled, value);
        }

        public bool ImportGamesToLibrary
        {
            get => _importGamesToLibrary;
            set => SetValue(ref _importGamesToLibrary, value);
        }

        public int MaxGamesToImport
        {
            get => _maxGamesToImport;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                SetValue(ref _maxGamesToImport, value);
            }
        }

        public bool OnlyImportGames
        {
            get => _onlyImportGames;
            set => SetValue(ref _onlyImportGames, value);
        }

        [DontSerialize]
        public GGDealsGames WishListedGames
        {
            get
            {
                if (GGDealsWishlist.Games is null || GGDealsWishlist.Games.Count == 0)
                {
                    GGDealsWishlist.DataHandler.RefreshGames();
                }

                return GGDealsWishlist.Games;
            }
        }

        public string WishlistUrl
        {
            get => _wishlistUrl;
            set => SetValue(ref _wishlistUrl, value);
        }
    }
}
