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
    public class WikitextParser
    {
        private readonly WikipediaMetadata plugin;

        public WikipediaGameMetadata GameMetadata { get; set; }

        /// <summary>
        /// Creates an instance of the class and fills the parameters by parsing the wikitext.
        /// </summary>
        /// <param name="gameData">Page object from wikipedia containing the wikitext and other data.</param>
        public WikitextParser(WikipediaMetadata plugin, WikipediaPage gameData)
        {
            this.plugin = plugin;

            GameMetadata = new WikipediaGameMetadata();

            if (gameData.Source != null)
            {
                try
                {
                    MwParserFromScratch.WikitextParser parser = new MwParserFromScratch.WikitextParser();
                    string text = gameData.Source;
                    Wikitext ast = parser.Parse(text);

                    GameMetadata.Key = gameData.Key;

                    // Most of the game relevant data can be found in the "infobox video game" template. Most Wikipedia pages
                    // for games have one of those. Without it, only name, cover image, description and links can be fetched.
                    Template infoBox = ast.EnumDescendants().OfType<Template>()
                        .Where(t => CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)) == "infobox video game").FirstOrDefault();

                    if (infoBox != null)
                    {
                        TemplateArgument gameTitle = infoBox.Arguments["title"];
                        if (gameTitle != null)
                        {
                            GameMetadata.Name = gameTitle.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                        }

                        GameMetadata.CoverImageUrl = GetImageUrl(gameData.Key);
                        GameMetadata.ReleaseDate = GetDate(infoBox);
                        GameMetadata.Genres = GetValues(infoBox, "genre");
                        GameMetadata.Developers = GetValues(infoBox, "developer", true);
                        GameMetadata.Publishers = GetValues(infoBox, "publisher", true);
                        GameMetadata.Features = GetValues(infoBox, "modes");

                        GameMetadata.Tags = new List<MetadataProperty>();

                        foreach (TagSetting tagSetting in plugin.Settings.Settings.TagSettings.Where(s => s.IsChecked))
                        {
                            GameMetadata.Tags.AddRange(GetValues(infoBox, tagSetting.Name.ToLower(), false, tagSetting.Prefix));
                        }

                        GameMetadata.Links = GetLinks(gameData);
                        GameMetadata.Series = GetValues(infoBox, "series");

                        List<MetadataProperty> platforms = new List<MetadataProperty>();

                        platforms.AddRange(GetValues(infoBox, "platforms"));

                        if (plugin.Settings.Settings.ArcadeSystemAsPlatform)
                        {
                            platforms.AddRange(GetValues(infoBox, "arcade system"));
                        }

                        PlatformHelper platformHelper = new PlatformHelper(API.Instance);

                        GameMetadata.Platforms = platforms.SelectMany(p => platformHelper.GetPlatforms(p.ToString())).ToList();
                    }

                    if (string.IsNullOrEmpty(GameMetadata.Name))
                    {
                        GameMetadata.Name = gameData.Title.Replace("(video game)", "").Trim();
                    }

                    GameMetadata.CriticScore = GetCriticScore(ast);
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
        private List<MetadataProperty> GetValues(Template infoBox, string field, bool removeParentheses = false, string prefix = "", bool removeSup = false)
        {
            try
            {
                TemplateArgument argument = infoBox.Arguments[field];
                if (argument != null)
                {
                    List<MetadataProperty> values = new List<MetadataProperty>();

                    // We go through all list templates used in the field to fetch the single values in the list.
                    foreach (Template template in argument.EnumDescendants().OfType<Template>()
                            .Where(t => Ressources.ListTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
                    {
                        List<TemplateArgument> arguments = new List<TemplateArgument>();

                        // In the template vgrelease every odd argument is supposed to be the country. So we only use the even
                        // ones as values.
                        if (Ressources.VgReleaseTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(template.Name).ToLower()))
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
                            if (listArgument.Name == null || listArgument.Name.ToPlainText() == "title")
                            {
                                IEnumerable<Template> sublistTemplates = listArgument.EnumDescendants().OfType<Template>()
                                    .Where(t => Ressources.AllowedSubListTemplates.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower()));

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
                                                values.AddRange(Split(StripUnwantedElements(sublistArgument).Value.ToString(), field, removeParentheses, removeSup));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    values.AddRange(Split(StripUnwantedElements(listArgument).Value.ToString(), field, removeParentheses, removeSup));
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
                    argument = StripUnwantedElements(argument);

                    values.AddMissing(Split(argument.Value.ToString(), field, removeParentheses, removeSup));

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
        /// <returns></returns>
        private ReleaseDate? GetDate(Template infoBox)
        {
            try
            {
                // We use the GetValues function to fetch all values from the "released" section.
                List<MetadataProperty> list = GetValues(infoBox, "released", true, "", true);

                List<DateTime> dates = new List<DateTime>();

                // We check each value for a valid date and at those to a datetime list.
                foreach (MetadataProperty property in list)
                {
                    if (DateTime.TryParseExact(property.ToString(), Ressources.DateFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        dates.Add(dateTime);
                    }
                }

                // If dates were found, We'll return the one depending on the settings.
                if (dates.Count > 0)
                {
                    switch (plugin.Settings.Settings.DateToUse)
                    {
                        case DateToUse.Earliest: return new ReleaseDate(dates.Min());
                        case DateToUse.Latest: return new ReleaseDate(dates.Max());
                        case DateToUse.First: return new ReleaseDate(dates.First());
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
        /// <returns></returns>
        private List<Link> GetLinks(WikipediaPage gameData)
        {
            List<Link> links = new List<Link>
            {
                new Link("Wikipedia", "https://en.wikipedia.org/wiki/" + gameData.Key)
            };

            return links;
        }

        /// <summary>
        /// Fetches the url of the main image from the page.
        /// </summary>
        /// <param name="key">Key of the page</param>
        /// <returns>Url of the image</returns>
        private string GetImageUrl(string key)
        {
            WikipediaImage imageData = ApiCaller.GetImage(key);

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

                if (list.Count == 0)
                {
                    foreach (string code in Ressources.PlatformCodes)
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
                    foreach (string code in Ressources.PlatformCodes)
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
                    switch (plugin.Settings.Settings.RatingToUse)
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
        /// Splits values, that are separated by line breaks or commas instead of list templates.
        /// </summary>
        /// <param name="value">value to be split</param>
        /// <param name="field">name of the field. Is used to recognize fields, that can contain values that have commas
        /// we don't want to split.</param>
        /// <param name="removeParentheses">Removes values in parentheses.</param>
        /// <param name="removeSup">Removes values in superscripts.</param>
        /// <returns>List of values</returns>
        private List<MetadataNameProperty> Split(string value, string field, bool removeParentheses = false, bool removeSup = false)
        {
            MwParserFromScratch.WikitextParser parser = new MwParserFromScratch.WikitextParser();
            List<MetadataNameProperty> values = new List<MetadataNameProperty>();

            // We remove all parentheses from the values.
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

            // If the value is the only one and is a link, we return it without splitting.
            if (value.Count(c => c == '[') == 2 && value.Count(c => c == ']') == 2 && value.EndsWith("]"))
            {
                values.Add(new MetadataNameProperty(parser.Parse(value).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim()));
                return values;
            }

            // Now the build the list of separators to split the values by.
            List<string> separators = new List<string>();

            separators.AddRange(Ressources.StringSeparators);

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
        private TemplateArgument StripUnwantedElements(TemplateArgument argument)
        {
            IEnumerable<Template> templates = argument.EnumDescendants().OfType<Template>();

            // First we remove every template we don't want.
            foreach (Template x in argument.EnumDescendants().OfType<Template>()
                            .Where(t => Ressources.UnwantedTemplateNames.Contains(CleanTemplateName(MwParserUtility.NormalizeTemplateArgumentName(t.Name)))))
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
            return name.RemoveTextBetween("<!--", "-->").ToLower();
        }
    }
}
