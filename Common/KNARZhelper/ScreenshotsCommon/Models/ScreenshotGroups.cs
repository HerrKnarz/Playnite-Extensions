using KNARZhelper.FilesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace KNARZhelper.ScreenshotsCommon.Models
{
    /// <summary>
    /// Collection of screenshot groups.
    /// </summary>
    public class ScreenshotGroups : ObservableCollection<ScreenshotGroup>
    {
        /// <summary>
        /// Creates a new instance of the ScreenshotGroups class.
        /// </summary>
        public ScreenshotGroups()
        {
        }

        /// <summary>
        /// Creates a new instance of the ScreenshotGroups class and populates it from JSON files.
        /// </summary>
        /// <param name="basePath">Base path where the JSON files are located. This is the add-on data path containing folders for each game.</param>
        /// <param name="gameId">Unique identifier for the game. This is the name of the sub folder in the base path</param>
        public ScreenshotGroups(string basePath, Guid gameId)
        {
            CreateGroupsFromFiles(basePath, gameId);
        }

        /// <summary>
        /// Specifies whether all screenshots in all groups have been downloaded.
        /// </summary>
        [DontSerialize]
        public bool IsEverythingDownloaded => !this.Any(g => g.Screenshots.Any(s => !s.IsDownloaded || string.IsNullOrEmpty(s.DownloadedThumbnailPath)));

        /// <summary>
        /// Creates screenshot groups from JSON files located in the specified base path and game ID.
        /// </summary>
        /// <param name="basePath">Base path where the JSON files are located. This is the add-on data path containing folders for each game.</param>
        /// <param name="gameId">Unique identifier for the game. This is the name of the sub folder in the base path</param>
        /// <param name="createEmptyGroupOnError"></param>
        public void CreateGroupsFromFiles(string basePath, Guid gameId, bool createEmptyGroupOnError = true)
        {
            if (gameId == Guid.Empty)
            {
                if (createEmptyGroupOnError)
                {
                    Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoGameSelected")));
                }

                return;
            }

            var path = FileHelper.GetDownloadPath(basePath, gameId);

            if (!path.Exists)
            {
                if (createEmptyGroupOnError)
                {
                    Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoScreenshotsFound")));
                }

                return;
            }

            var files = path.EnumerateFiles("*.json", SearchOption.AllDirectories);

            if (!files.Any())
            {
                if (createEmptyGroupOnError)
                {
                    Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoScreenshotsFound")));
                }

                return;
            }

            foreach (var file in files)
            {
                try
                {
                    Add(ScreenshotGroup.CreateFromFile(file));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to load screenshots from file {file.FullName}");
                }
            }
        }

        /// <summary>
        /// Downloads all screenshots in all groups.
        /// </summary>
        public void DownloadAll()
        {
            foreach (var group in this)
            {
                group.Download();
                group.Save();
            }
        }

        /// <summary>
        /// Resets the collection by clearing all groups.
        /// </summary>
        public void Reset() => Clear();

    }
}
