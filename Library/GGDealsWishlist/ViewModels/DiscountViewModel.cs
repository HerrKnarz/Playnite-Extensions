using GGDealsWishlist.Models;
using GGDealsWishlist.Views;
using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
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
        private readonly GGDealsWishlist _plugin;
        private CollectionViewSource _gamesViewSource;
        private bool _groupByShop;
        private string _searchTerm = string.Empty;
        private bool _showOnlyDiscountedGames = true;
        private bool _showOnlyHistoricalLowPrices = false;
        private SortOrder _sortOrder = SortOrder.Discount;

        public DiscountViewModel(GGDealsWishlist plugin)
        {
            _plugin = plugin;

            _groupByShop = _plugin.Settings.Settings.DiscountViewSettings.GroupByShop;
            _showOnlyDiscountedGames = _plugin.Settings.Settings.DiscountViewSettings.ShowOnlyDiscountedGames;
            _showOnlyHistoricalLowPrices = _plugin.Settings.Settings.DiscountViewSettings.ShowOnlyHistoricalLowPrices;
            _sortOrder = _plugin.Settings.Settings.DiscountViewSettings.SortOrder;

            PrepareGamesViewSource();
        }

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
        {
            var settings = _plugin.Settings.Settings.DiscountViewSettings;

            settings.GroupByShop = GroupByShop;
            settings.ShowOnlyDiscountedGames = ShowOnlyDiscountedGames;
            settings.ShowOnlyHistoricalLowPrices = ShowOnlyHistoricalLowPrices;
            settings.SortOrder = SortOrder;
            settings.WindowHeight = Convert.ToInt32(win.Height);
            settings.WindowWidth = Convert.ToInt32(win.Width);
            _plugin.SavePluginSettings(_plugin.Settings.Settings);

            win.DialogResult = true;
            win.Close();
        });

        public GGDealsGames Games { get; set; }

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

                SortGroupFilter();
            }
        }

        public RelayCommand RefreshDiscountsCommand => new RelayCommand(() =>
        {
            RefreshGames();
            SortGroupFilter();
        });

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

        public SortOrder SortOrder
        {
            get => _sortOrder;
            set
            {
                SetValue(ref _sortOrder, value);
                SortGroupFilter();
            }
        }

        public SortOrderWithCaptions SortOrderWithCaptions { get; } = new SortOrderWithCaptions();

        public static void ShowWindow(GGDealsWishlist plugin)
        {
            try
            {
                var viewModel = new DiscountViewModel(plugin);

                var view = new DiscountView();

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCGGDealsWishlistMenuDiscountView"),
                    plugin.Settings.Settings.DiscountViewSettings.WindowWidth, plugin.Settings.Settings.DiscountViewSettings.WindowHeight);

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
            if (_plugin.DataHandler.Games is null || _plugin.DataHandler.Games.Count == 0 || _plugin.DataHandler.Games.LastRefresh < DateTime.Now.AddHours(-1))
            {
                RefreshGames();
            }

            Games = _plugin.DataHandler.Games;

            GamesViewSource = new CollectionViewSource
            {
                Source = Games
            };

            SortGroupFilter();
        }

        private void RefreshGames()
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
                    _plugin.DataHandler.RefreshGames();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);
        }

        private void SortGames()
        {
            using (GamesViewSource.DeferRefresh())
            {
                GamesViewSource.SortDescriptions.Clear();

                if (GroupByShop)
                {
                    GamesViewSource.SortDescriptions.Add(new SortDescription("DiscountData.ShopName", ListSortDirection.Ascending));
                }

                switch (SortOrder)
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

            GamesViewSource.View.Filter = Filter;
        }

        private void SortGroupFilter()
        {
            SortGames();

            if (GroupByShop)
            {
                GamesViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("DiscountData.ShopName"));
            }
            else
            {
                GamesViewSource.View.GroupDescriptions.Clear();
            }

            GamesViewSource.View.Filter = Filter;
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

//TODO: Add list of games to the settings for themes to use them. Have to ask if the sorting is important.
//LATER: Maybe add option to just select a saved Playnite filter to filter the games.
