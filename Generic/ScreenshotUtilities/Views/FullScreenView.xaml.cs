using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
            mediaElement.MediaEnded += MediaElement_OnMediaEnded;
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
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

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = new TimeSpan(0, 0, 1);
            mediaElement.Play();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!(DataContext is ScreenshotViewerViewModel viewModel))
            {
                e.Handled = true;
                return;
            }

            Log.Error(e.Exception, $"Error displaying file {viewModel.SelectedGroup?.SelectedScreenshot?.DisplayPath}");
            e.Handled = true;
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
