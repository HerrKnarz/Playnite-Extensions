using Playnite.SDK.Models;

namespace GGDealsWishlist.Models
{
    public class GGDealsGame : GameMetadata
    {
        public DiscountData DiscountData { get; set; }
        public string DisplayImage => Game?.CoverImage ?? GGDealsCoverLink;
        public string DisplayName => Game?.Name ?? Name;
        public Game Game { get; set; }
        public string GGDealsCoverLink { get; set; }
    }
}
