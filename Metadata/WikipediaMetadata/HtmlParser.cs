using HtmlAgilityPack;
using KNARZhelper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    ///     Parser for the html code of a wikipedia page fetched by the rest api, to get the description and additional links.
    /// </summary>
    internal class HtmlParser
    {
        private readonly PluginSettings _settings;

        /// <summary>
        ///     Creates an instance of the class, fetches the html code and parses it.
        /// </summary>
        /// <param name="gameKey">Key of the page we want to parse</param>
        /// <param name="settings">Settings of the plugin</param>
        public HtmlParser(string gameKey, PluginSettings settings)
        {
            _settings = settings;

            // All paragraphs we want to remove from the description by default.
            var unwantedParagraphs = new List<string> { "see also", "notes", "references", "further reading", "sources", "external links" };

            unwantedParagraphs.AddMissing(settings.SectionsToRemove.Select(s => s.ToLower().Trim()));

            // We use HTML Agility Pack to fetch and parse the code. For the description we strip all bloat from the text and
            // build a simple new html string.
            var doc = new HtmlWeb().Load(string.Format(Resources.PageHtmlUrl, gameKey.UrlEncode()));

            // We go through all sections, because those typically contain the text sections of the page.
            foreach (var topLevelSection in doc.DocumentNode.SelectNodes("//body/section"))
            {
                // First we check, if the current section is the external links block by fetching its heading.
                if (topLevelSection.SelectSingleNode("./h2")?.InnerText.ToLower() == "external links")
                {
                    GetExternalLinks(topLevelSection);
                }

                // Now we fetch all allowed second level nodes.
                foreach (var secondLevelNode in topLevelSection.ChildNodes.Where(c => Resources.AllowedSecondLevelNodes.Contains(c.Name)))
                {
                    // If the heading is one of the unwanted sections, we completely omit the section.
                    if (secondLevelNode.Name == "h2" && unwantedParagraphs.Contains(secondLevelNode.InnerText.ToLower().Trim()))
                    {
                        break;
                    }

                    if (secondLevelNode.Name == "section")
                    {
                        // We now look for third level nodes and add those to the description.
                        foreach (var thirdLevelNode in secondLevelNode.ChildNodes.Where(c => Resources.AllowedThirdLevelNodes.Contains(c.Name)))
                        {
                            if (thirdLevelNode.Name == "section")
                            {
                                // We now look for fourth level nodes and add those to the description. Since further levels are
                                // very rarely used, we don't consider those for now.
                                foreach (var fourthLevelNode in thirdLevelNode.ChildNodes.Where(c => Resources.AllowedFourthLevelNodes.Contains(c.Name)))
                                {
                                    AddSectionToDescription(fourthLevelNode);
                                }
                            }
                            else
                            {
                                AddSectionToDescription(thirdLevelNode);
                            }
                        }
                    }
                    else
                    {
                        AddSectionToDescription(secondLevelNode);
                    }
                }

                // If we only want the overview, we directly break the loop after the first section.
                if (settings.DescriptionOverviewOnly)
                {
                    break;
                }
            }
        }

        public string Description { get; set; } = string.Empty;

        public List<Link> Links { get; } = new List<Link>();

        /// <summary>
        ///     Removes annotation marks from the text, because we don't need those in the game description.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static HtmlNode RemoveAnnotationMarks(HtmlNode text)
        {
            var supNodes = text.SelectNodes("./sup");

            if ((supNodes?.Count ?? 0) == 0)
            {
                return text;
            }

            foreach (var annotation in supNodes)
            {
                if (annotation.Attributes.Any(a => a.Name == "class" && a.Value.Contains("update")))
                {
                    annotation.Remove();
                }

                if (annotation.SelectSingleNode("./a/span[@class='mw-reflink-text']") != null)
                {
                    annotation.Remove();
                }
            }

            return text;
        }

        /// <summary>
        ///     Adds the provided section to the description
        /// </summary>
        /// <param name="node">The section to add</param>
        private void AddSectionToDescription(HtmlNode node)
        {
            if (node.Name.IsOneOf("ul", "ol"))
            {
                Description += GetList(node) + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                var text = RemoveUnwantedTags(RemoveAnnotationMarks(node), Resources.AllowedParagraphTags).InnerHtml.Trim();

                if (text.Any())
                {
                    Description += $"<{node.Name}>{text}</{node.Name}>" + Environment.NewLine + Environment.NewLine;
                }
            }
        }

        /// <summary>
        ///     Gets the external links from the provided node.
        /// </summary>
        /// <param name="node">Node with the links to add.</param>
        private void GetExternalLinks(HtmlNode node)
        {
            // We now fetch the ul or ol list and go through all list items to fetch the links.
            var linkList = node.SelectSingleNode("./ul[not(contains(@class,'portalbox'))]") ?? node.SelectSingleNode("./ol[not(contains(@class,'portalbox'))]");

            if (linkList == null)
            {
                return;
            }

            var listItems = linkList.SelectNodes("./li");

            if (!(listItems?.Any() ?? false))
            {
                return;
            }

            foreach (var listItem in listItems)
            {
                // We only use the first link from the list value.
                var link = listItem.Descendants("a").FirstOrDefault();

                if (link == null)
                {
                    continue;
                }

                var name = WebUtility.HtmlDecode(link.InnerText);

                // Since the link text most of the time simply consist of the name of the game, try to
                // match the url to popular websites that are often linked here.
                var pair = Resources.LinkPairs.FirstOrDefault(p => link.GetAttributeValue("href", "").Contains(p.Contains));

                if (pair != null)
                {
                    name = pair.Name;
                }

                Links.Add(new Link
                {
                    Name = name,
                    Url = link.GetAttributeValue("href", "")
                });
            }
        }

        /// <summary>
        ///     Gets the content of a ul or ol list as cleaned up html code.
        /// </summary>
        /// <param name="htmlList">list to process</param>
        /// <returns>the cleaned up html string for the list and its items</returns>
        private string GetList(HtmlNode htmlList)
        {
            var result = new StringBuilder();

            result.AppendLine($"<{htmlList.Name}>");

            foreach (var listNode in htmlList.SelectNodes("./li"))
            {
                result.AppendLine($"  <{listNode.Name}>{RemoveUnwantedTags(RemoveAnnotationMarks(listNode), Resources.AllowedParagraphTags).InnerHtml}</{listNode.Name}>");
            }

            result.Append($"</{htmlList.Name}>");

            return result.ToString();
        }

        private HtmlNode RemoveUnwantedTags(HtmlNode htmlNode, string[] acceptableTags)
        {
            var tryGetNodes = htmlNode.SelectNodes("./*|./text()");

            if (tryGetNodes is null || !tryGetNodes.Any())
            {
                return htmlNode;
            }

            var nodes = new Queue<HtmlNode>(tryGetNodes);

            while (nodes.Any())
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                var childNodes = node.SelectNodes("./*|./text()");

                if (childNodes != null)
                {
                    foreach (var child in childNodes)
                    {
                        nodes.Enqueue(child);
                    }
                }

                var acceptableTagList = acceptableTags.ToList();

                if (!_settings.RemoveDescriptionLinks)
                {
                    acceptableTagList.AddMissing("a");
                }

                if (!acceptableTagList.Contains(node.Name) && node.Name != "#text")
                {
                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
                else if (node.Name != "#text")
                {
                    if (node.Name == "a")
                    {
                        if (!(node.Attributes?.Any() ?? false))
                        {
                            continue;
                        }

                        foreach (var attribute in node.Attributes.ToList())
                        {
                            if (attribute.Name == "href")
                            {
                                if (attribute.Value.StartsWith("./"))
                                {
                                    attribute.Value = $"https://en.wikipedia.org/wiki/{attribute.Value.Remove(0, 2)}";
                                }
                            }
                            else
                            {
                                attribute.Remove();
                            }
                        }
                    }
                    else
                    {
                        node.Attributes.RemoveAll();
                    }
                }
            }

            return htmlNode;
        }
    }
}