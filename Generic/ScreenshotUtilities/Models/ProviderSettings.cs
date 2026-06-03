using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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
        private bool? _alwaysCreateThumbnails = false;
        private int _daysUntilRefresh = 5;
        private bool _downloadAutomatically = false;
        private ScreenshotFetchMode _fetchMode = ScreenshotFetchMode.Always;
        private int _priority = 1;
        private int _sortOrder = 1;
        private string _tagWhenHavingScreenshots = string.Empty;

        public RelayCommand AddTagToAddWhenHavingScreenshotsCommand
            => new RelayCommand(() =>
                {
                    var typeManager = new TypeTag();
                    var label = typeManager.LabelPlural;
                    var items = new ObservableCollection<BaseMetadataObject>();

                    typeManager.LoadAllMetadata(new HashSet<System.Guid>()).ForEach(item => items.Add(
                                    new BaseMetadataObject(typeManager, typeManager.Type, item.Name)
                                    {
                                        Id = item.Id
                                    }));

                    items.Sort(i => i.Name);

                    SelectMetadataViewModel.GetWindow(items, label, false)?.ShowDialog();

                    if (items.Count(i => i.Selected) == 0)
                    {
                        return;
                    }

                    TagWhenHavingScreenshots = items.First(i => i.Selected).Name;
                });

        public bool AlwaysCreateThumbnails
        {
            get => _alwaysCreateThumbnails ?? IsLocal;
            set => SetValue(ref _alwaysCreateThumbnails, value);
        }

        public int DaysUntilRefresh
        {
            get => _daysUntilRefresh;
            set => SetValue(ref _daysUntilRefresh, value < 0 ? 0 : value);
        }

        public bool DownloadAutomatically
        {
            get => _downloadAutomatically;
            set => SetValue(ref _downloadAutomatically, value);
        }

        [DontSerialize]
        public Visibility DownloadAutomaticallyVisibility => IsLocal ? Visibility.Collapsed : Visibility.Visible;

        public ScreenshotFetchMode FetchMode
        {
            get => _fetchMode;
            set => SetValue(ref _fetchMode, value);
        }

        [DontSerialize]
        public bool IsLocal { get; set; }

        public int Priority
        {
            get => _priority;
            set => SetValue(ref _priority, value < 1 ? 1 : value);
        }

        [DontSerialize]
        public string ProviderIcon => IsLocal ? "\xf103" : "\xf102";

        [DontSerialize]
        public ScreenshotFetchModesWithCaptions ScreenshotFetchModesWithCaptions { get; } = new ScreenshotFetchModesWithCaptions();

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

        [DontSerialize]
        public string ThumbnailDescription => IsLocal
            ? ResourceProvider.GetString("LOCScreenshotUtilitiesSettingsAlwaysCreateThumbnailsDescriptionLocal")
            : ResourceProvider.GetString("LOCScreenshotUtilitiesSettingsAlwaysCreateThumbnailsDescription");

        //NEXT Update font and use it here for the icons.
    }

    /// <summary>
    /// Dictionary of types with captions to show in a combo box.
    /// </summary>
    public class ScreenshotFetchModesWithCaptions : Dictionary<ScreenshotFetchMode, string>
    {
        public ScreenshotFetchModesWithCaptions()
        {
            Add(ScreenshotFetchMode.Always, ResourceProvider.GetString("LOCScreenshotUtilitiesSettingsFetchModeAlways"));
            Add(ScreenshotFetchMode.OnlyIfFirst, ResourceProvider.GetString("LOCScreenshotUtilitiesSettingsFetchModeOnlyIfFirst"));
            Add(ScreenshotFetchMode.OnlyManually, ResourceProvider.GetString("LOCScreenshotUtilitiesSettingsFetchModeOnlyManually"));
        }
    }
}
