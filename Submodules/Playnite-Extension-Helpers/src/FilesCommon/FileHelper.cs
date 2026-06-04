using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PlayniteExtensionHelpers.FilesCommon;

/// <summary>
/// Helper class for file operations.
/// </summary>
public partial class FileHelper
{
    private const string longPathPrefix = @"\\?\";
    private const string longPathUncPrefix = @"\\?\UNC\";

    /// <summary>
    /// Deletes the specified file. If the file is read-only and includeReadonly is true, the
    /// read-only attribute will be removed before deletion.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    /// <param name="includeReadonly">Whether to remove the read-only attribute before deletion.</param>
    public static void DeleteFile(string? path, bool includeReadonly = false)
    {
        if (path.IsNullOrEmpty())
        {
            return;
        }

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
        // Relative paths don't support long paths https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd
        return !IsFullPath(path)
            ? path
            : path.Length >= 258 && !path.StartsWith(longPathPrefix)
            ? path.StartsWith(@"\\") ? string.Concat(longPathUncPrefix, path.AsSpan(2)) : longPathPrefix + path
            : path;
    }

    /// <summary>
    /// Gets the file extension from a URL.
    /// </summary>
    /// <param name="url">The URL to get the file extension from.</param>
    /// <returns>The file extension, or an empty string if the URL is invalid.</returns>
    public static string? GetFileExtensionFromUrl(string? url)
    {
        try
        {
            return url.IsNullOrEmpty() ? null : Path.GetExtension(url.StripUriParams());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get file extension from URL.");
            return null;
        }
    }

    /// <summary>
    /// Checks if the given path is a full path (absolute path).
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if the path is a full path; otherwise, false.</returns>
    public static bool IsFullPath(string path) => !path.IsNullOrWhiteSpace() && PathRegEx().IsMatch(path);

    /// <summary>
    /// Writes the specified content to a file at the given path. If the file already exists, it
    /// will be deleted before writing.
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

    [GeneratedRegex(@"^([a-zA-Z]:\\|\\\\)")]
    private static partial Regex PathRegEx();

    private static void PrepareSaveFile(string? path, bool deleteFile = true)
    {
        var dir = Path.GetDirectoryName(path);

        if (dir.IsNullOrEmpty())
        {
            return;
        }

        new DirectoryInfo(dir).Create();

        if (deleteFile)
        {
            DeleteFile(path, true);
        }
    }
}