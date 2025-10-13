using KNARZhelper;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ScreenshotUtilities
{
    public class FileHelper
    {
        private const string longPathPrefix = @"\\?\";
        private const string longPathUncPrefix = @"\\?\UNC\";

        public static FileInfo CreateThumbnailImage(string imageFileName, string thumbnailFileName = "")
        {
            const int thumbnailSize = 120;

            if (string.IsNullOrEmpty(thumbnailFileName))
            {
                var fi = new FileInfo(imageFileName);
                thumbnailFileName = Path.Combine(fi.DirectoryName, $"{Path.GetFileNameWithoutExtension(fi.Name)}_thumb.png");
            }

            using (var image = Image.FromFile(imageFileName))
            {
                var imageHeight = image.Height;
                var imageWidth = image.Width;

                imageWidth = (int)(imageWidth / (float)imageHeight * thumbnailSize);
                imageHeight = thumbnailSize;

                using (var thumb = image.GetThumbnailImage(imageWidth, imageHeight, () => false, IntPtr.Zero))
                {
                    thumb.Save(thumbnailFileName);
                }
            }

            return new FileInfo(thumbnailFileName);
        }

        public static void DeleteFile(string path, bool includeReadonly = false)
        {
            path = FixPathLength(path);

            var fi = new FileInfo(path);

            if (!fi.Exists)
            {
                return;
            }

            if (includeReadonly)
            {

                if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fi.Attributes ^= FileAttributes.ReadOnly;
                }
            }

            fi.Delete();
        }

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

                var di = new DirectoryInfo(path);

                return di;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create download path.");

                return null;
            }
        }

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

        public static bool IsFullPath(string path) => !string.IsNullOrWhiteSpace(path) && Regex.IsMatch(path, @"^([a-zA-Z]:\\|\\\\)");

        private static void PrepareSaveFile(string path, bool deleteFile = true)
        {
            new DirectoryInfo(Path.GetDirectoryName(path)).Create();

            if (deleteFile)
            {
                DeleteFile(path, true);
            }
        }

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
