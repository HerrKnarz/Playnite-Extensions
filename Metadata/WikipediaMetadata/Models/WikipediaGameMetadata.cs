using KNARZhelper;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WikipediaMetadata.Models
{
    /// <summary>
    /// Class to parse the metadata from wikitext (mainly the "infobox video game" template).
    /// </summary>
    public class WikipediaGameMetadata
    {
        /// <summary>
        /// names of all list templates used to split values.
        /// </summary>
        private readonly string[] listTemplateNames = { "unbulleted list", "ubl", "collapsible list", "flatlist", "plainlist", "vgrelease", "video game release" };

        /// <summary>
        /// Possible names for video game release temnplates. Is needed to remove the country value from the list of platforms.
        /// </summary>
        private readonly string[] vgReleaseTemplateNames = { "vgrelease", "video game release" };

        /// <summary>
        /// Templates that will be removed from the results.
        /// </summary>
        private readonly string[] unwantedTemplateNames = { "efn", "cite web" };

        /// <summary>
        /// Array of strings to separate the values by.
        /// </summary>
        private readonly string[] stringSeparators = { "<br />", "<br/>", "<br>", "\n" };

        /// <summary>
        /// Typical date formats from wikipedia pages.
        /// </summary>
        private readonly string[] dateFormatStrings = { "MM/dd/yyyy", "MMMM d, yyyy", "d MMMM yyyy" };

        /// <summary>
        /// Name of the game
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Unique key - is the variable part of the page url.
        /// </summary>
        public string Key { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; }
        public ReleaseDate? ReleaseDate { get; set; }
        public List<MetadataProperty> Genres { get; set; }
        public List<MetadataProperty> Developers { get; set; }
        public List<MetadataProperty> Publishers { get; set; }
        public List<MetadataProperty> Features { get; set; }
        public List<MetadataProperty> Tags { get; set; }
        public List<Link> Links { get; set; }
        public List<MetadataProperty> Series { get; set; }
        public List<MetadataProperty> Platforms { get; set; }
        public int CriticScore { get; set; }

        /// <summary>
        /// Creates an instance of the class and fills the parameters by parsing the wikitext.
        /// </summary>
        /// <param name="gameData">Page object from wikipedia containing the wikitext and other data.</param>
        public WikipediaGameMetadata(WikipediaGameData gameData)
        {
            if (gameData.Source != null)
            {
                try
                {
                    WikitextParser parser = new WikitextParser();
                    string text = gameData.Source;
                    Wikitext ast = parser.Parse(text);

                    Key = gameData.Key;

                    // Most of the game relevant data can be found in the "infobox video game" template. Most Wikipedia pages
                    // for games have one of those. Without it, only name, cover image, description and links can be fetched.
                    Template infoBox = ast.EnumDescendants().OfType<Template>()
                        .Where(t => CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)) == "infobox video game").FirstOrDefault();

                    if (infoBox != null)
                    {
                        TemplateArgument gameTitle = infoBox.Arguments["title"];
                        if (gameTitle != null)
                        {
                            Name = gameTitle.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                        }

                        CoverImageUrl = GetImageUrl(Key);
                        ReleaseDate = GetDate(infoBox);
                        Genres = GetValues(infoBox, "genre");
                        Developers = GetValues(infoBox, "developer", true);
                        Publishers = GetValues(infoBox, "publisher", true);
                        Features = GetValues(infoBox, "modes");

                        Tags = new List<MetadataProperty>();

                        Tags.AddRange(GetValues(infoBox, "engine", false, "[Game Engine]"));
                        Tags.AddRange(GetValues(infoBox, "director", false, "[People] director:"));
                        Tags.AddRange(GetValues(infoBox, "producer", false, "[People] producer:"));
                        Tags.AddRange(GetValues(infoBox, "designer", false, "[People] designer:"));
                        Tags.AddRange(GetValues(infoBox, "programmer", false, "[People] programmer:"));
                        Tags.AddRange(GetValues(infoBox, "artist", false, "[People] artist:"));
                        Tags.AddRange(GetValues(infoBox, "writer", false, "[People] writer:"));
                        Tags.AddRange(GetValues(infoBox, "composer", false, "[People] composer:"));

                        Links = GetLinks(gameData);
                        Series = GetValues(infoBox, "series");

                        List<MetadataProperty> platforms = new List<MetadataProperty>();

                        platforms.AddRange(GetValues(infoBox, "platforms"));
                        platforms.AddRange(GetValues(infoBox, "arcade system"));

                        PlatformHelper platformHelper = new PlatformHelper(API.Instance);

                        Platforms = platforms.SelectMany(p => platformHelper.GetPlatforms(p.ToString())).ToList();
                    }

                    if (string.IsNullOrEmpty(Name))
                    {
                        Name = gameData.Title.Replace("(video game)", "").Trim();
                    }

                    CriticScore = GetCriticScore(ast);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error parsing Wikitext");
                }
            }
        }

        /// <summary>
        /// Gets the earliest release date from all dates found in the infobox template.
        /// </summary>
        /// <param name="infoBox">Infobox template</param>
        /// <returns></returns>
        private ReleaseDate? GetDate(Template infoBox)
        {
            try
            {
                // We use the GetValues function to fetch all values from the "released" section.
                List<MetadataProperty> list = GetValues(infoBox, "released");

                List<DateTime> dates = new List<DateTime>();

                // We check each value for a valid date and at those to a datetime list.
                foreach (MetadataProperty property in list)
                {
                    if (DateTime.TryParseExact(property.ToString(), dateFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        dates.Add(dateTime);
                    }
                }

                // If dates were found, We'll return the earliest one.
                if (dates.Count > 0)
                {
                    return new ReleaseDate(dates.Min());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error parsing date");
            }

            return null;
        }

        /// <summary>
        /// Gets all values from a field in the infobox template.
        /// </summary>
        /// <param name="infoBox">Infobox template</param>
        /// <param name="field">Name of the field</param>
        /// <param name="removeParentheses">If true all values in parentheses will be removed. Those mostly contain the
        /// platforms in the developer or publisher fields.</param>
        /// <param name="prefix">Prefix to be added to the value. Is used to categorize the tags.</param>
        /// <returns>List of all found values</returns>
        private List<MetadataProperty> GetValues(Template infoBox, string field, bool removeParentheses = false, string prefix = "")
        {
            try
            {
                TemplateArgument argument = infoBox.Arguments[field];
                if (argument != null)
                {
                    List<MetadataProperty> values = new List<MetadataProperty>();

                    // We go through all list templates used in the field to fetch the single values in the list.
                    foreach (Template template in argument.EnumDescendants().OfType<Template>()
                            .Where(t => listTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
                    {
                        // In the template vgrelease the first argument is supposed to be the country. We remove that, so it
                        // doesn't appear as a plaform.
                        if (vgReleaseTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(template.Name).ToLower()))
                        {
                            TemplateArgument arg = template.Arguments.FirstNode;

                            if (arg != null)
                            {
                                arg.Remove();
                            }
                        }

                        foreach (TemplateArgument listArgument in template.Arguments)
                        {
                            if (listArgument.Name == null || listArgument.Name.ToPlainText() == "title")
                            {
                                values.AddRange(Split(StripUnwantedElements(listArgument).Value.ToString(), field, removeParentheses));
                            }
                        }
                    }

                    // We now remove all sub templates to get the value itself, that won't be added otherwise.
                    foreach (Template template in argument.EnumDescendants().OfType<Template>())
                    {
                        template.Remove();
                    }

                    // All additional elements like cite etc. will be removed, too.
                    argument = StripUnwantedElements(argument);

                    values.AddMissing(Split(argument.Value.ToString(), field, removeParentheses));

                    // Now we add the prefix to all 
                    if (prefix != string.Empty && prefix != null)
                    {
                        List<MetadataProperty> prefixedValues = new List<MetadataProperty>();

                        prefixedValues.AddMissing(values.Select(v => new MetadataNameProperty($"{prefix} {v}")));

                        return prefixedValues;
                    }

                    return values;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error parsing argument '{field}'");
            }

            return new List<MetadataProperty>();
        }

        /// <summary>
        /// Gets the link to the Wikipedia page
        /// </summary>
        /// <param name="gameData">game data with the key to the page</param>
        /// <returns></returns>
        private List<Link> GetLinks(WikipediaGameData gameData)
        {
            List<Link> links = new List<Link>
            {
                new Link("Wikipedia", "https://en.wikipedia.org/wiki/" + gameData.Key)
            };

            return links;
        }

        /// <summary>
        /// Splits values, that are separated by line breaks or commas instead of list templates.
        /// </summary>
        /// <param name="value">value to be split</param>
        /// <param name="field">name of the field. Is used to recognize fields, that can contain values that have commas
        /// we don't want to split.</param>
        /// <param name="removeParentheses">Removes values in parentheses.</param>
        /// <returns>List of values</returns>
        private List<MetadataNameProperty> Split(string value, string field, bool removeParentheses = false)
        {
            List<MetadataNameProperty> values = new List<MetadataNameProperty>();

            // We remove all parentheses from the values.
            if (removeParentheses)
            {
                while (value.IndexOf("(") > 0)
                {
                    value = value.Remove(value.IndexOf("("), value.IndexOf(")") - value.IndexOf("(") + 1).Trim();
                }
            }

            // Now the build the list of separators to split the values by.
            List<string> separators = new List<string>();

            separators.AddRange(stringSeparators);

            // Fields for release dates and metacritic contain commas we don't want to split, so we leave commas out of the list.
            if (field != "released" && field != "MC")
            {
                separators.AddMissing(",");
            }

            // Now we split the values by the list of separators and parse the result to get the plain text values.
            foreach (string segment in value.Split(separators.ToArray(), 100, StringSplitOptions.RemoveEmptyEntries))
            {
                WikitextParser parser = new WikitextParser();

                string segmentEditable = parser.Parse(segment).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim();

                if (segmentEditable.Length > 0)
                {
                    values.Add(new MetadataNameProperty(segmentEditable));
                }
            }

            return values;
        }

        /// <summary>
        /// Strips an argument of unwanted elements
        /// </summary>
        /// <param name="argument">argument to clean up.</param>
        /// <returns>The cleaned up argument</returns>
        private TemplateArgument StripUnwantedElements(TemplateArgument argument)
        {
            IEnumerable<Template> templates = argument.EnumDescendants().OfType<Template>();

            // First we remove every template we don't want.
            foreach (Template x in argument.EnumDescendants().OfType<Template>()
                            .Where(t => unwantedTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
            {
                x.Remove();
            }

            // Now we also remove <ref> tags, because those contain footnotes etc, we don't need.
            foreach (LineNode line in argument.Value.Lines)
            {
                foreach (Node inline in line.EnumDescendants())
                {
                    if (inline.ToString().StartsWith("<ref"))
                    {
                        inline.Remove();
                    }
                }
            }
            return argument;
        }

        /// <summary>
        /// Fetches the url of the main image from the page.
        /// </summary>
        /// <param name="key">Key of the page</param>
        /// <returns>Url of the image</returns>
        private string GetImageUrl(string key)
        {
            WikipediaImage imageData = WikipediaApiCaller.GetImage(key);

            if (imageData != null && imageData.Query != null)
            {
                ImagePage page = imageData.Query.Pages.FirstOrDefault();

                if (page != null && page.Original != null)
                {
                    return page.Original.Source;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the average metacritic score from all platforms mentioned in the video game reviews template.
        /// </summary>
        /// <param name="ast">Wikitext object that contains the review box.</param>
        /// <returns></returns>
        private int GetCriticScore(Wikitext ast)
        {
            // We search for the first occurrence of a review template in the page.
            Template infoBox = ast.EnumDescendants().OfType<Template>()
                .Where(t => CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)) == "video game reviews").FirstOrDefault();

            if (infoBox != null)
            {
                // Now we use the GetValues function to get all review ratings from the metacritic section in the template.
                List<MetadataProperty> list = GetValues(infoBox, "MC", true);

                List<int> ratings = new List<int>();

                // We go through each value, remove everything before a colon (those are platform names) and then get the
                // integer value before the slash.
                foreach (MetadataProperty property in list)
                {
                    string value = property.ToString();

                    if (value.IndexOf(":") > 0)
                    {
                        value = value.Substring(value.IndexOf(":") + 1).Trim();
                    }

                    if (value.IndexOf("/") > 0)
                    {
                        if (int.TryParse(value.Substring(0, value.IndexOf("/")), out int rating))
                        {
                            ratings.Add(rating);
                        };
                    }
                }

                // If we found ratings, we return the average rating.
                if (ratings.Count > 0)
                {
                    return (int)Math.Ceiling(ratings.Average());
                }
            }

            return -1;
        }

        /// <summary>
        /// Cleans up a template name, because sometimes those containt html comment blocks, and converts it to lower case.
        /// </summary>
        /// <param name="name">name of the template</param>
        /// <returns>The cleaned up name</returns>
        public string CleanTemplateName(string name)
        {
            if (name.IndexOf("\n") > 0)
            {
                name = name.Substring(0, name.IndexOf("\n")).Trim();
            }

            if (name.Contains("<!--"))
            {
                int start = name.IndexOf("<!--");
                int end = name.IndexOf("-->", start) + 3;
                name = name.Remove(start, end - start);
            }
            return name.ToLower();
        }
    }
}
