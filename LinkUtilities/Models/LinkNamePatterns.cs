using Playnite;
using PlayniteExtensionHelpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace LinkUtilities.Models;

/// <summary>
/// Types of patterns that can be matched.
/// </summary>
public enum PatternTypes
{
    LinkNamePattern,
    RemovePattern,
    RenamePattern
}

/// <summary>
/// Handles the Patterns to find link names for URL/link title combinations
/// </summary>
public class LinkNamePatterns : ObservableCollection<LinkNamePattern>
{
    /// <summary>
    /// Creates an empty instance
    /// </summary>
    public LinkNamePatterns()
    {
        SortByPosition = false;
    }

    /// <summary>
    /// Creates an empty instance, but sets the SortByPosition property to the desired value.
    /// </summary>
    /// <param name="sortByPosition">If true the list is sorted by the position property.</param>
    public LinkNamePatterns(bool sortByPosition = false)
    {
        SortByPosition = sortByPosition;
    }

    public bool SortByPosition { get; set; }

    /// <summary>
    /// Adds the default patterns to the list and sorts it afterward.
    /// </summary>
    /// <param name="type">Type of the pattern to be added</param>
    public void AddDefaultPatterns(PatternTypes type)
    {
        foreach (var item in GetDefaultLinkNamePatterns(type).Where(item => this.All(x => x.LinkName != item.LinkName)))
        {
            Add(item);
        }

        SortPatterns();
    }

    public bool LinkMatch(ref string linkName, string linkUrl, bool overridePartialMatch = false)
    {
        var tempLinkName = linkName;

        var pattern = this.FirstOrDefault(x => x.LinkMatch(tempLinkName, linkUrl, overridePartialMatch));

        if (pattern == null)
        {
            return false;
        }

        linkName = pattern?.LinkName ?? string.Empty;
        return true;
    }

    public void RemoveEmpty() => this.RemoveAll(x => !(x.NamePattern?.Length == 0) && !(x.UrlPattern?.Length == 0));

    public void SortPatterns()
    {
        var patterns = this.ToList();
        Clear();

        if (SortByPosition)
        {
            this.AddMissing([.. patterns.Distinct()
                .OrderBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)]);
        }
        else
        {
            this.AddMissing([.. patterns.Distinct()
                .OrderBy(x => x.Position)
                .ThenBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)]);
        }
    }

    /// <summary>
    /// Gets a list of default patterns.
    /// </summary>
    /// <param name="type">Type of the pattern to be added</param>
    private static List<LinkNamePattern> GetDefaultLinkNamePatterns(PatternTypes type)
    {
        var pluginDir = LinkUtilitiesPlugin.InstallDir;

        if (pluginDir.IsNullOrEmpty())
        {
            return [];
        }

        var fileName = type switch
        {
            PatternTypes.LinkNamePattern => "DefaultLinkNamePatterns.json",
            PatternTypes.RemovePattern => "DefaultRemovePatterns.json",
            PatternTypes.RenamePattern => "DefaultRenamePatterns.json",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var setFile = Path.Combine(pluginDir, "Resources", fileName);

        if (File.Exists(setFile))
        {
            using var json = File.OpenRead(setFile);
            return JsonSerializer.Deserialize<List<LinkNamePattern>>(json) ?? [];
        }

        return [];
    }
}