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
        private readonly string[] templateNames = new[] { "unbulleted list", "ubl", "collapsible list" };

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
                        ReleaseDate = GetDate(infoBox);
                        Genres = GetValues(infoBox, "genre");
                        Developers = GetValues(infoBox, "developer");
                        Publishers = GetValues(infoBox, "publisher");
                        Features = GetValues(infoBox, "modes");
                        Links = GetLinks(gameData);
                        Series = GetValues(infoBox, "series");
                        Platforms = GetValues(infoBox, "platforms");
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

        private List<MetadataProperty> GetValues(Template infoBox, string field)
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
                            values.Add(new MetadataNameProperty(listArgument.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags)));
                        }
                    }

                    if (values.Count == 0)
                    {
                        values.Add(new MetadataNameProperty(argument.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags)));
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
    }
}
