using KNARZhelper;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;
using System;

namespace ScreenshotUtilities.Controls
{
    /// <summary>
    /// Interaction logic for ButtonControl.xaml
    /// </summary>
    public partial class ButtonControl : PluginUserControl
    {
        public ButtonControl(ScreenshotUtilities plugin)
        {
            DataContext = new ButtonControlViewModel(plugin);
            InitializeComponent();
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            LoadButton();
        }

        public void LoadButton()
        {
            try
            {
                if (!(DataContext is ButtonControlViewModel viewModel))
                {
                    return;
                }

                if ((viewModel.Game?.Id ?? Guid.Empty) == (GameContext?.Id ?? Guid.Empty))
                {
                    viewModel.Refresh();
                    return;
                }

                viewModel.Game = GameContext;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
