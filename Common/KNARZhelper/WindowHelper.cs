using Playnite.SDK;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace KNARZhelper
{
    /// <summary>
    /// Helper class to create and manage windows in a standardized way.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Creates a dialog window with fixed size that can't be resized by the user.
        /// </summary>
        /// <param name="title">Title of the window</param>
        /// <returns>Created window</returns>
        public static Window CreateFixedDialog(string title)
        {
            var window = CreateSizeToContentWindow(title, 300, 50);
            window.ResizeMode = ResizeMode.NoResize;

            return window;
        }

        /// <summary>
        /// Creates a full screen window without borders and no way to resize or close it
        /// </summary>
        /// <returns>Created window</returns>
        public static Window CreateFullScreenWindow()
        {
            var window = new Window
            {
                Height = 1000,
                Width = 600,
                Owner = API.Instance.Dialogs.GetCurrentAppWindow(),
                ResizeMode = ResizeMode.NoResize,
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Colors.Black)
            };

            return window;
        }

        /// <summary>
        /// Creates a window with the given size. If widthToMax or heightToMax is true, the window will be as high or wide as the screen allows.
        /// </summary>
        /// <param name="title">Title of the window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
        /// <param name="widthToMax">Whether to stretch the window to the maximum width</param>
        /// <param name="heightToMax">Whether to stretch the window to the maximum height</param>
        /// <returns>Created window</returns>
        public static Window CreateSizedWindow(string title, int width, int height, bool widthToMax = false, bool heightToMax = false)
        {
            var window = CreateWindow(title);

            if (widthToMax || heightToMax)
            {
                var ioHelper = new WindowInteropHelper(window.Owner);
                var hWnd = ioHelper.Handle;
                var screen = Screen.FromHandle(hWnd);
                var dpi = VisualTreeHelper.GetDpi(window);

                if (heightToMax)
                {
                    window.MinHeight = height;
                    window.Height = screen.WorkingArea.Height * 0.96D / dpi.DpiScaleY;
                }
                else
                {
                    window.Height = height;
                }

                if (widthToMax)
                {
                    window.MinWidth = width;
                    window.Width = screen.WorkingArea.Width * 0.96D / dpi.DpiScaleY;
                }
                else
                {
                    window.Width = width;
                }
            }
            else
            {
                window.Width = width;
                window.Height = height;
            }

            PositionWindow(window);

            return window;
        }

        /// <summary>
        /// Creates a window that sizes itself to its content, but has a minimum size and won't exceed the screen size.
        /// </summary>
        /// <param name="title">Title of the window</param>
        /// <param name="minWidth">Minimum width of the window</param>
        /// <param name="minHeight">Minimum height of the window</param>
        /// <returns>Created window</returns>
        public static Window CreateSizeToContentWindow(string title, int minWidth = 500, int minHeight = 500)
        {
            var window = CreateWindow(title);

            var ioHelper = new WindowInteropHelper(window.Owner);
            var hWnd = ioHelper.Handle;
            var screen = Screen.FromHandle(hWnd);
            var dpi = VisualTreeHelper.GetDpi(window);

            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.MinHeight = minHeight;
            window.MaxHeight = screen.WorkingArea.Height * 0.96D / dpi.DpiScaleY;
            window.MinWidth = minWidth;
            window.MaxWidth = screen.WorkingArea.Width * 0.96D / dpi.DpiScaleY;

            PositionWindow(window);

            return window;
        }

        /// <summary>
        /// Creates a window with the given title and button configuration.
        /// </summary>
        /// <param name="title">Title of the window</param>
        /// <param name="showMaximizeButton">Whether to show the maximize button</param>
        /// <param name="showMinimizeButton">Whether to show the minimize button</param>
        /// <param name="showCloseButton">Whether to show the close button</param>
        /// <returns>Created window</returns>
        private static Window CreateWindow(string title, bool showMaximizeButton = false, bool showMinimizeButton = false, bool showCloseButton = true)
        {
            var window = API.Instance.Dialogs.CreateWindow(new WindowCreationOptions { ShowCloseButton = showCloseButton, ShowMaximizeButton = showMaximizeButton, ShowMinimizeButton = showMinimizeButton });
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.Title = title;

            return window;
        }

        /// <summary>
        /// Positions the window in the center of its owner.
        /// </summary>
        /// <param name="window">The window to position</param>
        private static void PositionWindow(Window window) => window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}