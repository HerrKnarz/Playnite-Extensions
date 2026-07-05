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

        public RelayCommand CopyCodeCommand => new RelayCommand(() =>
        {
            if (!string.IsNullOrEmpty(DiscountData.DiscountCodeValue))
            {
                Clipboard.SetText(DiscountData.DiscountCodeValue);
                DisplayCopySuccessNoticeAsync();
            }
        });

        public DiscountData DiscountData
        {
            get => _discountData;
            set => SetValue(ref _discountData, value);
        }

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

        public string DisplayName => Game?.Name ?? ImportedMetadata.Name;

        public Game Game
        {
            get => _game;
            set => SetValue(ref _game, value);
        }

        public string GGDealsCoverLink
        {
            get => _gGDealsCoverLink;
            set => SetValue(ref _gGDealsCoverLink, value);
        }

        public string GGDealsLink => ImportedMetadata.Links.FirstOrDefault()?.Url;

        public GameMetadata ImportedMetadata
        {
            get => _importedMetadata;
            set => SetValue(ref _importedMetadata, value);
        }

        public Settings Settings
        {
            get => _settings;
            set => SetValue(ref _settings, value);
        }

        public string SortingName => Game?.SortingName ?? Game?.Name ?? new SortableNameConverter().Convert(ImportedMetadata.Name);

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
