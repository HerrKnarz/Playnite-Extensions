using Playnite.SDK;
using System.Collections.Generic;

namespace GGDealsWishlist.Models
{
    public enum GroupBy
    {
        None = 1,
        Shop = 2,
        CompletionStatus = 3,
    }

    public enum SortOrder
    {
        Name = 1,
        Discount = 2,
        Price = 3,
    }

    public class DiscountViewSettings
    {
        public GroupBy GroupBy { get; set; } = GroupBy.None;
        public bool ShowOnlyDiscountedGames { get; set; } = true;
        public bool ShowOnlyHistoricalLowPrices { get; set; } = false;
        public SortOrder SortOrder { get; set; } = SortOrder.Discount;
        public int WindowHeight { get; set; } = 800;
        public int WindowWidth { get; set; } = 1000;
    }

    public class GroupByWithCaptions : Dictionary<GroupBy, string>
    {
        public GroupByWithCaptions()
        {
            Add(GroupBy.None, ResourceProvider.GetString("LOCGGDealsWishlistGroupByNone"));
            Add(GroupBy.Shop, ResourceProvider.GetString("LOCGGDealsWishlistGroupByShop"));
            Add(GroupBy.CompletionStatus, ResourceProvider.GetString("LOCGGDealsWishlistGroupByCompletionStatus"));
        }
    }

    /// <summary>
    /// Dictionary of types with captions to show in a combo box.
    /// </summary>
    public class SortOrderWithCaptions : Dictionary<SortOrder, string>
    {
        public SortOrderWithCaptions()
        {
            Add(SortOrder.Name, ResourceProvider.GetString("LOCGGDealsWishlistSortOrderName"));
            Add(SortOrder.Discount, ResourceProvider.GetString("LOCGGDealsWishlistSortOrderDiscount"));
            Add(SortOrder.Price, ResourceProvider.GetString("LOCGGDealsWishlistSortOrderPrice"));
        }
    }
}
