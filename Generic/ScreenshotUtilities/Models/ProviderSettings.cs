using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    public enum ScreenshotFetchMode
    {
        Always = 1,
        OnlyIfFirst = 2,
        OnlyManually = 3
    }

    public class ProviderSettings : ObservableObject
    {
        private bool _alwaysCreateThumbnails = false;
        private int _daysUntilRefresh = 5;
        private bool _downloadAutomatically = false;
        private ScreenshotFetchMode _fetchMode = ScreenshotFetchMode.Always;
        private int _priority = 1;
        private int _sortOrder = 1;
        private string _tagWhenHavingScreenshots = string.Empty;

        public bool AlwaysCreateThumbnails
        {
            get => _alwaysCreateThumbnails;
            set => SetValue(ref _alwaysCreateThumbnails, value);
        }

        public int DaysUntilRefresh
        {
            get => _daysUntilRefresh;
            set => SetValue(ref _daysUntilRefresh, value < 1 ? 1 : value);
        }

        public bool DownloadAutomatically
        {
            get => _downloadAutomatically;
            set => SetValue(ref _downloadAutomatically, value);
        }

        public ScreenshotFetchMode FetchMode
        {
            get => _fetchMode;
            set => SetValue(ref _fetchMode, value);
        }

        public int Priority
        {
            get => _priority;
            set => SetValue(ref _priority, value < 1 ? 1 : value);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value < 1 ? 1 : value);
        }

        public string TagWhenHavingScreenshots
        {
            get => _tagWhenHavingScreenshots;
            set => SetValue(ref _tagWhenHavingScreenshots, value);
        }
    }
}
