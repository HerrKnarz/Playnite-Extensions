using System.Collections.Generic;

namespace GGDealsWishlist.Models
{
    public class Settings : ObservableObject
    {
        private bool _debugMode = false;
        private string _defaultCategory = string.Empty;
        private ImageOption _discountViewImage = ImageOption.GGDealsBanner;
        private bool _importGamesAsInstalled = false;
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

        public bool ImportGamesAsInstalled
        {
            get => _importGamesAsInstalled;
            set => SetValue(ref _importGamesAsInstalled, value);
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

        public string WishlistUrl
        {
            get => _wishlistUrl;
            set => SetValue(ref _wishlistUrl, value);
        }
    }
}
