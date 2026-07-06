using KNARZhelper;
using System.Collections.Generic;
using System.Windows;

namespace GGDealsWishlist.Models
{
    /// <summary>
    /// Represents the discount data for a game, including availability, discount percentage,
    /// prices, and shop information.
    /// </summary>
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

        /// <summary>
        /// Indicates whether the game is available for purchase.
        /// </summary>
        public bool Available
        {
            get => _available;
            set => SetValue(ref _available, value);
        }

        /// <summary>
        /// Discount percentage.
        /// </summary>
        public double Discount
        {
            get => _discount;
            set => SetValue(ref _discount, value);
        }

        /// <summary>
        /// Text of the discount code (usually a percentage, not the code itself)..
        /// </summary>
        public string DiscountCode
        {
            get => _discountCode;
            set => SetValue(ref _discountCode, value);
        }

        /// <summary>
        /// The actual value of the discount code.
        /// </summary>
        public string DiscountCodeValue
        {
            get => _discountCodeValue;
            set => SetValue(ref _discountCodeValue, value);
        }

        /// <summary>
        /// Visibility of the discount code. If the discount code is empty, it will be collapsed;
        /// otherwise, it will be visible.
        /// </summary>
        public Visibility DiscountCodeVisibility => string.IsNullOrEmpty(DiscountCode) ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Indicates whether the game is discounted (i.e., the discount is less than 0).
        /// </summary>
        public bool Discounted => Discount < 0;

        /// <summary>
        /// The discounted price of the game. If the game is not discounted, this will be the
        /// regular price.
        /// </summary>
        public double DiscountedPrice
        {
            get => _discountedPrice;
            set => SetValue(ref _discountedPrice, value);
        }

        /// <summary>
        /// The discounted price including currency as a string. When set, it will extract the
        /// numeric value and update the DiscountedPrice property accordingly.
        /// </summary>
        public string DiscountedPriceString
        {
            get => _discountedPriceString;
            set
            {
                DiscountedPrice = string.IsNullOrEmpty(value) ? 0 : value.ExtractNumber();

                SetValue(ref _discountedPriceString, value);
            }
        }

        /// <summary>
        /// Visibility of the discounted price. If the game is discounted, it will be visible;
        /// otherwise, it will be collapsed.
        /// </summary>
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

        /// <summary>
        /// Indicates whether the game is at its historical low price.
        /// </summary>
        public bool HistoricalLow
        {
            get => _historicalLow;
            set => SetValue(ref _historicalLow, value);
        }

        /// <summary>
        /// Visibility of the historical low indicator. If the game is at its historical low price,
        /// it will be visible; otherwise, it will be collapsed.
        /// </summary>
        public Visibility HistoricalLowVisibility => HistoricalLow ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// The regular price of the game. If the game is discounted, this will be the original
        /// price before the discount.
        /// </summary>
        public double RegularPrice
        {
            get => _regularPrice;
            set => SetValue(ref _regularPrice, value);
        }

        /// <summary>
        /// The regular price of the game including currency as a string. When set, it will extract
        /// the numeric value and update the RegularPrice property accordingly.
        /// </summary>
        public string RegularPriceString
        {
            get => _regularPriceString;
            set
            {
                RegularPrice = string.IsNullOrEmpty(value) ? 0 : value.ExtractNumber();

                SetValue(ref _regularPriceString, value);
            }
        }

        /// <summary>
        /// The link to the shop where the game can be purchased.
        /// </summary>
        public string ShopLink
        {
            get => _shopLink;
            set => SetValue(ref _shopLink, value);
        }

        /// <summary>
        /// The name of the shop where the game can be purchased.
        /// </summary>
        public string ShopName
        {
            get => _shopName;
            set => SetValue(ref _shopName, value);
        }
    }
}
