using System.Collections.Generic;
using System.Windows;

namespace GGDealsWishlist.Models
{
    public class DiscountData : ObservableObject
    {
        private bool _available = true;
        private string _discount;
        private string _discountCode;
        private string _discountCodeValue;
        private bool _discounted = false;
        private string _discountedPrice;
        private bool _historicalLow = false;
        private string _regularPrice;
        private string _shopImage;
        private string _shopLink;
        private string _shopName;

        public bool Available
        {
            get => _available;
            set => SetValue(ref _available, value);
        }

        public string Discount
        {
            get => _discount;
            set => SetValue(ref _discount, value);
        }

        public string DiscountCode
        {
            get => _discountCode;
            set => SetValue(ref _discountCode, value);
        }

        public string DiscountCodeValue
        {
            get => _discountCodeValue;
            set => SetValue(ref _discountCodeValue, value);
        }

        public Visibility DiscountCodeVisibility => string.IsNullOrEmpty(DiscountCode) ? Visibility.Collapsed : Visibility.Visible;

        public bool Discounted
        {
            get => _discounted;
            set => SetValue(ref _discounted, value);
        }

        public string DiscountedPrice
        {
            get => _discountedPrice;
            set => SetValue(ref _discountedPrice, value);
        }

        public Visibility DiscountedVisibility => Discounted ? Visibility.Visible : Visibility.Collapsed;

        public bool HistoricalLow
        {
            get => _historicalLow;
            set => SetValue(ref _historicalLow, value);
        }

        public Visibility HistoricalLowVisibility => HistoricalLow ? Visibility.Visible : Visibility.Collapsed;

        public string RegularPrice
        {
            get => _regularPrice;
            set => SetValue(ref _regularPrice, value);
        }

        public string ShopImage
        {
            get => _shopImage;
            set => SetValue(ref _shopImage, value);
        }

        public string ShopLink
        {
            get => _shopLink;
            set => SetValue(ref _shopLink, value);
        }

        public string ShopName
        {
            get => _shopName;
            set => SetValue(ref _shopName, value);
        }
    }
}
