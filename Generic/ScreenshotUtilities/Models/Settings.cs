using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenshotUtilities.Models
{
    public class Settings : ObservableObject
    {
        private int _aspectHeight = 9;
        private int _aspectWidth = 16;
        private ScreenshotGroups _currentScreenshotGroups = new ScreenshotGroups();
        private bool _debug = true;
        private bool _displayButtonControl = true;
        private bool _displayViewerControl = true;
        private ObservableCollection<MetadataObject> _downloadFilter = new ObservableCollection<MetadataObject>();
        private bool _isButtonControlVisible = false;
        private bool _isViewerControlVisible = false;
        private Dictionary<string, ProviderSettings> _providerSettings = new Dictionary<string, ProviderSettings>();
        private int _thumbnailHeight = 120;
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

        [DontSerialize]
        public ScreenshotGroup CurrentLocalScreenshotGroup
            => _currentScreenshotGroups.FirstOrDefault(g => g.Provider.Id == Guid.Parse("a049eff8-fd41-4dbc-9e35-01acc6b1a0cb"));

        [DontSerialize]
        public ScreenshotGroups CurrentScreenshotGroups
        {
            get => _currentScreenshotGroups; set => SetValue(ref _currentScreenshotGroups, value);
        }

        public bool Debug
        {
            get => _debug;
            set => SetValue(ref _debug, value);
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

        public ObservableCollection<MetadataObject> DownloadFilter
        {
            get => _downloadFilter;
            set => SetValue(ref _downloadFilter, value);
        }

        [DontSerialize]
        public bool HasLocalScreenshots
           => !(CurrentLocalScreenshotGroup is null) && CurrentLocalScreenshotGroup.HasScreenshots;

        [DontSerialize]
        public bool IsButtonControlVisible
        {
            get => _isButtonControlVisible;
            set => SetValue(ref _isButtonControlVisible, value);
        }

        [DontSerialize]
        public bool IsViewerControlVisible
        {
            get => _isViewerControlVisible;
            set => SetValue(ref _isViewerControlVisible, value);
        }

        public Dictionary<string, ProviderSettings> ProviderSettings
        {
            get => _providerSettings;
            set => SetValue(ref _providerSettings, value);
        }

        public int ThumbnailHeight
        {
            get => _thumbnailHeight;
            set
            {
                if (value < 50)
                {
                    value = 50;
                }

                SetValue(ref _thumbnailHeight, value);
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
    }
}
