using KNARZhelper;
using KNARZhelper.FilesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ScreenshotUtilities.Models
{
    internal class ScreenshotGroups : ObservableCollection<ScreenshotGroup>
    {
        public ScreenshotGroups()
        {
        }

        public ScreenshotGroups(string basePath, Guid gameId)
        {
            CreateGroupsFromFiles(basePath, gameId);
        }

        [DontSerialize]
        public bool IsEverythingDownloaded => !this.Any(g => g.Screenshots.Any(s => !s.IsDownloaded || string.IsNullOrEmpty(s.DownloadedThumbnailPath)));

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

        public void DownloadAll()
        {
            foreach (var group in this)
            {
                group.Download();
                group.Save();
            }
        }

        public void Reset() => Clear();

    }
}
