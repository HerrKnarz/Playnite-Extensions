using GGDealsWishlist.Models;
using GGDealsWishlist.Views;
using KNARZhelper;
using Playnite.SDK;
using System;

namespace GGDealsWishlist.ViewModels
{
    public class DiscountViewModel
    {
        public DiscountViewModel()
        {
            if (IsInDesignMode)
            {
            }
        }

        public GGDealsGames Games { get; set; }

        public bool IsInDesignMode { get; private set; }

        public GGDealsGame SelectedGame { get; set; }

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

                var view = new DiscountView();

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCGGDealsWishlistMenuDiscountView"),
                    800,
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
    }
}
