using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
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

        public Screenshot(string path = "", string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _path = path;
            _name = name == string.Empty ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
        }

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

        public void GenerateThumbnail()
        {
            if (!IsDownloaded || !string.IsNullOrEmpty(DownloadedThumbnailPath) || !System.IO.File.Exists(DownloadedPath))
            {
                return;
            }

            var thumb = FileHelper.CreateThumbnailImage(DownloadedPath);
            DownloadedThumbnailPath = thumb.FullName;
        }

        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        [DontSerialize]
        public string DisplayPath => IsDownloaded ? DownloadedPath : Path;

        [SerializationPropertyName("downloadedPath")]
        public string DownloadedPath
        {
            get => _downloadedPath;
            set => SetValue(ref _downloadedPath, value);
        }

        [SerializationPropertyName("downloadedThumbnailPath")]
        public string DownloadedThumbnailPath
        {
            get => _downloadedThumbnailPath;
            set => SetValue(ref _downloadedThumbnailPath, value);
        }

        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        [DontSerialize]
        public bool IsDownloaded => !string.IsNullOrEmpty(DownloadedPath);

        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [SerializationPropertyName("path")]
        public string Path
        {
            get => _path;
            set => SetValue(ref _path, value);
        }

        [DontSerialize]
        public bool PathIsUrl => !string.IsNullOrEmpty(Path) && (Path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || Path.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }

        [SerializationPropertyName("thumbnailPath")]
        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set => SetValue(ref _thumbnailPath, value);
        }

        [DontSerialize]
        public string DisplayThumbnail => IsDownloaded && !string.IsNullOrEmpty(DownloadedThumbnailPath)
            ? DownloadedThumbnailPath : !string.IsNullOrEmpty(ThumbnailPath)
            ? ThumbnailPath : DisplayPath;
    }
}
