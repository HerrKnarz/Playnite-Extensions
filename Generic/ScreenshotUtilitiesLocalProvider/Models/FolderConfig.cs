using KNARZhelper;
using KNARZhelper.GamesCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class FolderConfig : ObservableObject
    {
        private bool _active = true;
        private string _exampleName = "Baldur's Gate 3";
        private string _exampleResult = "";
        private string _fileMask = "*.jpg";
        private string _invalidCharReplacement = "_";
        private string _name = string.Empty;
        private string _path = string.Empty;
        private bool _removeDiacritics = false;
        private bool _removeEditionSuffix = false;
        private bool _removeHyphens = false;
        private bool _removeSpecialChars = false;
        private bool _removeWhitespaces = false;
        private string _resolvedFileMask = string.Empty;
        private string _resolvedPath = string.Empty;
        private GameEx _testGame = new GameEx();
        private bool _underscoresToWhitespaces = false;
        private bool _whitespacesToHyphens = false;
        private bool _whitespacesToUnderscores = false;

        public bool Active
        {
            get => _active;
            set => SetValue(ref _active, value);
        }

        [DontSerialize]
        public string ExampleName
        {
            get => _exampleName;
            set
            {
                SetValue(ref _exampleName, value);
                ResolveFormat();
            }
        }

        [DontSerialize]
        public string ExampleResult
        {
            get => _exampleResult;
            set => SetValue(ref _exampleResult, value);
        }

        public string FileMask
        {
            get => _fileMask;
            set => SetValue(ref _fileMask, value);
        }

        public string InvalidCharReplacement
        {
            get => _invalidCharReplacement;
            set => SetValue(ref _invalidCharReplacement, value);
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public RelayCommand OpenResolvedFolderCommand => new RelayCommand(() =>
        {
            if (new FileInfo(ResolvedPath).Directory.Exists)
            {
                Process.Start("explorer.exe", ResolvedPath);

                return;
            }

            API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCScreenshotUtilitiesLocalProviderSettingsPathDoesntExist"));
        });

        public string Path
        {
            get => _path;
            set => SetValue(ref _path, value);
        }

        public bool RemoveDiacritics
        {
            get => _removeDiacritics;
            set => SetValue(ref _removeDiacritics, value);
        }

        public bool RemoveEditionSuffix
        {
            get => _removeEditionSuffix;
            set => SetValue(ref _removeEditionSuffix, value);
        }

        public bool RemoveHyphens
        {
            get => _removeHyphens;
            set => SetValue(ref _removeHyphens, value);
        }

        public bool RemoveSpecialChars
        {
            get => _removeSpecialChars;
            set => SetValue(ref _removeSpecialChars, value);
        }

        public bool RemoveWhitespaces
        {
            get => _removeWhitespaces;
            set => SetValue(ref _removeWhitespaces, value);
        }

        [DontSerialize]
        public string ResolvedFileMask
        {
            get => _resolvedFileMask;
            set => SetValue(ref _resolvedFileMask, value);
        }

        [DontSerialize]
        public string ResolvedPath
        {
            get => _resolvedPath;
            set => SetValue(ref _resolvedPath, value);
        }

        public RelayCommand SelectFolderCommand => new RelayCommand(() =>
        {
            var path = API.Instance.Dialogs.SelectFolder(Path);

            if (!string.IsNullOrEmpty(path))
            {
                Path = path;
            }
        });

        [DontSerialize]
        public StringExpander StringExpander { get; set; }

        [DontSerialize]
        public GameEx TestGame
        {
            get => _testGame;
            set
            {
                SetValue(ref _testGame, value);
                ExampleName = _testGame.Game.Name;
                ResolveConfig();
            }
        }

        public bool UnderscoresToWhitespaces
        {
            get => _underscoresToWhitespaces;
            set => SetValue(ref _underscoresToWhitespaces, value);
        }

        public bool WhitespacesToHyphens
        {
            get => _whitespacesToHyphens;
            set => SetValue(ref _whitespacesToHyphens, value);
        }

        public bool WhitespacesToUnderscores
        {
            get => _whitespacesToUnderscores;
            set => SetValue(ref _whitespacesToUnderscores, value);
        }

        public string FormatGameName(string gameName)
        {
            var formatParameters = new StringFormatParameters
            {
                InvalidCharReplacement = InvalidCharReplacement,
                RemoveDiacritics = RemoveDiacritics,
                RemoveEditionSuffix = RemoveEditionSuffix,
                RemoveHyphens = RemoveHyphens,
                RemoveSpecialChars = RemoveSpecialChars,
                RemoveWhitespaces = RemoveWhitespaces,
                ReplaceInvalidFileNameChars = true,
                UnderscoresToWhitespaces = UnderscoresToWhitespaces,
                WhitespacesToHyphens = WhitespacesToHyphens,
                WhitespacesToUnderscores = WhitespacesToUnderscores
            };

            return gameName.FormatString(formatParameters);
        }

        public List<Screenshot> LoadScreenshots(Game game)
        {
            var gameName = FormatGameName(game.Name);
            var folder = StringExpander.ReplaceAllPlaceholders(Path, game, gameName);
            var fileMask = StringExpander.ReplaceAllPlaceholders(FileMask, game, gameName);
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

        public void ResolveConfig()
        {
            ResolvedPath = StringExpander?.ReplaceAllPlaceholders(Path, TestGame?.Game, ExampleResult);
            ResolvedFileMask = StringExpander?.ReplaceAllPlaceholders(FileMask, TestGame?.Game, ExampleResult); ;
        }

        public void ResolveFormat() => ExampleResult = FormatGameName(ExampleName) ?? string.Empty;
    }
}
