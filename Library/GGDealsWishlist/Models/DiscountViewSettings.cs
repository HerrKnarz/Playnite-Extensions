using GGDealsWishlist.ViewModels;

namespace GGDealsWishlist.Models
{
    public class DiscountViewSettings
    {
        public bool GroupByShop { get; set; }
        public bool ShowOnlyDiscountedGames { get; set; } = true;
        public bool ShowOnlyHistoricalLowPrices { get; set; } = false;
        public SortOrder SortOrder { get; set; } = SortOrder.Discount;
        public int WindowHeight { get; set; } = 800;
        public int WindowWidth { get; set; } = 1000;
    }
}
