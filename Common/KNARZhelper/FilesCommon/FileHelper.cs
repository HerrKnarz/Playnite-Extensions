using ImageMagick;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KNARZhelper.FilesCommon
{
    /// <summary>
    /// Helper class for file operations.
    /// </summary>
    public class FileHelper
    {
        private const string longPathPrefix = @"\\?\";
        private const string longPathUncPrefix = @"\\?\UNC\";

        /// <summary>
        /// Creates a thumbnail image with a height of 120 pixels, maintaining the aspect ratio.
        /// </summary>
        /// <param name="imageFileName">The path to the original image file.</param>
        /// <param name="thumbnailFileName">The path to the thumbnail image file.</param>
        /// <returns>The FileInfo of the created thumbnail image.</returns>
        public static FileInfo CreateThumbnailImage(string imageFileName, string thumbnailFileName = "")
        {
            if (string.IsNullOrEmpty(thumbnailFileName))
            {
                var fileInfo = new FileInfo(imageFileName);
                thumbnailFileName = Path.Combine(fileInfo.DirectoryName, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}_thumb.jpg");
            }

            using (var image = new MagickImage(imageFileName))
            {
                image.Scale(0, 120);

                image.Format = MagickFormat.Jpg;

                image.Write(thumbnailFileName);
            }

            return new FileInfo(thumbnailFileName);
        }

        /// <summary>
        /// Deletes the specified file. If the file is read-only and includeReadonly is true, the read-only attribute will be removed before deletion.
        /// </summary>
        /// <param name="path">The path to the file to delete.</param>
        /// <param name="includeReadonly">Whether to remove the read-only attribute before deletion.</param>
        public static void DeleteFile(string path, bool includeReadonly = false)
        {
            path = FixPathLength(path);

            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                return;
            }

            if (includeReadonly)
            {

                if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fileInfo.Attributes ^= FileAttributes.ReadOnly;
                }
            }

            fileInfo.Delete();
        }

        /// <summary>
        /// Fixes the path length by adding the appropriate prefix for long paths if necessary.
        /// </summary>
        /// <param name="path">The file path to fix.</param>
        /// <returns>The fixed file path.</returns>
        public static string FixPathLength(string path)
        {
            // Relative paths don't support long paths
            // https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd
            return !IsFullPath(path)
                ? path
                : path.Length >= 258 && !path.StartsWith(longPathPrefix)
                ? path.StartsWith(@"\\") ? longPathUncPrefix + path.Substring(2) : longPathPrefix + path
                : path;
        }

        /// <summary>
        /// Gets the download path based on the base path, game ID, and provider ID.
        /// </summary>
        /// <param name="basePath">The base path for the download location.</param>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="providerId">The ID of the provider.</param>
        /// <returns>The directory info for the download path.</returns>
        public static DirectoryInfo GetDownloadPath(string basePath, Guid gameId = default, Guid providerId = default)
        {
            try
            {
                if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                {
                    return null;
                }

                var path = Path.Combine(basePath,
                    gameId == default ? string.Empty : gameId.ToString(),
                    providerId == default ? string.Empty : providerId.ToString());

                var directoryInfo = new DirectoryInfo(path);

                return directoryInfo;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create download path.");

                return null;
            }
        }

        /// <summary>
        /// Gets the file extension from a URL.
        /// </summary>
        /// <param name="url">The URL to get the file extension from.</param>
        /// <returns>The file extension, or an empty string if the URL is invalid.</returns>
        public static string GetFileExtensionFromUrl(string url)
        {
            try
            {
                return string.IsNullOrEmpty(url) ? string.Empty : Path.GetExtension(new Uri(url).GetLeftPart(UriPartial.Path));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get file extension from URL.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if the given path is a full path (absolute path).
        /// </summary>
        /// <param name="path">The file path to check.</param>
        /// <returns>True if the path is a full path; otherwise, false.</returns>
        public static bool IsFullPath(string path) => !string.IsNullOrWhiteSpace(path) && Regex.IsMatch(path, @"^([a-zA-Z]:\\|\\\\)");

        private static void PrepareSaveFile(string path, bool deleteFile = true)
        {
            new DirectoryInfo(Path.GetDirectoryName(path)).Create();

            if (deleteFile)
            {
                DeleteFile(path, true);
            }
        }

        /// <summary>
        /// Writes the specified content to a file at the given path. If the file already exists, it will be deleted before writing.
        /// </summary>
        /// <param name="path">The file path to write to.</param>
        /// <param name="content">The content to write to the file.</param>
        /// <param name="useUtf8">Whether to use UTF-8 encoding.</param>
        internal static void WriteStringToFile(string path, string content, bool useUtf8 = false)
        {
            path = FixPathLength(path);

            PrepareSaveFile(path);

            if (useUtf8)
            {
                File.WriteAllText(path, content, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }
    }
}
