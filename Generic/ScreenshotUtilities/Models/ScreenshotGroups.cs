using KNARZhelper;
using Playnite.SDK;
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

        public void Reset() => Clear();

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
    }
}
