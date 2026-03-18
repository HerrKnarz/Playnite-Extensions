using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class FolderConfig : ObservableObject
    {
        [DontSerialize]
        public StringExpander StringExpander;

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
    }
}
