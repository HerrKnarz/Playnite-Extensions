using HtmlAgilityPack;
using KNARZhelper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace WikipediaMetadata
{

    /// <summary>
    /// Parser for the html code of a wikipedia page fetched by the rest api, to get the description and additional links.
    /// </summary>
    public class HtmlParser
    {
        public string Description { get; set; } = string.Empty;
        public List<Link> Links { get; } = new List<Link>();

        private readonly WikipediaMetadata plugin;

        /// <summary>
        /// Creates an instance of the class, fetches the html code and parses it.
        /// </summary>
        /// <param name="gameKey">Key of the page we want to parse</param>
        public HtmlParser(string gameKey, WikipediaMetadata plugin)
        {
            this.plugin = plugin;

            // All paragraphs we want to remove from the description by default.
            List<string> unwantedParagraphs = new List<string>()
              { "see also", "notes", "references", "further reading", "sources", "external links" };

            unwantedParagraphs.AddMissing(plugin.Settings.Settings.SectionsToRemove.Select(s => s.ToLower().Trim()));

            // We use HTML Agility Pack to fetch and parse the code. For the description we strip all bloat from the text and
            // build a simple new html string.
            string apiUrl = string.Format(Resources.PageHtmlUrl, gameKey.UrlEncode());

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(apiUrl);

            // We go through all sections, because those typically contain the text sections of the page.
            HtmlNodeCollection topLevelSections = doc.DocumentNode.SelectNodes("//body/section");

            foreach (HtmlNode topLevelSection in topLevelSections)
            {
                // First we check, if the current section is the "external links block by fetching its heading.
                HtmlNode linkNode = topLevelSection.SelectSingleNode("./h2");

                if (linkNode != null && linkNode.InnerText.ToLower() == "external links")
                {
                    GetExternalLinks(topLevelSection);
                }

                // Now we fetch all allowed second level nodes.
                List<HtmlNode> secondLevelNodes = topLevelSection.ChildNodes.Where(c => Resources.AllowedSecondLevelNodes.Contains(c.Name)).ToList();

                foreach (HtmlNode secondLevelNode in secondLevelNodes)
                {
                    // If the heading is one of the unwanted sections, we completely omit the section.
                    if (secondLevelNode.Name == "h2" && unwantedParagraphs.Contains(secondLevelNode.InnerText.ToLower().Trim()))
                    {
                        break;
                    }
                    else if (secondLevelNode.Name == "section")
                    {
                        // We now look for third level nodes and add those to the description.
                        List<HtmlNode> thirdLevelNodes = secondLevelNode.ChildNodes.Where(c => Resources.AllowedThirdLevelNodes.Contains(c.Name)).ToList();

                        foreach (HtmlNode thirdLevelNode in thirdLevelNodes)
                        {
                            if (thirdLevelNode.Name == "section")
                            {
                                // We now look for fourth level nodes and add those to the description. Since further levels are
                                // very rarely used, we don't consider those for now.
                                List<HtmlNode> fourthLevelNodes = thirdLevelNode.ChildNodes.Where(c => Resources.AllowedFourthLevelNodes.Contains(c.Name)).ToList();

                                foreach (HtmlNode fourthLevelNode in fourthLevelNodes)
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
                if (plugin.Settings.Settings.DescriptionOverviewOnly)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the content of a ul or ol list as cleaned up html code.
        /// </summary>
        /// <param name="htmlList">list to process</param>
        /// <returns>the cleaned up html string for the list and its items</returns>
        private string GetList(HtmlNode htmlList)
        {
            string result = $"<{htmlList.Name}>" + Environment.NewLine;

            foreach (HtmlNode listNode in htmlList.SelectNodes("./li"))
            {
                result += $"  <{listNode.Name}>{RemoveUnwantedTags(RemoveAnnotationMarks(listNode), Resources.AllowedParagraphTags).InnerHtml}</{listNode.Name}>" + Environment.NewLine;
            }

            result += $"</{htmlList.Name}>";

            return result;
        }

        /// <summary>
        /// Removes annotation marks from the text, because we don't need those in the game description.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private HtmlNode RemoveAnnotationMarks(HtmlNode text)
        {
            HtmlNodeCollection supNodes = text.SelectNodes("./sup");

            if (supNodes != null && supNodes.Count > 0)
            {
                foreach (HtmlNode annotation in supNodes)
                {
                    if (annotation.Attributes.Where(a => a.Name == "class" && a.Value.Contains("update")) != null)
                    {
                        annotation.Remove();
                    }

                    if (annotation.SelectSingleNode("./a/span[@class='mw-reflink-text']") != null)
                    {
                        annotation.Remove();
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Adds the provided section to the description
        /// </summary>
        /// <param name="node">The section to add</param>
        private void AddSectionToDescription(HtmlNode node)
        {
            if (node.Name == "ul" || node.Name == "ol")
            {
                Description += GetList(node) + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                string text = RemoveUnwantedTags(RemoveAnnotationMarks(node), Resources.AllowedParagraphTags).InnerHtml.Trim();

                if (text.Count() > 0)
                {
                    Description += $"<{node.Name}>{text}</{node.Name}>" + Environment.NewLine + Environment.NewLine;
                }
            }
        }

        /// <summary>
        /// Gets the external links from the provided node.
        /// </summary>
        /// <param name="node">Node with the links to add.</param>
        public void GetExternalLinks(HtmlNode node)
        {
            // We now fetch the ul or ol list and go through all list items to fetch the links.
            HtmlNode linkList = node.SelectSingleNode("./ul[not(contains(@class,'portalbox'))]") ?? node.SelectSingleNode("./ol[not(contains(@class,'portalbox'))]");

            if (linkList != null)
            {
                HtmlNodeCollection listItems = linkList.SelectNodes("./li");
                if (listItems != null && listItems.Count > 0)
                {
                    foreach (HtmlNode listItem in listItems)
                    {
                        // We only use the first link from the list value.
                        HtmlNode link = listItem.Descendants("a").FirstOrDefault();
                        if (link != null)
                        {
                            string name = WebUtility.HtmlDecode(link.InnerText);

                            // Since the link text most of the time simply consist of the name of the game, try to
                            // match the url to popular web sites that are often linked here.
                            LinkPair pair = Resources.LinkPairs.Where(p => link.GetAttributeValue("href", "").Contains(p.Contains)).FirstOrDefault();

                            if (pair != null)
                            {
                                name = pair.Name;
                            }

                            Links.Add(new Link()
                            {
                                Name = name,
                                Url = link.GetAttributeValue("href", ""),
                            });
                        }
                    }
                }
            }
        }

        private HtmlNode RemoveUnwantedTags(HtmlNode htmlNode, string[] acceptableTags)
        {
            HtmlNodeCollection tryGetNodes = htmlNode.SelectNodes("./*|./text()");

            if (tryGetNodes == null || !tryGetNodes.Any())
            {
                return htmlNode;
            }

            Queue<HtmlNode> nodes = new Queue<HtmlNode>(tryGetNodes);

            while (nodes.Count > 0)
            {
                HtmlNode node = nodes.Dequeue();
                HtmlNode parentNode = node.ParentNode;

                HtmlNodeCollection childNodes = node.SelectNodes("./*|./text()");

                if (childNodes != null)
                {
                    foreach (HtmlNode child in childNodes)
                    {
                        nodes.Enqueue(child);
                    }
                }

                List<string> acceptableTagList = acceptableTags.ToList();

                if (!plugin.Settings.Settings.RemoveDescriptionLinks)
                {
                    acceptableTagList.AddMissing("a");
                }

                if (!acceptableTagList.Contains(node.Name) && node.Name != "#text")
                {
                    if (childNodes != null)
                    {
                        foreach (HtmlNode child in childNodes)
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
                        if (node.Attributes != null && node.Attributes.Any())
                        {
                            foreach (HtmlAttribute attribute in node.Attributes.ToList())
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
