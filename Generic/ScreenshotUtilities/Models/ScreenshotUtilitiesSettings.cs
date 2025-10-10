using Playnite.SDK.Data;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    public class ScreenshotUtilitiesSettings : ObservableObject
    {
        private bool _isControlVisible = false;

        [DontSerialize]
        public bool IsControlVisible
        {
            get => _isControlVisible;
            set => SetValue(ref _isControlVisible, value);
        }
    }
}
