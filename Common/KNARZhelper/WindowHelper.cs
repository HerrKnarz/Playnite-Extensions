using Playnite.SDK;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace KNARZhelper
{
    public static class WindowHelper
    {
        private static Window CreateWindow(string title, bool showMaximizeButton = false, bool showMinimizeButton = false)
        {
            Window window = API.Instance.Dialogs.CreateWindow(new WindowCreationOptions { ShowCloseButton = true, ShowMaximizeButton = showMaximizeButton, ShowMinimizeButton = showMinimizeButton });
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.Title = title;

            return window;
        }

        private static void PositionWindow(Window window) => window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        public static Window CreateSizeToContentWindow(string title, int minWidth = 500, int minHeight = 500)
        {
            Window window = CreateWindow(title);

            WindowInteropHelper ioHelper = new WindowInteropHelper(window.Owner);
            IntPtr hWnd = ioHelper.Handle;
            Screen screen = Screen.FromHandle(hWnd);
            DpiScale dpi = VisualTreeHelper.GetDpi(window);

            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.MinHeight = minHeight;
            window.MaxHeight = screen.WorkingArea.Height * 0.96D / dpi.DpiScaleY;
            window.MinWidth = minWidth;
            window.MaxWidth = screen.WorkingArea.Width * 0.96D / dpi.DpiScaleY;

            PositionWindow(window);

            return window;
        }

        public static Window CreateSizedWindow(string title, int width, int height, bool widthToMax = false, bool heighttoMax = false)
        {
            Window window = CreateWindow(title);

            if (widthToMax || heighttoMax)
            {
                WindowInteropHelper ioHelper = new WindowInteropHelper(window.Owner);
                IntPtr hWnd = ioHelper.Handle;
                Screen screen = Screen.FromHandle(hWnd);
                DpiScale dpi = VisualTreeHelper.GetDpi(window);

                if (heighttoMax)
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
    }
}