using KNARZhelper;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WikipediaMetadata.Models
{
    public class WikipediaGameMetadata
    {
        private readonly string[] listTemplateNames = { "unbulleted list", "ubl", "collapsible list", "flatlist", "plainlist", "vgrelease", "video game release" };
        private readonly string[] vgReleaseTemplateNames = { "vgrelease", "video game release" };
        private readonly string[] unwantedTemplateNames = { "efn", "cite web" };
        private readonly string[] stringSeparators = { "<br />", "<br/>", "<br>", "\n" };
        private readonly string[] windowsPlatform = { "Microsoft Windows", "Windows" };
        private readonly string[] dateFormatStrings = { "MM/dd/yyyy", "MMMM d, yyyy", "d MMMM yyyy" };

        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public ReleaseDate? ReleaseDate { get; set; }
        public List<MetadataProperty> Genres { get; set; }
        public List<MetadataProperty> Developers { get; set; }
        public List<MetadataProperty> Publishers { get; set; }
        public List<MetadataProperty> Features { get; set; }
        public List<MetadataProperty> Tags { get; set; }
        public List<Link> Links { get; set; }
        public List<MetadataProperty> Series { get; set; }
        public List<MetadataProperty> Platforms { get; set; }

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

                    Template infoBox = ast.EnumDescendants().OfType<Template>()
                        .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower() == "infobox video game").First();

                    if (infoBox != null)
                    {
                        TemplateArgument gameTitle = infoBox.Arguments["title"];
                        if (gameTitle != null)
                        {
                            Name = gameTitle.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                        }

                        ReleaseDate = GetDate(infoBox);
                        Genres = GetValues(infoBox, "genre");
                        Developers = GetValues(infoBox, "developer", true);
                        Publishers = GetValues(infoBox, "publisher", true);
                        Features = GetValues(infoBox, "modes");

                        Tags = new List<MetadataProperty>();

                        Tags.AddRange(GetValues(infoBox, "engine", false, "[engine]"));
                        Tags.AddRange(GetValues(infoBox, "director", false, "[people] director:"));
                        Tags.AddRange(GetValues(infoBox, "producer", false, "[people] producer:"));
                        Tags.AddRange(GetValues(infoBox, "designer", false, "[people] designer:"));
                        Tags.AddRange(GetValues(infoBox, "programmer", false, "[people] programmer:"));
                        Tags.AddRange(GetValues(infoBox, "artist", false, "[people] artist:"));
                        Tags.AddRange(GetValues(infoBox, "writer", false, "[people] writer:"));
                        Tags.AddRange(GetValues(infoBox, "composer", false, "[people] composer:"));

                        Links = GetLinks(gameData);
                        Series = GetValues(infoBox, "series");

                        Platforms = new List<MetadataProperty>();

                        Platforms.AddRange(GetValues(infoBox, "platforms"));
                        Platforms.AddRange(GetValues(infoBox, "arcade system"));
                    }

                    if (Name == string.Empty || Name == null)
                    {
                        Name = gameData.Title.Replace("(video game)", "").Trim();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error parsing Wikitext");
                }
            }
        }

        private ReleaseDate? GetDate(Template infoBox)
        {
            try
            {
                List<MetadataProperty> list = GetValues(infoBox, "released");

                List<DateTime> dates = new List<DateTime>();

                foreach (MetadataProperty property in list)
                {
                    if (DateTime.TryParseExact(property.ToString(), dateFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        dates.Add(dateTime);
                    }
                }

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

        private List<MetadataProperty> GetValues(Template infoBox, string field, bool removeParentheses = false, string prefix = "")
        {
            try
            {
                TemplateArgument argument = infoBox.Arguments[field];
                if (argument != null)
                {
                    List<MetadataProperty> values = new List<MetadataProperty>();

                    foreach (Template template in argument.EnumDescendants().OfType<Template>()
                            .Where(t => listTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower())))
                    {
                        // In the template vgrelease the first argument is supposed to be the country.
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

                    argument = StripUnwantedElements(argument);

                    values.AddMissing(Split(argument.Value.ToString(), field, removeParentheses));


                    if (prefix != string.Empty && prefix != null)
                    {
                        List<MetadataProperty> prefixedValues = new List<MetadataProperty>();

                        foreach (MetadataProperty value in values)
                        {
                            prefixedValues.Add(new MetadataNameProperty($"{prefix} {value}"));
                        }

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

        private List<Link> GetLinks(WikipediaGameData gameData)
        {
            List<Link> links = new List<Link>
            {
                new Link("Wikipedia", "https://en.wikipedia.org/wiki/" + gameData.Key)
            };

            return links;
        }

        private List<MetadataNameProperty> Split(string value, string field, bool removeParentheses = false)
        {
            List<MetadataNameProperty> values = new List<MetadataNameProperty>();

            if (removeParentheses)
            {
                while (value.IndexOf("(") > 0)
                {
                    value = value.Remove(value.IndexOf("("), value.IndexOf(")") - value.IndexOf("(") + 1).Trim();
                }
            }

            List<string> separators = new List<string>();

            separators.AddRange(stringSeparators);

            if (field != "released")
            {
                separators.AddMissing(",");
            }

            foreach (string segment in value.Split(separators.ToArray(), 100, StringSplitOptions.RemoveEmptyEntries))
            {
                WikitextParser parser = new WikitextParser();

                string segmentEditable = parser.Parse(segment).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim();

                if (field == "platforms" && windowsPlatform.Contains(segmentEditable))
                {
                    segmentEditable = "PC (Windows)";
                }

                if (segmentEditable.Length > 0)
                {
                    values.Add(new MetadataNameProperty(segmentEditable));
                }
            }

            return values;
        }

        private TemplateArgument StripUnwantedElements(TemplateArgument argument)
        {
            IEnumerable<Template> templates = argument.EnumDescendants().OfType<Template>();

            foreach (Template x in argument.EnumDescendants().OfType<Template>()
                            .Where(t => unwantedTemplateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower())))
            {
                x.Remove();
            }

            foreach (LineNode line in argument.Value.Lines)
            {
                foreach (Node inline in line.EnumDescendants())
                {
                    if (inline.ToString().StartsWith("<ref>"))
                    {
                        inline.Remove();
                    }
                }
            }
            return argument;
        }
    }
}
