using Playnite;
using PlayniteExtensionHelpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace LinkUtilities.Models;

public enum PatternMatchModes
{
    MatchByUrl,
    MatchByName,
    MatchByUrlAndName,
    MatchByType,
    MatchByUrlAndType,
}

/// <summary>
/// Types of patterns that can be matched.
/// </summary>
public enum PatternTypes
{
    LinkNamePattern,
    RemovePattern
}

/// <summary>
/// Handles the Patterns to find link names for URL/link title combinations
/// </summary>
public class LinkNamePatterns : ObservableCollection<LinkNamePattern>
{
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

    public bool LinkMatch(ref string linkName, string linkUrl, ref string? linkTypeIdentifier, PatternMatchModes matchMode = PatternMatchModes.MatchByUrlAndName)
    {
        var tempLinkName = linkName;
        var tempLinkTypeIdentifier = linkTypeIdentifier;

        var pattern = this.FirstOrDefault(x => x.LinkMatch(tempLinkName, linkUrl, tempLinkTypeIdentifier, matchMode));

        if (pattern == null)
        {
            return false;
        }

        linkName = pattern?.LinkName ?? string.Empty;
        linkTypeIdentifier = pattern?.LinkTypeIdentifier;

        return true;
    }

    public void RemoveEmpty(bool removePattern = false)
    {
        if (removePattern)
        {
            this.RemoveAll(x => x.UrlPattern.IsNullOrEmpty() && x.LinkTypeIdentifier.IsNullOrEmpty());
        }
        else
        {
            this.RemoveAll(x => x.NamePattern.IsNullOrEmpty() && x.UrlPattern.IsNullOrEmpty());
        }
    }

    public void SortPatterns()
    {
        var patterns = this.ToList();
        Clear();

        this.AddMissing([.. patterns.Distinct()
            .OrderBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)]);
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