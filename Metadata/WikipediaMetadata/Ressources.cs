using System;
using System.Collections.Generic;

namespace WikipediaMetadata
{
    /// <summary>
    /// contains several ressources needed to parse the wikipedia data
    /// </summary>
    public static class Ressources
    {
        /// <summary>
        /// names of all list templates used to split values.
        /// </summary>
        public static readonly string[] ListTemplateNames = { "unbulleted list", "ubl", "collapsible list", "flatlist", "plainlist", "vgrelease", "video game release" };

        /// <summary>
        /// Possible names for video game release templates. Is needed to remove the country value from the list of platforms.
        /// </summary>
        public static readonly string[] VgReleaseTemplateNames = { "vgrelease", "video game release" };

        /// <summary>
        /// Templates that will be removed from the results.
        /// </summary>
        public static readonly string[] UnwantedTemplateNames = { "efn", "cite web" };

        /// <summary>
        /// Templates that will treated like their own list of values.
        /// </summary>
        public static readonly string[] AllowedSubListTemplates = { "nobold", "nowrap", "start date" };

        /// <summary>
        /// Array of strings to separate the values by.
        /// </summary>
        public static readonly string[] StringSeparators = { "<br />", "<br/>", "<br>", "\n" };

        /// <summary>
        /// Typical date formats from wikipedia pages.
        /// </summary>
        public static readonly string[] DateFormatStringsFull = { "MM/dd/yyyy", "MMMM d, yyyy", "d MMMM yyyy", "yyyy-MM-dd" };

        /// <summary>
        /// Typical date formats from wikipedia pages.
        /// </summary>
        public static readonly string[] DateFormatStringsYearMonth = { "MM/yyyy", "MMMM, yyyy", "MMMM yyyy", "yyyy-MM" };

        /// <summary>
        /// List of all possible platform codes for metacritic ratings.
        /// </summary>
        public static readonly List<string> PlatformCodes = new List<string> {
            "3DO", "3DS", "AMI", "ARC", "A2600", "JAG", "LYNX", "AST", "C64", "CD32", "CV", "DOS", "SDC", "DS", "GB",
            "GBA", "GBC", "NGC", "GEN", "iOS", "MAC", "SMS", "MOB", "N-G", "N64", "NES", "NS", "PC", "VITA", "PS", "PS2",
            "PS3", "PS4", "PS5", "PSP", "SSAT", "SMD", "SGG", "NSHI", "SNES", "TG16", "WII", "WIIU", "XBOX", "X360",
            "XONE", "XSXS", "ZX", };

        /// <summary>
        /// Url to fetch the HTML of a page.
        /// </summary>
        public static readonly string PageHtmlUrl = "https://en.wikipedia.org/api/rest_v1/page/html/{0}";

        /// <summary>
        /// We only fetch headings, paragraphs, lists and su sections from a section, because stuff like blockquotes or tables
        /// don't work well in Playnite and usually aren't essential to the description of a video game.
        /// </summary>
        public static readonly string[] AllowedSecondLevelNodes = { "h2", "p", "ul", "ol", "section" };

        /// <summary>
        /// The same for third levels...
        /// </summary>
        public static readonly string[] AllowedThirdLevelNodes = { "h3", "p", "ul", "ol", "section" };

        /// <summary>
        /// The same for fourth levels...
        /// </summary>
        public static readonly string[] AllowedFourthLevelNodes = { "h4", "p", "ul", "ol" };

        /// <summary>
        /// Array of all in a paragraph accepted tags.
        /// </summary>
        public static readonly string[] AllowedParagraphTags = { "u", "b", "strong", "i", "em", "sub", "sup", "mark", "small", "del", "ins" };

        /// <summary>
        /// List of rename patterns for the links
        /// </summary>
        public static readonly List<LinkPair> LinkPairs = new List<LinkPair>()
        {
            new LinkPair { Contains = "gamefaqs.gamespot.com",
            Name = "GameFAQs"},
            new LinkPair { Contains = "giantbomb.com",
            Name = "Giant Bomb"},
            new LinkPair { Contains = "imdb.com",
            Name = "IMDb"},
            new LinkPair { Contains = "arcade-museum.com",
            Name = "Killer List of Videogames"},
            new LinkPair { Contains = "metacritic.com",
            Name = "Metacritic"},
            new LinkPair { Contains = "mobygames.com",
            Name = "MobyGames"},
            new LinkPair { Contains = "steampowered.com",
            Name = "Steam"},
        };
    }

    /// <summary>
    /// Simple class for a pair of string values to rename links.
    /// </summary>
    public class LinkPair
    {
        /// <summary>
        /// Value the link must contain
        /// </summary>
        public string Contains { get; set; }
        /// <summary>
        /// Desired name of the link.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Simple class for a pair of string values to rename links.
    /// </summary>
    public class PartialDate
    {
        /// <summary>
        /// DateTime representation of the value
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Specifies, if the day was present in the date
        /// </summary>
        public bool HasDay { get; set; }
        /// <summary>
        /// Specifies, if the month was present in the date
        /// </summary>
        public bool HasMonth { get; set; }

        public PartialDate(DateTime date, bool hasDay = true, bool hasMonth = true)
        {
            Date = date;
            HasMonth = hasMonth;
            HasDay = hasDay;
        }
    }
}
