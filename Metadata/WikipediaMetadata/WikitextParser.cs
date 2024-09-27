// Ignore Spelling: Wikitext

using KNARZhelper;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    /// Parses the given wikitext to get the relevant metadata infos.
    /// </summary>
    internal class WikitextParser
    {
        private readonly PluginSettings _settings;

        /// <summary>
        /// Creates an instance of the class and fills the parameters by parsing the wikitext.
        /// </summary>
        /// <param name="settings">Settings of the plugin</param>
        public WikitextParser(PluginSettings settings) => _settings = settings;

        public WikipediaGameMetadata GameMetadata { get; set; }

        /// <summary>
        /// Parses the Wikitext
        /// </summary>
        /// <param name="gameData">Page object from wikipedia containing the wikitext and other data.</param>
        /// <param name="platformList">List of all platforms in the database</param>
        public void Parse(WikipediaPage gameData, IItemCollection<Platform> platformList)
        {
            GameMetadata = new WikipediaGameMetadata
            {
                Key = gameData.Key,
                Links = GetLinks(gameData)
            };

            if (gameData.Source == null)
            {
                return;
            }

            try
            {
                var ast = new MwParserFromScratch.WikitextParser().Parse(gameData.Source);

                GameMetadata.CriticScore = GetCriticScore(ast);

                // Most of the game relevant data can be found in the "infobox video game" template. Most Wikipedia pages
                // for games have one of those. Without it, only name, cover image, description and links can be fetched.
                var infoBox = ast.EnumDescendants().OfType<Template>()
                    .FirstOrDefault(t => Resources.InfoBoxVideoGameTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name))));

                if (infoBox != null)
                {
                    GameMetadata.Name = infoBox.Arguments["title"]?.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                    GameMetadata.CoverImageUrl = WikipediaHelper.GetImageUrl(gameData.Key);
                    GameMetadata.ReleaseDate = GetDate(infoBox);
                    GameMetadata.Genres = GetValues(infoBox, "genre");
                    GameMetadata.Developers = GetValues(infoBox, "developer", true);
                    GameMetadata.Publishers = GetValues(infoBox, "publisher", true);
                    GameMetadata.Features = GetValues(infoBox, "modes");
                    GameMetadata.Tags = new List<MetadataProperty>();

                    foreach (var tagSetting in _settings.TagSettings.Where(s => s.IsChecked))
                    {
                        GameMetadata.Tags.AddRange(GetValues(infoBox, tagSetting.Name.ToLower(), false, tagSetting.Prefix));
                    }

                    GameMetadata.Series = GetValues(infoBox, "series");

                    var platforms = new List<MetadataProperty>();

                    platforms.AddRange(GetValues(infoBox, "platforms"));

                    if (_settings.ArcadeSystemAsPlatform)
                    {
                        platforms.AddRange(GetValues(infoBox, "arcade system"));
                    }

                    var platformHelper = new PlatformHelper(platformList.ToList());

                    GameMetadata.Platforms = platforms.SelectMany(p => platformHelper.GetPlatforms(p.ToString())).ToList();
                }

                if (string.IsNullOrEmpty(GameMetadata.Name))
                {
                    GameMetadata.Name = gameData.Title.Replace("(video game)", "").Trim();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error parsing Wikitext");
            }
        }

        /// <summary>
        /// Cleans up a template name, because sometimes those contain html comment blocks, and converts it to lower case.
        /// </summary>
        /// <param name="name">name of the template</param>
        /// <returns>The cleaned up name</returns>
        internal string CleanTemplateName(string name)
        {
            if (name.IndexOf("\n", StringComparison.Ordinal) > 0)
            {
                name = name.Substring(0, name.IndexOf("\n", StringComparison.Ordinal)).Trim();
            }

            return name.RemoveTextBetween("<!--", "-->").ToLower();
        }

        /// <summary>
        /// Cleans up a value and splits it by line breaks or commas instead of list templates.
        /// </summary>
        /// <param name="argument">value to be split</param>
        /// <param name="field">name of the field. Is used to recognize fields, that can contain values that have commas
        /// we don't want to split.</param>
        /// <param name="removeParentheses">Removes values in parentheses.</param>
        /// <param name="removeSup">Removes values in superscripts.</param>
        /// <returns>List of values</returns>
        internal List<MetadataNameProperty> CleanUpAndSplit(TemplateArgument argument, string field, bool removeParentheses = false, bool removeSup = false)
        {
            // First we remove unwanted elements on template level and get the text value of the argument.
            var value = StripUnwantedElements(argument).Value.ToString();

            // Now we remove all parentheses from the values.
            if (removeParentheses)
            {
                value = value.RemoveTextBetween("(", ")").Trim();
            }

            // We remove all superscripts from the values.
            if (removeSup)
            {
                value = value.RemoveTextBetween("<sup", "</sup>").Trim();
            }

            value = value.RemoveTextBetween("<!--", "-->").Trim();

            // Since we also split by new line and don't need to do that, if it's the last character, we remove that one.
            if (value.EndsWith("\n"))
            {
                value = value.Remove(value.Length - "\n".Length).Trim();
            }

            var parser = new MwParserFromScratch.WikitextParser();
            var values = new List<MetadataNameProperty>();

            // If the value is the only one and is a link, we return it without splitting.
            if (value.Count(c => c == '[') == 2 && value.Count(c => c == ']') == 2 && value.StartsWith("[") && value.EndsWith("]"))
            {
                values.Add(new MetadataNameProperty(parser.Parse(value).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim()));
                return values;
            }

            // Now the build the list of separators to split the values by.
            var separators = new List<string>();

            separators.AddRange(Resources.StringSeparators);

            // Fields for release dates and metacritic contain commas we don't want to split, so we leave commas out of the list.
            // We also don't split by comma, if the value is already from a list.
            if (field != "released" && field != "MC" && field != "GR")
            {
                // We only add a comma, if the string isn't already separated by one of the other separators to retain wanted
                // commas in company names etc. This is no perfect solution but better than always splitting by comma.
                if (separators.All(separator => value.IndexOf(separator, StringComparison.Ordinal) <= -1))
                {
                    separators.AddMissing(",");
                }
            }

            // Now we split the values by the list of separators and parse the result to get the plain text values.
            values.AddRange(value.Split(separators.ToArray(), 100, StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => parser.Parse(segment).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim())
                .Where(segmentEditable => segmentEditable.Length > 0)
                .Select(segmentEditable => new MetadataNameProperty(segmentEditable)));

            return values;
        }

        /// <summary>
        /// Gets the average metacritic score from all platforms mentioned in the video game reviews template.
        /// </summary>
        /// <param name="ast">Wikitext object that contains the review box.</param>
        /// <returns>The found critic score</returns>
        internal int GetCriticScore(Wikitext ast)
        {
            // We search for the first occurrence of a review template in the page.
            var infoBox = ast.EnumDescendants().OfType<Template>()
                .FirstOrDefault(t => CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)) == "video game reviews");

            if (infoBox == null)
            {
                return -1;
            }

            // Now we use the GetValues function to get all review ratings from the metacritic section in the template.
            var list = GetValues(infoBox, "MC", true);

            var ratings = new List<int>();

            if (list.Count == 0)
            {
                foreach (var code in Resources.PlatformCodes)
                {
                    list.AddRange(GetValues(infoBox, $"MC_{code}", true));
                }
            }

            // If no metacritc ratings were found, we'll try GameRankings
            if (list.Count == 0)
            {
                list.AddRange(GetValues(infoBox, "GR", true));
            }

            if (list.Count == 0)
            {
                foreach (var code in Resources.PlatformCodes)
                {
                    list.AddRange(GetValues(infoBox, $"GR_{code}", true));
                }
            }

            // We go through each value, remove everything before a colon (those are platform names) and
            // then get the integer value before the slash or percent character.
            foreach (var value in list.Select(property => property.ToString()))
            {
                var editValue = value;

                if (editValue.IndexOf(":", StringComparison.Ordinal) > 0)
                {
                    editValue = editValue.Substring(editValue.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
                }

                if (editValue.IndexOf("/", StringComparison.Ordinal) > 0)
                {
                    if (int.TryParse(editValue.Substring(0, editValue.IndexOf("/", StringComparison.Ordinal)), out var rating))
                    {
                        ratings.Add(rating);
                    }
                }
                else if (editValue.IndexOf("%", StringComparison.Ordinal) > 0)
                {
                    if (int.TryParse(editValue.Substring(0, editValue.IndexOf("%", StringComparison.Ordinal)), out var rating))
                    {
                        ratings.Add(rating);
                    }
                }
            }

            // If we found ratings, we return the average rating.
            if (!ratings.Any())
            {
                return -1;
            }

            switch (_settings.RatingToUse)
            {
                case RatingToUse.Lowest:
                    return ratings.Min();
                case RatingToUse.Highest:
                    return ratings.Max();
                case RatingToUse.Average:
                    return (int)Math.Ceiling(ratings.Average());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the earliest release date from all dates found in the infobox template.
        /// </summary>
        /// <param name="infoBox">Infobox template</param>
        /// <returns>The found release date or null</returns>
        internal ReleaseDate? GetDate(Template infoBox)
        {
            try
            {
                var dates = new List<PartialDate>();

                // We use the GetValues function to fetch all values from the "released" section.
                // We check each value for a valid date and at those to a datetime list.
                foreach (var property in GetValues(infoBox, "released", true, "", true))
                {
                    if (DateTime.TryParseExact(property.ToString(), Resources.DateFormatStringsFull, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
                    {
                        dates.Add(new PartialDate(dateTime));
                    }
                    else if (DateTime.TryParseExact(property.ToString(), Resources.DateFormatStringsYearMonth, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
                    {
                        dates.Add(new PartialDate(dateTime, false));
                    }
                    else if (DateTime.TryParseExact(property.ToString(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
                    {
                        dates.Add(new PartialDate(dateTime, false, false));
                    }
                }

                // If dates were found, We'll return the one depending on the _settings.
                if (dates.Count > 0)
                {
                    foreach (var date in dates)
                    {
                        if (!date.HasMonth)
                        {
                            date.Date = date.Date.AddMonths(11);
                        }

                        if (!date.HasDay)
                        {
                            date.Date = date.Date.EndOfMonth();
                        }
                    }

                    PartialDate dateToUse;

                    switch (_settings.DateToUse)
                    {
                        case DateToUse.Earliest:
                            dateToUse = dates.OrderBy(d => d.Date).First();
                            break;
                        case DateToUse.Latest:
                            dateToUse = dates.OrderByDescending(d => d.Date).First();
                            break;
                        case DateToUse.First:
                            dateToUse = dates.First();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (dateToUse != null)
                    {
                        return dateToUse.HasDay
                            ? new ReleaseDate(dateToUse.Date)
                            : dateToUse.HasMonth
                                ? new ReleaseDate(dateToUse.Date.Year, dateToUse.Date.Month)
                                : new ReleaseDate(dateToUse.Date.Year);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error parsing date");
            }

            return null;
        }

        /// <summary>
        /// Gets the link to the Wikipedia page
        /// </summary>
        /// <param name="gameData">game data with the key to the page</param>
        /// <returns>List of found links</returns>
        internal List<Link> GetLinks(WikipediaPage gameData) => gameData?.Key?.Any() ?? false
            ? new List<Link>
            {
                new Link("Wikipedia", "https://en.wikipedia.org/wiki/" + gameData.Key)
            }
            : null;

        /// <summary>
        /// Gets all values from a field in the infobox template.
        /// </summary>
        /// <param name="infoBox">Infobox template</param>
        /// <param name="field">Name of the field</param>
        /// <param name="removeParentheses">If true all values in parentheses will be removed. Those mostly contain the
        /// platforms in the developer or publisher fields.</param>
        /// <param name="prefix">Prefix to be added to the value. Is used to categorize the tags.</param>
        /// <param name="removeSup">Removes values in superscripts.</param>
        /// <returns>List of all found values</returns>
        internal List<MetadataProperty> GetValues(Template infoBox, string field, bool removeParentheses = false, string prefix = "", bool removeSup = false)
        {
            try
            {
                var argument = infoBox.Arguments[field];

                if (argument != null)
                {
                    var values = new List<MetadataProperty>();

                    // We go through all list templates used in the field to fetch the single values in the list.
                    foreach (var template in argument.EnumDescendants().OfType<Template>()
                        .Where(t => Resources.ListTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
                    {
                        var arguments = new List<TemplateArgument>();

                        // In the template vgrelease every odd argument is supposed to be the country. So we only use the even
                        // ones as values.
                        if (Resources.VgReleaseTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(template.Name).ToLower()))
                        {
                            var counter = 1;

                            foreach (var arg in template.Arguments)
                            {
                                if (counter % 2 == 0)
                                {
                                    arguments.Add(arg);
                                }

                                counter++;
                            }
                        }
                        else
                        {
                            arguments.AddRange(template.Arguments);
                        }

                        foreach (var listArgument in arguments)
                        {
                            if (!(listArgument.Name is null) && listArgument.Name.ToPlainText() != "title")
                            {
                                continue;
                            }

                            var sublistTemplates = listArgument.EnumDescendants().OfType<Template>().Where(t => Resources.AllowedSubListTemplates.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower())).ToList();

                            if (sublistTemplates.Count > 0)
                            {
                                foreach (var sublistTemplate in sublistTemplates)
                                {
                                    if (MwParserUtility.NormalizeTemplateArgumentName(sublistTemplate.Name).ToLower() == "start date")
                                    {
                                        values.Add(new MetadataNameProperty(string.Join("-", sublistTemplate.Arguments)));
                                    }
                                    else
                                    {
                                        foreach (var sublistArgument in sublistTemplate.Arguments)
                                        {
                                            values.AddRange(CleanUpAndSplit(sublistArgument, field, removeParentheses, removeSup));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                values.AddRange(CleanUpAndSplit(listArgument, field, removeParentheses, removeSup));
                            }
                        }
                    }

                    // We now remove all sub templates to get the value itself, that won't be added otherwise.
                    foreach (var template in argument.EnumDescendants().OfType<Template>())
                    {
                        template.Remove();
                    }

                    // All additional elements like cite etc. will be removed, too.
                    values.AddMissing(CleanUpAndSplit(argument, field, removeParentheses, removeSup));

                    // Now we add the prefix to all
                    return string.IsNullOrEmpty(prefix)
                        ? values
                        : new List<MetadataProperty>(values.Select(v => new MetadataNameProperty($"{prefix} {v}")));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error parsing argument '{field}'");
            }

            return new List<MetadataProperty>();
        }

        /// <summary>
        /// Strips an argument of unwanted elements
        /// </summary>
        /// <param name="argument">argument to clean up.</param>
        /// <returns>The cleaned up argument</returns>
        internal TemplateArgument StripUnwantedElements(TemplateArgument argument)
        {
            // First we remove every template we don't want.
            foreach (var x in argument.EnumDescendants().OfType<Template>()
                .Where(t => Resources.UnwantedTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
            {
                x.Remove();
            }

            // Now we also remove <ref> tags, because those contain footnotes etc., we don't need.
            foreach (var line in argument.Value.Lines)
            {
                foreach (var inline in line.EnumDescendants())
                {
                    if (inline.ToString().StartsWith("<ref"))
                    {
                        inline.Remove();
                    }
                }
            }

            return argument;
        }
    }
}