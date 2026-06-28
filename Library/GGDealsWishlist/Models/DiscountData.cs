namespace GGDealsWishlist.Models
{
    public class DiscountData
    {
        public bool Available { get; set; } = true;
        public string Discount { get; set; }
        public string DiscountCode { get; set; }
        public string DiscountCodeValue { get; set; }
        public bool Discounted { get; set; } = false;
        public string DiscountedPrice { get; set; }
        public bool HistoricalLow { get; set; } = false;
        public string RegularPrice { get; set; }
        public string ShopImage { get; set; }
        public string ShopLink { get; set; }
        public string ShopName { get; set; }
    }
}
