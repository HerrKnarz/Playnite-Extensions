using Playnite.SDK;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace KNARZhelper
{
    public static class WindowHelper
    {
        public static Window CreateFixedDialog(string title)
        {
            var window = CreateSizeToContentWindow(title, 300, 50);
            window.ResizeMode = ResizeMode.NoResize;

            return window;
        }

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

        private static Window CreateWindow(string title, bool showMaximizeButton = false, bool showMinimizeButton = false)
        {
            var window = API.Instance.Dialogs.CreateWindow(new WindowCreationOptions { ShowCloseButton = true, ShowMaximizeButton = showMaximizeButton, ShowMinimizeButton = showMinimizeButton });
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.Title = title;

            return window;
        }

        private static void PositionWindow(Window window) => window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}