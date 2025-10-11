using Playnite.SDK.Data;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    public class Settings : ObservableObject
    {
        private int _aspectHeight = 9;
        private int _aspectWidth = 16;
        private bool _isControlVisible = false;
        private bool _displayButtonControl = true;
        private bool _displayViewerControl = true;
        private int _viewerWindowHeight = 700;
        private int _viewerWindowWidth = 800;

        public int AspectHeight
        {
            get => _aspectHeight;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                SetValue(ref _aspectHeight, value);
            }
        }

        public int AspectWidth
        {
            get => _aspectWidth;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                SetValue(ref _aspectWidth, value);
            }
        }

        public bool DisplayButtonControl
        {
            get => _displayButtonControl;
            set => SetValue(ref _displayButtonControl, value);
        }

        public bool DisplayViewerControl
        {
            get => _displayViewerControl;
            set => SetValue(ref _displayViewerControl, value);
        }

        [DontSerialize]
        public bool IsControlVisible
        {
            get => _isControlVisible;
            set => SetValue(ref _isControlVisible, value);
        }

        public int ViewerWindowWidth
        {
            get => _viewerWindowWidth;
            set
            {
                if (value < 100)
                {
                    value = 100;
                }

                SetValue(ref _viewerWindowWidth, value);
            }
        }

        public int ViewerWindowHeight
        {
            get => _viewerWindowHeight;
            set
            {
                if (value < 100)
                {
                    value = 100;
                }

                SetValue(ref _viewerWindowHeight, value);
            }
        }
    }
}
