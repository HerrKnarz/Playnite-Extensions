using KNARZhelper;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Windows.Input;

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
            AddKeyBindings();
        }

        private void AddKeyBindings()
        {
            if (!(DataContext is ScreenshotViewerViewModel viewModel))
            {
                return;
            }

            var leftKeyBinding = new KeyBinding

            {
                Key = Key.Left,
                Command = viewModel.SelectPreviousScreenshotCommand
            };

            InputBindings.Add(leftKeyBinding);

            var rightKeyBinding = new KeyBinding

            {
                Key = Key.Right,
                Command = viewModel.SelectNextScreenshotCommand
            };

            InputBindings.Add(rightKeyBinding);

            ScreenshotListBox.PreviewMouseWheel += WheelHandler;
        }

        private void WheelHandler(object s, MouseWheelEventArgs e)
        {
            if (!(DataContext is ScreenshotViewerViewModel viewModel))
            {
                return;
            }

            if (e.Delta > 0) // Scroll up
            {
                if (viewModel.SelectPreviousScreenshotCommand?.CanExecute(null) == true)
                {
                    viewModel.SelectPreviousScreenshotCommand.Execute(null);
                }

                e.Handled = true;
            }
            else if (e.Delta < 0) // Scroll down
            {
                if (viewModel.SelectNextScreenshotCommand?.CanExecute(null) == true)
                {
                    viewModel.SelectNextScreenshotCommand.Execute(null);
                }

                e.Handled = true;
            }
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
