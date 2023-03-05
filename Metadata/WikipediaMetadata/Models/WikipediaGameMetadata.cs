using KNARZhelper;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WikipediaMetadata.Models
{
    public class WikipediaGameMetadata
    {
        /*
            MetadataField.Name,
            MetadataField.ReleaseDate,
            MetadataField.Genres,
            MetadataField.Developers,
            MetadataField.Publishers,
            MetadataField.Features,
            MetadataField.Tags,
            MetadataField.Links,
            MetadataField.Series,
            MetadataField.Platform,
            MetadataField.CoverImage,
            //MetadataField.CriticScore,
            //MetadataField.CommunityScore,
            //MetadataField.AgeRating,
            MetadataField.Description,
         */
        private readonly string[] templateNames = { "unbulleted list", "ubl", "collapsible list" };

        private readonly string[] stringSpearators = { "<br />", "<br>", "," };

        private readonly string[] windowsPlatform = { "Microsoft Windows", "Windows" };

        public string Name { get; set; } = string.Empty;
        public ReleaseDate? ReleaseDate { get; set; }
        public List<MetadataProperty> Genres { get; set; }
        public List<MetadataProperty> Developers { get; set; }
        public List<MetadataProperty> Publishers { get; set; }
        public List<MetadataProperty> Features { get; set; }
        public List<MetadataProperty> Tags { get; set; }
        public List<Link> Links { get; set; }
        public List<MetadataProperty> Series { get; set; }
        public List<MetadataProperty> Platforms { get; set; }
        public string Description { get; set; } = string.Empty;

        public WikipediaGameMetadata(WikipediaGameData gameData)
        {
            if (gameData.Source != null)
            {
                try
                {
                    WikitextParser parser = new WikitextParser();
                    string text = gameData.Source;
                    Wikitext ast = parser.Parse(text);

                    Name = gameData.Title;

                    Description = ast.ToPlainText(NodePlainTextOptions.RemoveRefTags);

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
                TemplateArgument releaseDate = infoBox.Arguments["released"];
                if (releaseDate != null)
                {
                    Template dateBox = releaseDate.Value.EnumDescendants().OfType<Template>()
                        .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower() == "collapsible list").First();

                    if (dateBox != null)
                    {
                        TemplateArgument dateTitle = dateBox.Arguments["title"];
                        if (dateTitle != null)
                        {
                            Template date = dateTitle.Value.EnumDescendants().OfType<Template>()
                                .Where(t => MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower() == "nobold").FirstOrDefault();

                            string dateString = string.Empty;

                            if (date != null)
                            {
                                dateString = date.Arguments.First().Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);

                            }
                            else
                            {
                                dateString = dateTitle.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);
                            }

                            DateTime dateTime = DateTime.ParseExact(dateString, "MMMM d, yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);

                            return new ReleaseDate(dateTime);
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

        private List<MetadataProperty> GetValues(Template infoBox, string field, bool removeParentheses = false, string prefix = "")
        {
            try
            {
                TemplateArgument argument = infoBox.Arguments[field];
                if (argument != null)
                {
                    List<MetadataProperty> values = new List<MetadataProperty>();

                    foreach (Template template in argument.EnumDescendants().OfType<Template>()
                            .Where(t => templateNames.Contains(MwParserUtility.NormalizeTemplateArgumentName(t.Name).ToLower())))
                    {
                        foreach (TemplateArgument listArgument in template.Arguments)
                        {
                            string value = listArgument.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags);

                            if (removeParentheses && value.IndexOf("(") > 0)
                            {
                                value = value.Substring(0, value.IndexOf("(") - 1).Trim();
                            }

                            values.AddRange(Split(value, field, removeParentheses));
                        }
                    }

                    if (values.Count == 0)
                    {
                        values.AddRange(Split(argument.Value.ToString(), field, removeParentheses));
                    }

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

            return null;
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

            foreach (string segment in value.Split(stringSpearators, 100, StringSplitOptions.RemoveEmptyEntries))
            {
                WikitextParser parser = new WikitextParser();

                string segmentEditable = parser.Parse(segment).ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim();

                if (removeParentheses && segment.IndexOf("(") > 0)
                {
                    segmentEditable = segmentEditable.Substring(0, segment.IndexOf("(") - 1).Trim();
                }

                if (field == "platforms" && windowsPlatform.Contains(segmentEditable))
                {
                    segmentEditable = "PC (Windows)";
                }

                values.Add(new MetadataNameProperty(segmentEditable));
            }

            return values;
        }
    }
}
