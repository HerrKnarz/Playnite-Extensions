using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenshotUtilities.Models
{
    public class Settings : ObservableObject
    {
        private int _aspectHeight = 9;
        private int _aspectWidth = 16;
        private bool _automaticDownload = false;
        private bool _displayButtonControl = true;
        private bool _displayViewerControl = true;
        private ObservableCollection<MetadataObject> _downloadFilter = new ObservableCollection<MetadataObject>();
        private bool _isViewerControlVisible = false;
        private bool _isButtonControlVisible = false;
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

        public bool AutomaticDownload
        {
            get => _automaticDownload;
            set => SetValue(ref _automaticDownload, value);
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
        public bool IsViewerControlVisible
        {
            get => _isViewerControlVisible;
            set => SetValue(ref _isViewerControlVisible, value);
        }

        [DontSerialize]
        public bool IsButtonControlVisible
        {
            get => _isButtonControlVisible;
            set => SetValue(ref _isButtonControlVisible, value);
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
