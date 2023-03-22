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
    /// Parses the given wikitext to get the relevant metadata informations.
    /// </summary>
    internal class WikitextParser
    {
        private readonly PluginSettings _settings;

        public WikipediaGameMetadata GameMetadata { get; set; }

        /// <summary>
        /// Creates an instance of the class and fills the parameters by parsing the wikitext.
        /// </summary>
        /// <param name="settings">Settings of the _plugin</param>
        /// <param name="gameData">Page object from wikipedia containing the wikitext and other data.</param>
        /// <param name="platformList">List of all platforms in the database</param>
        public WikitextParser(PluginSettings settings)
            => _settings = settings;


        public void Parse(WikipediaPage gameData, IItemCollection<Platform> platformList)
        {
            GameMetadata = new WikipediaGameMetadata
            {
                Key = gameData.Key,
                Links = GetLinks(gameData)
            };

            if (gameData.Source != null)
            {
                try
                {
                    Wikitext ast = new MwParserFromScratch.WikitextParser().Parse(gameData.Source);

                    GameMetadata.CriticScore = GetCriticScore(ast);

                    // Most of the game relevant data can be found in the "infobox video game" template. Most Wikipedia pages
                    // for games have one of those. Without it, only name, cover image, description and links can be fetched.
                    Template infoBox = ast.EnumDescendants().OfType<Template>()
                        .Where(t => Resources.InfoBoxVideoGameTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))).FirstOrDefault();

                    if (infoBox != null)
                    {
                        TemplateArgument gameTitle = infoBox.Arguments["title"];
                        if (gameTitle != null)
                        {
                            GameMetadata.Name = gameTitle.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                        }

                        GameMetadata.CoverImageUrl = WikipediaHelper.GetImageUrl(gameData.Key);
                        GameMetadata.ReleaseDate = GetDate(infoBox);
                        GameMetadata.Genres = GetValues(infoBox, "genre");
                        GameMetadata.Developers = GetValues(infoBox, "developer", true);
                        GameMetadata.Publishers = GetValues(infoBox, "publisher", true);
                        GameMetadata.Features = GetValues(infoBox, "modes");

                        GameMetadata.Tags = new List<MetadataProperty>();

                        foreach (TagSetting tagSetting in _settings.TagSettings.Where(s => s.IsChecked))
                        {
                            GameMetadata.Tags.AddRange(GetValues(infoBox, tagSetting.Name.ToLower(), false, tagSetting.Prefix));
                        }

                        GameMetadata.Series = GetValues(infoBox, "series");

                        List<MetadataProperty> platforms = new List<MetadataProperty>();

                        platforms.AddRange(GetValues(infoBox, "platforms"));

                        if (_settings.ArcadeSystemAsPlatform)
                        {
                            platforms.AddRange(GetValues(infoBox, "arcade system"));
                        }

                        PlatformHelper platformHelper = new PlatformHelper(platformList.ToList());

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
        }

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
                TemplateArgument argument = infoBox.Arguments[field];
                if (argument != null)
                {
                    List<MetadataProperty> values = new List<MetadataProperty>();

                    // We go through all list templates used in the field to fetch the single values in the list.
                    foreach (Template template in argument.EnumDescendants().OfType<Template>()
                            .Where(t => Resources.ListTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
                    {
                        List<TemplateArgument> arguments = new List<TemplateArgument>();

                        // In the template vgrelease every odd argument is supposed to be the country. So we only use the even
                        // ones as values.
                        if (Resources.VgReleaseTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(template.Name).ToLower()))
                        {
                            int counter = 1;
                            foreach (TemplateArgument arg in template.Arguments)
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

                        foreach (TemplateArgument listArgument in arguments)
                        {
                            if (listArgument.Name is null || listArgument.Name.ToPlainText() == "title")
                            {
                                IEnumerable<Template> sublistTemplates = listArgument.EnumDescendants().OfType<Template>()
                                    .Where(t => Resources.AllowedSubListTemplates.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower()));

                                if (sublistTemplates != null && sublistTemplates.Count() > 0)
                                {
                                    foreach (Template sublistTemplate in sublistTemplates)
                                    {
                                        if (MwParserUtility.NormalizeTemplateArgumentName(sublistTemplate.Name).ToLower() == "start date")
                                        {
                                            values.Add(new MetadataNameProperty(string.Join("-", sublistTemplate.Arguments)));
                                        }
                                        else
                                        {
                                            foreach (TemplateArgument sublistArgument in sublistTemplate.Arguments)
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
                    }

                    // We now remove all sub templates to get the value itself, that won't be added otherwise.
                    foreach (Template template in argument.EnumDescendants().OfType<Template>())
                    {
                        template.Remove();
                    }

                    // All additional elements like cite etc. will be removed, too.
                    values.AddMissing(CleanUpAndSplit(argument, field, removeParentheses, removeSup));

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
        /// Gets the earliest release date from all dates found in the infobox template.
        /// </summary>
        /// <param name="infoBox">Infobox template</param>
        /// <returns>The found release date or null</returns>
        internal ReleaseDate? GetDate(Template infoBox)
        {
            try
            {
                // We use the GetValues function to fetch all values from the "released" section.
                List<MetadataProperty> list = GetValues(infoBox, "released", true, "", true);

                List<PartialDate> dates = new List<PartialDate>();

                DateTime dateTime;

                // We check each value for a valid date and at those to a datetime list.
                foreach (MetadataProperty property in list)
                {
                    if (DateTime.TryParseExact(property.ToString(), Resources.DateFormatStringsFull, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
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
                    foreach (PartialDate date in dates)
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

                    PartialDate dateToUse = null;

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
                    }

                    if (dateToUse != null)
                    {
                        if (dateToUse.HasDay)
                        {
                            return new ReleaseDate(dateToUse.Date);
                        }
                        else if (dateToUse.HasMonth)
                        {
                            return new ReleaseDate(dateToUse.Date.Year, dateToUse.Date.Month);
                        }
                        else
                        {
                            return new ReleaseDate(dateToUse.Date.Year);
                        }
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
        internal List<Link> GetLinks(WikipediaPage gameData) => new List<Link>
        {
            new Link("Wikipedia", "https://en.wikipedia.org/wiki/" + gameData.Key)
        };

        /// <summary>
        /// Gets the average metacritic score from all platforms mentioned in the video game reviews template.
        /// </summary>
        /// <param name="ast">Wikitext object that contains the review box.</param>
        /// <returns>The found critic score</returns>
        internal int GetCriticScore(Wikitext ast)
        {
            // We search for the first occurrence of a review template in the page.
            Template infoBox = ast.EnumDescendants().OfType<Template>()
                .Where(t => CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)) == "video game reviews").FirstOrDefault();

            if (infoBox != null)
            {
                // Now we use the GetValues function to get all review ratings from the metacritic section in the template.
                List<MetadataProperty> list = GetValues(infoBox, "MC", true);

                List<int> ratings = new List<int>();

                if (list.Count == 0)
                {
                    foreach (string code in Resources.PlatformCodes)
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
                    foreach (string code in Resources.PlatformCodes)
                    {
                        list.AddRange(GetValues(infoBox, $"GR_{code}", true));
                    }
                }

                // We go through each value, remove everything before a colon (those are platform names) and
                // then get the integer value before the slash or percent character.
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
                    else if (value.IndexOf("%") > 0)
                    {
                        if (int.TryParse(value.Substring(0, value.IndexOf("%")), out int rating))
                        {
                            ratings.Add(rating);
                        };
                    }
                }

                // If we found ratings, we return the average rating.
                if (ratings.Count > 0)
                {
                    switch (_settings.RatingToUse)
                    {
                        case RatingToUse.Lowest: return ratings.Min();
                        case RatingToUse.Highest: return ratings.Max();
                        case RatingToUse.Average: return (int)Math.Ceiling(ratings.Average());
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Cleans up a value and splits it by line breaks or commas instead of list templates.
        /// </summary>
        /// <param name="value">value to be split</param>
        /// <param name="field">name of the field. Is used to recognize fields, that can contain values that have commas
        /// we don't want to split.</param>
        /// <param name="removeParentheses">Removes values in parentheses.</param>
        /// <param name="removeSup">Removes values in superscripts.</param>
        /// <returns>List of values</returns>
        internal List<MetadataNameProperty> CleanUpAndSplit(TemplateArgument argument, string field, bool removeParentheses = false, bool removeSup = false)
        {
            // First we remove unwanted elements on template level and get the text value of the argument.
            string value = StripUnwantedElements(argument).Value.ToString();

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

            MwParserFromScratch.WikitextParser parser = new MwParserFromScratch.WikitextParser();
            List<MetadataNameProperty> values = new List<MetadataNameProperty>();

            // If the value is the only one and is a link, we return it without splitting.
            if (value.Count(c => c == '[') == 2 && value.Count(c => c == ']') == 2 && value.StartsWith("[") && value.EndsWith("]"))
            {
                values.Add(new MetadataNameProperty(parser.Parse(value).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim()));
                return values;
            }

            // Now the build the list of separators to split the values by.
            List<string> separators = new List<string>();

            separators.AddRange(Resources.StringSeparators);

            // Fields for release dates and metacritic contain commas we don't want to split, so we leave commas out of the list.
            // We also don't split by comma, if the value is already from a list.
            if (field != "released" && field != "MC" && field != "GR")
            {
                // We only add a comma, if the string isn't already separated by one of the other separators to retain wanted
                // commas in company names etc.. This is no perfect solution but better than always splitting by comma.
                bool addComma = true;

                foreach (string separator in separators)
                {
                    if (value.IndexOf(separator) > -1)
                    {
                        addComma = false;
                        break;
                    }
                }

                if (addComma)
                {
                    separators.AddMissing(",");
                }
            }

            // Now we split the values by the list of separators and parse the result to get the plain text values.
            foreach (string segment in value.Split(separators.ToArray(), 100, StringSplitOptions.RemoveEmptyEntries))
            {
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
        internal TemplateArgument StripUnwantedElements(TemplateArgument argument)
        {
            IEnumerable<Template> templates = argument.EnumDescendants().OfType<Template>();

            // First we remove every template we don't want.
            foreach (Template x in argument.EnumDescendants().OfType<Template>()
                            .Where(t => Resources.UnwantedTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
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
        /// Cleans up a template name, because sometimes those contain html comment blocks, and converts it to lower case.
        /// </summary>
        /// <param name="name">name of the template</param>
        /// <returns>The cleaned up name</returns>
        internal string CleanTemplateName(string name)
        {
            if (name.IndexOf("\n") > 0)
            {
                name = name.Substring(0, name.IndexOf("\n")).Trim();
            }

            return name.RemoveTextBetween("<!--", "-->").ToLower();
        }
    }
}
