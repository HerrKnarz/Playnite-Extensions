using KNARZhelper;
using System.Collections.Generic;
using System.Windows;

namespace GGDealsWishlist.Models
{
    public class DiscountData : ObservableObject
    {
        private bool _available = true;
        private double _discount;
        private string _discountCode;
        private string _discountCodeValue;
        private double _discountedPrice;
        private string _discountedPriceString;
        private string _discountString;
        private bool _historicalLow = false;
        private double _regularPrice;
        private string _regularPriceString;
        private string _shopLink;
        private string _shopName;

        public bool Available
        {
            get => _available;
            set => SetValue(ref _available, value);
        }

        public double Discount
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

        public bool Discounted => Discount < 0;

        public double DiscountedPrice
        {
            get => _discountedPrice;
            set => SetValue(ref _discountedPrice, value);
        }

        public string DiscountedPriceString
        {
            get => _discountedPriceString;
            set
            {
                DiscountedPrice = string.IsNullOrEmpty(value) ? 0 : value.ExtractNumber();

                SetValue(ref _discountedPriceString, value);
            }
        }

        public Visibility DiscountedVisibility => Discounted ? Visibility.Visible : Visibility.Collapsed;

        public string DiscountString
        {
            get => _discountString;
            set
            {
                Discount = string.IsNullOrEmpty(value) ? 0 : value.ExtractNumber();

                SetValue(ref _discountString, value);
            }
        }

        public bool HistoricalLow
        {
            get => _historicalLow;
            set => SetValue(ref _historicalLow, value);
        }

        public Visibility HistoricalLowVisibility => HistoricalLow ? Visibility.Visible : Visibility.Collapsed;

        public double RegularPrice
        {
            get => _regularPrice;
            set => SetValue(ref _regularPrice, value);
        }

        public string RegularPriceString
        {
            get => _regularPriceString;
            set
            {
                RegularPrice = string.IsNullOrEmpty(value) ? 0 : value.ExtractNumber();

                SetValue(ref _regularPriceString, value);
            }
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
