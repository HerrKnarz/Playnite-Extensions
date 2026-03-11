using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class FolderConfig : ObservableObject
    {
        private readonly Guid _gogId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        private readonly Guid _gogOssId = Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C");
        private readonly string _placeholderGameName = "{GameName}";
        private readonly string _placeholderGogId = "{GogId}";
        private readonly string _placeholderRomName = "{RomName}";
        private readonly string _placeholderSteamId = "{SteamId}";

        private bool _active = true;
        private string _fileMask = "*.jpg";
        private string _invalidCharReplacement = "_";
        private string _name = string.Empty;
        private string _path = string.Empty;
        private bool _removeDiacritics = false;
        private bool _removeEditionSuffix = false;
        private bool _removeHyphens = false;
        private bool _removeSpecialChars = false;
        private bool _removeWhitespaces = false;
        private bool _underscoresToWhitespaces = false;
        private bool _whitespacesToHyphens = false;
        private bool _whitespacesToUnderscores = false;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }

        public string FileMask
        {
            get => _fileMask; set
            {
                _fileMask = value;
                OnPropertyChanged();
            }
        }

        public string InvalidCharReplacement
        {
            get => _invalidCharReplacement; set
            {
                _invalidCharReplacement = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name; set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get => _path; set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        public bool RemoveDiacritics
        {
            get => _removeDiacritics; set
            {
                _removeDiacritics = value;
                OnPropertyChanged();
            }
        }

        public bool RemoveEditionSuffix
        {
            get => _removeEditionSuffix; set
            {
                _removeEditionSuffix = value;
                OnPropertyChanged();
            }
        }

        public bool RemoveHyphens
        {
            get => _removeHyphens; set
            {
                _removeHyphens = value;
                OnPropertyChanged();
            }
        }

        public bool RemoveSpecialChars
        {
            get => _removeSpecialChars; set
            {
                _removeSpecialChars = value;
                OnPropertyChanged();
            }
        }

        public bool RemoveWhitespaces
        {
            get => _removeWhitespaces; set
            {
                _removeWhitespaces = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SelectFolderCommand => new RelayCommand(() =>
        {
            var path = API.Instance.Dialogs.SelectFolder(Path);

            if (!string.IsNullOrEmpty(path))
            {
                Path = path;
            }
        });

        public bool UnderscoresToWhitespaces
        {
            get => _underscoresToWhitespaces; set
            {
                _underscoresToWhitespaces = value;
                OnPropertyChanged();
            }
        }

        public bool WhitespacesToHyphens
        {
            get => _whitespacesToHyphens; set
            {
                _whitespacesToHyphens = value;
                OnPropertyChanged();
            }
        }

        public bool WhitespacesToUnderscores
        {
            get => _whitespacesToUnderscores; set
            {
                _whitespacesToUnderscores = value;
                OnPropertyChanged();
            }
        }

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

            return gameName.ReplaceInvalidFileNameChars(InvalidCharReplacement);
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
