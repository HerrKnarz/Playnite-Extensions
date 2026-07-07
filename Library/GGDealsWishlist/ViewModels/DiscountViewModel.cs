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
    public class DiscountViewModel : ObservableObject
    {
        private readonly GGDealsWishlist _plugin;
        private CollectionViewSource _gamesViewSource;
        private GroupBy _groupBy = GroupBy.None;
        private string _searchTerm = string.Empty;
        private bool _showOnlyDiscountedGames = true;
        private bool _showOnlyHistoricalLowPrices = false;
        private SortOrder _sortOrder = SortOrder.Discount;

        public DiscountViewModel(GGDealsWishlist plugin)
        {
            _plugin = plugin;

            _groupBy = _plugin.Settings.Settings.DiscountViewSettings.GroupBy;
            _showOnlyDiscountedGames = _plugin.Settings.Settings.DiscountViewSettings.ShowOnlyDiscountedGames;
            _showOnlyHistoricalLowPrices = _plugin.Settings.Settings.DiscountViewSettings.ShowOnlyHistoricalLowPrices;
            _sortOrder = _plugin.Settings.Settings.DiscountViewSettings.SortOrder;

            PrepareGamesViewSource();
        }

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
        {
            var settings = _plugin.Settings.Settings.DiscountViewSettings;

            settings.GroupBy = GroupBy;
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

        public GroupBy GroupBy
        {
            get => _groupBy;
            set
            {
                SetValue(ref _groupBy, value);

                SortGroupFilter();
            }
        }

        public GroupByWithCaptions GroupByWithCaptions { get; } = new GroupByWithCaptions();

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
            if (GGDealsWishlist.Games is null || GGDealsWishlist.Games.Count == 0 || GGDealsWishlist.Games.LastRefresh < DateTime.Now.AddHours(-1))
            {
                RefreshGames();
            }

            Games = GGDealsWishlist.Games;

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
                    GGDealsWishlist.DataHandler.RefreshGames();
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

                switch (GroupBy)
                {
                    case GroupBy.Shop:
                        GamesViewSource.SortDescriptions.Add(new SortDescription("DiscountData.ShopName", ListSortDirection.Ascending));
                        break;

                    case GroupBy.CompletionStatus:
                        GamesViewSource.SortDescriptions.Add(new SortDescription("Game.CompletionStatus", ListSortDirection.Ascending));
                        break;
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

            switch (GroupBy)
            {
                case GroupBy.Shop:
                    GamesViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("DiscountData.ShopName"));
                    break;

                case GroupBy.CompletionStatus:
                    GamesViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("Game.CompletionStatus"));
                    break;

                default:
                    GamesViewSource.View.GroupDescriptions.Clear();
                    break;
            }

            GamesViewSource.View.Filter = Filter;
        }
    }
}
