using GGDealsWishlist.Models;
using GGDealsWishlist.Views;
using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace GGDealsWishlist.ViewModels
{
    public enum SortOrder
    {
        Name = 1,
        Discount = 2,
        Price = 3,
    }

    public class DiscountViewModel : ObservableObject
    {
        private SortOrder _currentSortOrder = SortOrder.Discount;
        private CollectionViewSource _gamesViewSource;
        private bool _groupByShop;
        private string _searchTerm = string.Empty;
        private bool _showOnlyDiscountedGames = true;
        private bool _showOnlyHistoricalLowPrices = false;

        public DiscountViewModel()
        {
        }

        public SortOrder CurrentSortOrder
        {
            get => _currentSortOrder;
            set
            {
                SetValue(ref _currentSortOrder, value);
                SortGames();
            }
        }

        public GGDealsGames Games { get; set; }

        //TODO: Add option to display either the cover, gg.deals image or icon in the discount view.
        public CollectionViewSource GamesViewSource
        {
            get => _gamesViewSource;
            set => SetValue(ref _gamesViewSource, value);
        }

        public bool GroupByShop
        {
            get => _groupByShop;
            set
            {
                SetValue(ref _groupByShop, value);

                SortGames(_groupByShop);

                if (_groupByShop)
                {
                    GamesViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("DiscountData.ShopName"));
                }
                else
                {
                    GamesViewSource.View.GroupDescriptions.Clear();
                }

                ((IEditableCollectionView)GamesViewSource.View).CommitEdit();
                GamesViewSource.View.Filter = Filter;
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                GamesViewSource.View.Filter = Filter;
            }
        }

        public GGDealsGame SelectedGame { get; set; }

        public bool ShowOnlyDiscountedGames
        {
            get => _showOnlyDiscountedGames;
            set
            {
                SetValue(ref _showOnlyDiscountedGames, value);
                GamesViewSource.View.Filter = Filter;
            }
        }

        public bool ShowOnlyHistoricalLowPrices
        {
            get => _showOnlyHistoricalLowPrices;
            set
            {
                SetValue(ref _showOnlyHistoricalLowPrices, value);
                GamesViewSource.View.Filter = Filter;
            }
        }

        public SortOrderWithCaptions SortOrderWithCaptions { get; } = new SortOrderWithCaptions();

        public static void ShowWindow(GGDealsWishlist plugin)
        {
            try
            {
                if (plugin.DataHandler.Games is null || plugin.DataHandler.Games.Count == 0)
                {
                    var globalProgressOptions = new GlobalProgressOptions(
                        ResourceProvider.GetString("LOCGGDealsWishlistProgessLoadingDiscountData"),
                        false)
                    {
                        IsIndeterminate = true
                    };

                    API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                    {
                        try
                        {
                            plugin.DataHandler.RefreshGames();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }, globalProgressOptions);
                }

                var viewModel = new DiscountViewModel
                {
                    Games = plugin.DataHandler.Games
                };

                viewModel.PrepareGamesViewSource();

                var view = new DiscountView();

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCGGDealsWishlistMenuDiscountView"),
                    1200,
                    800);

                window.Content = view;
                window.DataContext = viewModel;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing discount viewer", true);
            }
        }

        private bool Filter(object item)
        {
            return item is GGDealsGame game &&
            game.DisplayName.RegExIsMatch(SearchTerm) &&
            (!_showOnlyDiscountedGames || game.DiscountData.Discounted) &&
            (!_showOnlyHistoricalLowPrices || game.DiscountData.HistoricalLow);
        }

        private void PrepareGamesViewSource()
        {
            GamesViewSource = new CollectionViewSource
            {
                Source = Games
            };

            GamesViewSource.View.Filter = Filter;
            SortGames();
        }

        private void SortGames(bool sortByShop = false)
        {
            using (GamesViewSource.DeferRefresh())
            {
                GamesViewSource.SortDescriptions.Clear();

                if (sortByShop)
                {
                    GamesViewSource.SortDescriptions.Add(new SortDescription("DiscountData.ShopName", ListSortDirection.Ascending));
                }

                switch (CurrentSortOrder)
                {
                    case SortOrder.Name:
                        GamesViewSource.SortDescriptions.Add(new SortDescription("SortingName", ListSortDirection.Ascending));
                        break;

                    case SortOrder.Discount:
                        GamesViewSource.SortDescriptions.Add(new SortDescription("DiscountData.Discount", ListSortDirection.Ascending));
                        GamesViewSource.SortDescriptions.Add(new SortDescription("SortingName", ListSortDirection.Ascending));
                        break;

                    case SortOrder.Price:
                        GamesViewSource.SortDescriptions.Add(new SortDescription("DiscountData.DiscountedPrice", ListSortDirection.Ascending));
                        GamesViewSource.SortDescriptions.Add(new SortDescription("SortingName", ListSortDirection.Ascending));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GamesViewSource.IsLiveSortingRequested = true;
            }
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
