using KNARZhelper.FilesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KNARZhelper.ScreenshotsCommon.Models
{
    /// <summary>
    /// Class representing a group of screenshots with properties and methods for managing them.
    /// </summary>
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

        /// <summary>
        /// Creates a new instance of the ScreenshotGroup class.
        /// </summary>
        /// <param name="name">Name of the group. Will be displayed in the UI.</param>
        /// <param name="id">Unique identifier for the group.</param>
        public ScreenshotGroup(string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _name = name;
        }

        /// <summary>
        /// Creates a ScreenshotGroup instance from a JSON file with the same structure.
        /// </summary>
        /// <param name="file">The JSON file to read from.</param>
        /// <returns>A ScreenshotGroup instance.</returns>
        public static ScreenshotGroup CreateFromFile(FileInfo file)
        {
            var group = Serialization.FromJsonFile<ScreenshotGroup>(file.FullName);
            group.BasePath = file.DirectoryName;
            group.FileName = file.FullName;
            return group;
        }

        /// <summary>
        /// Downloads all screenshots in the group.
        /// </summary>
        public void Download()
        {
            var globalProgressOptions = new GlobalProgressOptions(
                $"{ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadingScreenshots")} {DisplayName}",
                true
            )
            {
                IsIndeterminate = false
            };

            API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    activateGlobalProgress.ProgressMaxValue = Screenshots.Count();

                    foreach (var screenshot in Screenshots)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        screenshot.Download(BasePath);

                        screenshot.GenerateThumbnail();

                        activateGlobalProgress.CurrentProgressValue++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);
        }

        /// <summary>
        /// Saves the ScreenshotGroup to its associated JSON file.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }

            var serializedData = Serialization.ToJson(this);
            FileHelper.WriteStringToFile(FileName, serializedData, true);
        }

        /// <summary>
        /// Selects the next screenshot in the group. Loops back to the first screenshot if at the end.
        /// </summary>
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

        /// <summary>
        /// Selects the previous screenshot in the group. Loops back to the last screenshot if at the beginning.
        /// </summary>
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

        /// <summary>
        /// Base path where screenshots are downloaded. This is also where the JSON file is stored.
        /// </summary>
        [DontSerialize]
        public string BasePath
        {
            get => _basePath;
            set => SetValue(ref _basePath, value);
        }

        /// <summary>
        /// Description of the screenshot group.
        /// </summary>
        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        /// <summary>
        /// Display name of the screenshot group, combining provider name and group name.
        /// </summary>
        [DontSerialize]
        public string DisplayName => Provider == null || string.IsNullOrEmpty(Provider.Name) ? Name
            : string.IsNullOrEmpty(Name) ? (Provider?.Name)
            : $"{Provider?.Name}: {Name}";

        /// <summary>
        /// Gets or sets the name of the JSON file associated with this instance.
        /// </summary>
        [DontSerialize]
        public string FileName
        {
            get => _fileName;
            set => SetValue(ref _fileName, value);
        }

        /// <summary>
        /// Unique identifier for the screenshot group.
        /// </summary>
        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the name of the screenshot group. Will be displayed in the UI.
        /// </summary>
        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the provider of the screenshots in this group. This usually is the playnite add-on providing the screenshots.
        /// </summary>
        [SerializationPropertyName("provider")]
        public ScreenshotProvider Provider
        {
            get => _provider;
            set => SetValue(ref _provider, value);
        }

        /// <summary>
        /// Collection of screenshots in this group.
        /// </summary>
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

        /// <summary>
        /// Currently selected screenshot in the group.
        /// </summary>
        [DontSerialize]
        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set => SetValue(ref _selectedScreenshot, value);
        }

        /// <summary>
        /// Gets or sets the sort order of the screenshot group in a list of groups.
        /// </summary>
        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }
    }
}
