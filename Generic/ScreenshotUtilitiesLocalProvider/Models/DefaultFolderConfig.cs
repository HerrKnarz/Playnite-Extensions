using System.Collections.Generic;
using System.Windows.Media;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class CommonConfigs : List<DefaultFolderConfig>
    {
        public CommonConfigs()
        {
            Add(new DefaultFolderConfig()
            {
                Name = "Custom Folder",
                Icon = "\xf100",
                IconColor = Color.FromRgb(255, 255, 255),
                Path = "",
                FileMask = "*.jpg",
                InvalidCharReplacement = "",
                IsCustom = true
            });

            Add(new DefaultFolderConfig()
            {
                Name = "Steam",
                Icon = "\xf000",
                IconColor = Color.FromRgb(102, 192, 244),
                Path = "{SteamScreenshotsDir}\\{SteamId}\\screenshots",
                FileMask = "*.jpg",
                InvalidCharReplacement = ""
            });

            Add(new DefaultFolderConfig()
            {
                Name = "GOG Galaxy",
                Icon = "\xf001",
                IconColor = Color.FromRgb(125, 66, b: 151),
                Path = "{GogScreenshotDir}\\{GameName}",
                FileMask = "*.jpg",
                InvalidCharReplacement = ""
            });

            Add(new DefaultFolderConfig()
            {
                Name = "Ubisoft",
                Icon = "\xf002",
                IconColor = Color.FromRgb(127, 75, 34),
                Path = "{UbisoftScreenshotsDir}{GameName}",
                FileMask = "*",
                InvalidCharReplacement = ""
            });

            Add(new DefaultFolderConfig()
            {
                Name = "RetroArch",
                Icon = "\xf003",
                IconColor = Color.FromRgb(255, 255, 255),
                Path = "{RetroArchScreenshotsDir}",
                FileMask = "{ImageNameNoExt}-*",
                InvalidCharReplacement = ""
            });

            Add(new DefaultFolderConfig()
            {
                Name = "ScummVM",
                Icon = "\xf004",
                IconColor = Color.FromRgb(0, 200, 50),
                Path = "{UserProfile}\\Pictures\\ScummVM Screenshots",
                FileMask = "scummvm-{ImageNameNoExt}-*",
                InvalidCharReplacement = ""
            });

            Add(new DefaultFolderConfig()
            {
                Name = "Xbox Game Bar",
                Icon = "\xf006",
                IconColor = Color.FromRgb(16, 124, 16),
                Path = "{XboxGamebarScreenshotsDir}",
                FileMask = "{GameName} *.png",
                InvalidCharReplacement = ""
            });
        }
    }

    public class DefaultFolderConfig : FolderConfig
    {
        public string Icon { get; set; }

        public Brush IconBrush => new SolidColorBrush(IconColor);

        public Color IconColor { get; set; }

        public bool IsCustom { get; set; } = false;
    }
}
