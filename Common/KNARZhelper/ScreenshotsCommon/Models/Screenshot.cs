using KNARZhelper.FilesCommon;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KNARZhelper.ScreenshotsCommon.Models
{
    /// <summary>
    /// Class representing a screenshot with properties and methods for managing it.
    /// </summary>
    public class Screenshot : ObservableObject
    {
        private string _description;
        private string _downloadedPath;
        private string _downloadedThumbnailPath;
        private Guid _id = Guid.NewGuid();
        private string _name;
        private string _path;
        private int _sortOrder = 0;
        private string _thumbnailPath;

        /// <summary>
        /// Creates a new instance of the Screenshot class.
        /// </summary>
        /// <param name="path">Path to the screenshot file.</param>
        /// <param name="name">Name of the screenshot.</param>
        /// <param name="id">Unique identifier for the screenshot.</param>
        public Screenshot(string path = "", string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _path = path;
            _name = name == string.Empty ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
        }

        /// <summary>
        /// Downloads the screenshot to the specified path.
        /// </summary>
        /// <param name="path">Path to the folder where the screenshot will be downloaded.</param>
        public void Download(string path)
        {
            if (!PathIsUrl || IsDownloaded)
            {
                return;
            }

            path = System.IO.Path.Combine(path, $"{Id}{FileHelper.GetFileExtensionFromUrl(Path)}");
            var image = FileDownloader.Instance().DownloadFileAsync(path, new Uri(Path)).Result;
            DownloadedPath = image.FullName;
        }

        /// <summary>
        /// Generates a thumbnail for the downloaded screenshot.
        /// </summary>
        public void GenerateThumbnail()
        {
            if (!IsDownloaded
                || (!string.IsNullOrEmpty(DownloadedThumbnailPath) && File.Exists(DownloadedThumbnailPath))
                || !File.Exists(DownloadedPath))
            {
                return;
            }

            var thumb = FileHelper.CreateThumbnailImage(DownloadedPath);
            DownloadedThumbnailPath = thumb.FullName;
        }

        /// <summary>
        /// Opens the folder containing the screenshot in File Explorer.
        /// </summary>
        public void OpenContainingFolder()
        {
            var directoryInfo = new FileInfo(DisplayPath).Directory;

            if (directoryInfo.Exists)
            {
                Process.Start("explorer.exe", $"/select, \"{DisplayPath}\"");
            }
        }

        /// <summary>
        /// Opens the screenshot in its associated application.
        /// </summary>
        public void OpenInAssociatedApplication()
        {
            var fileInfo = new FileInfo(DisplayPath);

            if (fileInfo.Exists)
            {
                Process.Start(new ProcessStartInfo(fileInfo.FullName) { UseShellExecute = true });
            }
        }

        /// <summary>
        /// Opens the screenshot URL in the default web browser.
        /// </summary>
        public void OpenInBrowser()
        {
            if (PathIsUrl)
            {
                Process.Start(new ProcessStartInfo(Path));
            }
        }

        /// <summary>
        /// Copies the screenshot image to the clipboard.
        /// </summary>
        public void CopyToClipboard()
        {
            var fileInfo = new FileInfo(DisplayPath);

            if (fileInfo.Exists)
            {
                try
                {
                    Clipboard.SetImage(BitmapFrame.Create(new Uri(DisplayPath, UriKind.Absolute)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to copy screenshot to clipboard: {ex}");
                }
            }
        }

        /// <summary>
        /// Description of the screenshot.
        /// </summary>
        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        /// <summary>
        /// Gets the display path for the screenshot. If the screenshot is downloaded, it returns the downloaded path; otherwise, it
        /// returns the original path.
        /// </summary>
        [DontSerialize]
        public string DisplayPath => IsDownloaded ? DownloadedPath : Path;

        /// <summary>
        /// Gets or sets the file path where the downloaded screenshot is stored.
        /// </summary>
        [SerializationPropertyName("downloadedPath")]
        public string DownloadedPath
        {
            get => _downloadedPath;
            set => SetValue(ref _downloadedPath, value);
        }

        /// <summary>
        /// Gets or sets the file path of the thumbnail to the downloaded screenshot. It will be generated automatically after downloading
        /// the screenshot from the original file instead of downloading the thumbnail separately.
        /// </summary>
        [SerializationPropertyName("downloadedThumbnailPath")]
        public string DownloadedThumbnailPath
        {
            get => _downloadedThumbnailPath;
            set => SetValue(ref _downloadedThumbnailPath, value);
        }

        /// <summary>
        /// Gets or sets the unique identifier for the screenshot. It is used as the filename for the downloaded image, so it's not advisable
        /// to change this value after the screenshot was downloaded already to avoid confusion.
        /// </summary>
        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        /// <summary>
        /// Specifies whether the screenshot has been downloaded.
        /// </summary>
        [DontSerialize]
        public bool IsDownloaded => !string.IsNullOrEmpty(DownloadedPath);

        /// <summary>
        /// Name of the screenshot.
        /// </summary>
        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        /// <summary>
        /// Initial path or URL of the screenshot.
        /// </summary>
        [SerializationPropertyName("path")]
        public string Path
        {
            get => _path;
            set => SetValue(ref _path, value);
        }

        /// <summary>
        /// Specifies whether the Path property is a valid URL.
        /// </summary>
        [DontSerialize]
        public bool PathIsUrl => !string.IsNullOrEmpty(Path) && Path.IsValidHttpUrl();

        /// <summary>
        /// Gets or sets the sort order for the item.
        /// </summary>
        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }

        /// <summary>
        /// Gets or sets the initial path or URL to the thumbnail image.
        /// </summary>
        [SerializationPropertyName("thumbnailPath")]
        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set => SetValue(ref _thumbnailPath, value);
        }

        /// <summary>
        /// Gets the path to the thumbnail image to display, based on the download and availability status.
        /// </summary>
        [DontSerialize]
        public string DisplayThumbnail => IsDownloaded && !string.IsNullOrEmpty(DownloadedThumbnailPath)
            ? DownloadedThumbnailPath : !string.IsNullOrEmpty(ThumbnailPath)
            ? ThumbnailPath : DisplayPath;
    }
}
