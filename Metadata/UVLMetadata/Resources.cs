using KNARZhelper;
using System;
using System.Collections.Generic;
using UVLMetadata.Models;

namespace UVLMetadata;

public enum DescriptionToUse
{
    Description,
    OfficialDescription,
    Both,
}

public enum RatingToUse
{
    Median,
    Average,
}

/// <summary>
/// contains several resources needed to parse the UVL data
/// </summary>
public static class Resources
{
    /// <summary>
    /// Typical date formats from UVL pages. They usually use yyyy-MM-dd, but it's safer to have a
    /// few more formats, just in case.
    /// </summary>
    public static readonly string[] DateFormatStringsFull = ["MM/dd/yyyy", "MMMM d, yyyy", "d MMMM yyyy", "yyyy-MM-dd"];

    /// <summary>
    /// Typical date formats from UVL pages.
    /// </summary>
    public static readonly string[] DateFormatStringsYearMonth = ["MM/yyyy", "MMMM, yyyy", "MMMM yyyy", "yyyy-MM"];

    public static readonly string HorizontalLineHtml = "\n\n<hr><br>\n\n";

    /// <summary>
    /// List of rename patterns for the links
    /// </summary>
    public static readonly StringPairs LinkPairs =
    [
        new StringPair
        {
            Contains = "gog.com",
            Name = "GOG"
        },

        new StringPair
        {
            Contains = "Steam Powered",
            Name = "Steam"
        },

        new StringPair
        {
            Contains = "steampowered.com",
            Name = "Steam"
        }
    ];

    public static readonly Dictionary<string, TagType> TagTypes = new()
    {
        { "series", TagType.Series },
        { "themes", TagType.Theme },
        { "concepts", TagType.Concept },
        { "entities", TagType.Entity }
    };

    public static readonly string WebsiteUrl = "https://www.uvlist.net";
}

public class PartialDate(DateTime date, bool hasDay = true, bool hasMonth = true)
{
    /// <summary>
    /// DateTime representation of the value
    /// </summary>
    public DateTime Date { get; set; } = date;

    /// <summary>
    /// Specifies, if the day was present in the date
    /// </summary>
    public bool HasDay { get; set; } = hasDay;

    /// <summary>
    /// Specifies, if the month was present in the date
    /// </summary>
    public bool HasMonth { get; set; } = hasMonth;
}
