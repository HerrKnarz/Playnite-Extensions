using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilities.Models
{
    public class ScreenshotGroup : ObservableObject
    {
        private string _description;
        private Guid _id = Guid.NewGuid();
        private string _name;
        private ScreenshotProvider _provider;
        private RangeObservableCollection<Screenshot> _screenshots = new RangeObservableCollection<Screenshot>();
        private Screenshot _selectedScreenshot;
        private int _sortOrder = 0;
        private string _basePath;
        private string _fileName;

        public ScreenshotGroup(string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _name = name;
        }

        public static ScreenshotGroup CreateFromFile(FileInfo file)
        {
            var group = Serialization.FromJsonFile<ScreenshotGroup>(file.FullName);
            group.BasePath = file.DirectoryName;
            group.FileName = file.FullName;
            return group;
        }

        public void Download()
        {
            var globalProgressOptions = new GlobalProgressOptions(
                $"{ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadingScreenshots")} {Name}",
                true
            )
            {
                IsIndeterminate = false
            };

            API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    activateGlobalProgress.ProgressMaxValue = Screenshots.Where(s => s.PathIsUrl && !s.IsDownloaded).Count();

                    foreach (var screenshot in Screenshots)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!screenshot.PathIsUrl || screenshot.IsDownloaded)
                        {
                            continue;
                        }

                        // Create file path and ensure directory exists
                        var path = Path.Combine(BasePath, $"{screenshot.Id}{FileHelper.GetFileExtensionFromUrl(screenshot.Path)}");

                        FileDownloader.Instance().DownloadFileAsync(path, new Uri(screenshot.Path)).Wait();

                        screenshot.DownloadedPath = path;

                        activateGlobalProgress.CurrentProgressValue++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }

            var serializedData = Serialization.ToJson(this);
            FileHelper.WriteStringToFile(FileName, serializedData, true);
        }

        public void SelectNextScreenshot()
        {
            if (Screenshots is null || Screenshots.Count == 0 || SelectedScreenshot is null)
            {
                return;
            }

            var index = Screenshots.IndexOf(SelectedScreenshot) + 1;

            if (index >= Screenshots.Count)
            {
                index = 0;
            }

            SelectedScreenshot = Screenshots[index];
        }

        public void SelectPreviousScreenshot()
        {
            if (Screenshots is null || Screenshots.Count == 0 || SelectedScreenshot is null)
            {
                return;
            }

            var index = Screenshots.IndexOf(SelectedScreenshot) - 1;

            if (index < 0)
            {
                index = Screenshots.Count - 1;
            }

            SelectedScreenshot = Screenshots[index];
        }

        [DontSerialize]
        public string BasePath
        {
            get => _basePath;
            set => SetValue(ref _basePath, value);
        }

        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        [DontSerialize]
        public string DisplayName => Provider == null || string.IsNullOrEmpty(Provider.Name) ? Name
            : string.IsNullOrEmpty(Name) ? (Provider?.Name)
            : $"{Provider?.Name}: {Name}";

        [DontSerialize]
        public string FileName
        {
            get => _fileName;
            set => SetValue(ref _fileName, value);
        }

        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [SerializationPropertyName("provider")]
        public ScreenshotProvider Provider
        {
            get => _provider;
            set => SetValue(ref _provider, value);
        }

        [SerializationPropertyName("screenshots")]
        public RangeObservableCollection<Screenshot> Screenshots
        {
            get => _screenshots;
            set
            {
                SetValue(ref _screenshots, value);

                SelectedScreenshot = value != null && value.Count > 0 ? value[0] : null;
            }
        }

        [DontSerialize]
        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set => SetValue(ref _selectedScreenshot, value);
        }

        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }
    }
}
