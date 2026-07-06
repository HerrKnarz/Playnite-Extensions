using KNARZhelper.GamesCommon;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GGDealsWishlist.Models
{
    public enum ImageOption
    {
        GGDealsBanner = 1,
        Cover = 2,
        Icon = 3,
    }

    /// <summary>
    /// Represents a game with associated discount data, imported metadata, and settings for display
    /// in the GGDeals Wishlist plugin.
    /// </summary>
    public class GGDealsGame : ObservableObject
    {
        private DiscountData _discountData;
        private Game _game;
        private string _gGDealsCoverLink;
        private GameMetadata _importedMetadata;
        private Settings _settings;

        public GGDealsGame(Game game, GameMetadata importedMetadata, DiscountData discountData, Settings settings)
        {
            _game = game;
            _importedMetadata = importedMetadata;
            _discountData = discountData;
            _settings = settings;
        }

        /// <summary>
        /// Command to copy the discount code to the clipboard. If the discount code is not empty,
        /// it copies the code and displays a success notice.
        /// </summary>
        public RelayCommand CopyCodeCommand => new RelayCommand(() =>
        {
            if (!string.IsNullOrEmpty(DiscountData.DiscountCodeValue))
            {
                Clipboard.SetText(DiscountData.DiscountCodeValue);
                DisplayCopySuccessNoticeAsync();
            }
        });

        /// <summary>
        /// The discount data associated with the game, including availability, discount percentage,
        /// prices, and shop information.
        /// </summary>
        public DiscountData DiscountData
        {
            get => _discountData;
            set => SetValue(ref _discountData, value);
        }

        /// <summary>
        /// Gets the image to display based on the selected image option.
        /// </summary>
        public string DisplayImage
        {
            get
            {
                switch (Settings.DiscountViewImage)
                {
                    case ImageOption.GGDealsBanner:
                        return GGDealsCoverLink;

                    case ImageOption.Cover:
                        return GameEx.GetGameCoverPath(Game);

                    case ImageOption.Icon:
                        return GameEx.GetGameIconPath(Game);

                    default:
                        return GGDealsCoverLink;
                }
            }
        }

        /// <summary>
        /// Gets the display name for the game.
        /// </summary>
        public string DisplayName => Game?.Name ?? ImportedMetadata.Name;

        /// <summary>
        /// Gets or sets the game from the Playnite library associated with this instance.
        /// </summary>
        public Game Game
        {
            get => _game;
            set => SetValue(ref _game, value);
        }

        /// <summary>
        /// Link to the GGDeals cover image for the game. This is used when displaying the GGDeals
        /// banner image option.
        /// </summary>
        public string GGDealsCoverLink
        {
            get => _gGDealsCoverLink;
            set => SetValue(ref _gGDealsCoverLink, value);
        }

        /// <summary>
        /// Link to the GGDeals page for the game.
        /// </summary>
        public string GGDealsLink => ImportedMetadata.Links.FirstOrDefault()?.Url;

        /// <summary>
        /// Gets or sets the imported metadata for the game, which includes information such as
        /// name, description, and links. Mainly useful if the game is not in the Playnite library.
        /// </summary>
        public GameMetadata ImportedMetadata
        {
            get => _importedMetadata;
            set => SetValue(ref _importedMetadata, value);
        }

        /// <summary>
        /// Settings for the GGDeals Wishlist plugin, which includes user preferences for display
        /// and behavior.
        /// </summary>
        public Settings Settings
        {
            get => _settings;
            set => SetValue(ref _settings, value);
        }

        /// <summary>
        /// Gets the sorting name for the game, which is used for sorting in the UI. It prioritizes
        /// the game's sorting name, then the game's name, and finally falls back to a converted
        /// version of the imported metadata name.
        /// </summary>
        public string SortingName => Game?.SortingName ?? Game?.Name ?? new SortableNameConverter().Convert(ImportedMetadata.Name);

        /// <summary>
        /// Displays a temporary success notice when the discount code is copied to the clipboard.
        /// It replaces the discount code text with a success message for a brief moment before
        /// restoring the original discount code.
        /// </summary>
        /// <returns></returns>
        private async Task DisplayCopySuccessNoticeAsync()
        {
            var notificationMessage = ResourceProvider.GetString("LOCGGDealsWishlistDiscountCodeCopied");
            var tempCode = DiscountData.DiscountCode;
            DiscountData.DiscountCode = notificationMessage;
            await Task.Delay(1000);
            DiscountData.DiscountCode = tempCode;
        }
    }

    /// <summary>
    /// Dictionary of types with captions to show in a combo box.
    /// </summary>
    public class ImageOptionWithCaptions : Dictionary<ImageOption, string>
    {
        public ImageOptionWithCaptions()
        {
            Add(ImageOption.GGDealsBanner, ResourceProvider.GetString("LOCGGDealsWishlistImageOptionGGDealsBanner"));
            Add(ImageOption.Cover, ResourceProvider.GetString("LOCGGDealsWishlistImageOptionCover"));
            Add(ImageOption.Icon, ResourceProvider.GetString("LOCGGDealsWishlistImageOptionIcon"));
        }
    }
}
