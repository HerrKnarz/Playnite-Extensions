using KNARZhelper;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;
using System;

namespace ScreenshotUtilities.Controls
{
    /// <summary>
    /// Interaction logic for ScreenshotViewerControl.xaml
    /// </summary>
    public partial class ScreenshotViewerControl : PluginUserControl
    {
        public ScreenshotViewerControl(ScreenshotUtilities plugin, Game game = null)
        {
            InitializeComponent();
            DataContext = new ScreenshotViewerViewModel(plugin, game);
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            LoadScreenshots();
        }

        public void LoadScreenshots()
        {
            try
            {
                if (!(DataContext is ScreenshotViewerViewModel viewModel))
                {
                    return;
                }

                if (viewModel.GameId == (GameContext?.Id ?? Guid.Empty))
                {
                    viewModel.LoadScreenshots();
                    return;
                }

                viewModel.GameId = GameContext?.Id ?? Guid.Empty;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
