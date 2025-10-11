using Playnite.SDK.Data;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    public class ScreenshotUtilitiesSettings : ObservableObject
    {
        private bool _isControlVisible = false;
        private bool _displayButtonControl = true;
        private bool _displayViewerControl = true;

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
    }
}
