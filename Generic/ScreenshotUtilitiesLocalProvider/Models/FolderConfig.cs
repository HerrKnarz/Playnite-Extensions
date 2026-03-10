using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class FolderConfig
    {
        private readonly Guid _gogId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        private readonly Guid _gogOssId = Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C");
        private readonly string _placeholderGameName = "{GameName}";
        private readonly string _placeholderGogId = "{GogId}";
        private readonly string _placeholderRomName = "{RomName}";
        private readonly string _placeholderSteamId = "{SteamId}";
        public bool Active { get; set; } = true;
        public bool EscapeDataString { get; set; } = false;
        public string FileMask { get; set; } = "*.jpg";
        public string Name { get; set; }
        public string Path { get; set; }
        public bool RemoveDiacritics { get; set; } = false;
        public bool RemoveEditionSuffix { get; set; } = false;
        public bool RemoveHyphens { get; set; } = false;
        public bool RemoveSpecialChars { get; set; } = false;
        public bool RemoveWhitespaces { get; set; } = false;
        public bool ReturnsSameUrl { get; set; } = false;
        public bool UnderscoresToWhitespaces { get; set; } = false;
        public bool UrlEncode { get; set; } = false;
        public bool WhitespacesToHyphens { get; set; } = false;
        public bool WhitespacesToUnderscores { get; set; } = false;

        public string FormatGameName(string gameName)
        {
            if (RemoveEditionSuffix)
            {
                gameName = gameName.RemoveEditionSuffix();
            }

            if (RemoveHyphens)
            {
                gameName = gameName.Replace("-", "");
            }

            if (UnderscoresToWhitespaces)
            {
                gameName = gameName.Replace("_", " ");
            }

            if (RemoveSpecialChars)
            {
                gameName = gameName.RemoveSpecialChars();
            }

            if (RemoveDiacritics)
            {
                gameName = gameName.RemoveDiacritics();
            }

            gameName = RemoveWhitespaces ? gameName.Replace(" ", "") : gameName.CollapseWhitespaces();

            if (WhitespacesToHyphens)
            {
                gameName = gameName.Replace(" ", "-");
            }

            if (WhitespacesToUnderscores)
            {
                gameName = gameName.Replace(" ", "_");
            }

            if (EscapeDataString)
            {
                gameName = gameName.EscapeDataString();
            }

            if (UrlEncode)
            {
                gameName = gameName.UrlEncode();
            }

            return gameName;
        }

        public List<Screenshot> LoadScreenshots(Game game)
        {
            var folder = ReplacePlaceholders(Path, game);
            var fileMask = ReplacePlaceholders(FileMask, game);

            var result = new List<Screenshot>();

            try
            {
                var dirInfo = new DirectoryInfo(folder);

                if (!dirInfo.Exists)
                {
                    return result;
                }

                var files = dirInfo.GetFiles(fileMask).OrderBy(f => f.Name).ToList();

                if (files.Count == 0)
                {
                    return result;
                }

                // TODO: Think about a good way to allow separate thumbnails without two paths and file masks if possible.
                result.AddRange(files.Select(file => new Screenshot(file.FullName)
                {
                    ThumbnailPath = file.FullName,
                    SortOrder = files.IndexOf(file),
                    Type = MediaType.SelfmadeScreenshot,
                    Name = $"{Name}: {file.Name}"
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading screenshots from {Name} for {game.Name}");
            }

            return result;
        }

        private string ReplacePlaceholders(string path, Game game)
        {
            var result = path;

            if (result.Contains(_placeholderGameName))
            {
                var gameName = FormatGameName(game.Name);

                if (string.IsNullOrEmpty(gameName))
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderGameName, gameName);
            }

            if (result.Contains(_placeholderSteamId))
            {
                var steamId = SteamHelper.GetSteamId(game);

                if (string.IsNullOrEmpty(steamId))
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderSteamId, steamId);
            }

            if (result.Contains(_placeholderGogId))
            {
                if (!game.PluginId.IsOneOf(_gogId, _gogOssId))
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderGogId, game.GameId);
            }

            if (result.Contains(_placeholderRomName))
            {
                if (game.IsInstalled && (game.Roms?.Any() ?? false))
                {
                    result = result.Replace(_placeholderRomName, System.IO.Path.GetFileNameWithoutExtension(game.Roms[0].Path));
                }
                else
                {
                    return string.Empty;
                }
            }

            return result;
        }
    }
}
