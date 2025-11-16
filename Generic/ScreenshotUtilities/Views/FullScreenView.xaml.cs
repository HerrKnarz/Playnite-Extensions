using KNARZhelper.ScreenshotsCommon.Models;
using ScreenshotUtilities.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenshotUtilities.Views
{
    /// <summary>
    /// Interaction logic for FullScreenView.xaml
    /// </summary>
    public partial class FullScreenView : UserControl
    {
        public FullScreenView(ScreenshotUtilities plugin, ScreenshotGroup selectedGroup)
        {
            Loaded += FullScreenView_Loaded;
            InitializeComponent();
            DataContext = new FullScreenViewModel(plugin, selectedGroup);
            AddKeyBindings();
        }

        private void AddKeyBindings()
        {
            if (!(DataContext is FullScreenViewModel viewModel))
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

            FullScreenGrid.PreviewMouseWheel += WheelHandler;
        }

        private void FullScreenView_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }

        private void WheelHandler(object s, MouseWheelEventArgs e)
        {
            if (!(DataContext is FullScreenViewModel viewModel))
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
    }
}