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
    /// Parser for the html code of a wikipedia page fetched by the rest api, to get the description and additional links.
    /// </summary>
    public class WikipediaHtmlParser
    {
        private readonly string pageHtmlUrl = "https://en.wikipedia.org/api/rest_v1/page/html/{0}";

        /// <summary>
        /// We only fetch headings, paragraphs, lists and su sections from a section, because stuff like blockquotes or tables
        /// don't work well in Playnite and usually aren't essential to the description of a video game.
        /// </summary>
        private readonly string[] allowdSecondLevelNodes = { "h2", "p", "ul", "ol", "section" };
        /// <summary>
        /// The same for third levels...
        /// </summary>
        private readonly string[] allowdThirdLevelNodes = { "h3", "p", "ul", "ol", "section" };
        /// <summary>
        /// List of rename patterns for the links
        /// </summary>
        private readonly List<LinkPair> LinkPairs = new List<LinkPair>()
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

        public string Description { get; } = string.Empty;
        public List<Link> Links { get; } = new List<Link>();

        /// <summary>
        /// Creates an instance of the class, fetches the html code and parses it.
        /// </summary>
        /// <param name="gameKey">Key of the page we want to parse</param>
        public WikipediaHtmlParser(string gameKey, WikipediaMetadata plugin)
        {
            // All paragraphs we want to remove from the description by default.
            List<string> unwantedParagraphs = new List<string>()
              { "see also", "notes", "references", "further reading", "sources", "external links" };

            unwantedParagraphs.AddMissing(plugin.Settings.Settings.SectionsToRemove.Select(s => s.ToLower().Trim()));

            // We use HTML Agility Pack to fetch and parse the code. For the description we strip all bloat from the text and
            // build a simple new html string.
            string apiUrl = string.Format(pageHtmlUrl, gameKey.UrlEncode());

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
                    // We now fetch the ul or ol list and go through all list items to fetch the links.
                    HtmlNode linkList = topLevelSection.SelectSingleNode("./ul") ?? topLevelSection.SelectSingleNode("./ol");

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
                                    LinkPair pair = LinkPairs.Where(p => link.GetAttributeValue("href", "").Contains(p.Contains)).FirstOrDefault();

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

                // Now we fetch all allowed second level nodes.
                List<HtmlNode> secondLevelNodes = topLevelSection.ChildNodes.Where(c => allowdSecondLevelNodes.Contains(c.Name)).ToList();

                foreach (HtmlNode secondLevelNode in secondLevelNodes)
                {
                    // If the heading is one of the unwanted sections, we completely omit the section.
                    if (secondLevelNode.Name == "h2" && unwantedParagraphs.Contains(secondLevelNode.InnerText.ToLower().Trim()))
                    {
                        break;
                    }
                    else if (secondLevelNode.Name == "section")
                    {
                        // We now look for third level nodes and add those to the description. Since fourth levels are very rarely
                        // used, we don't consider those for now.
                        List<HtmlNode> thirdLevelNodes = secondLevelNode.ChildNodes.Where(c => allowdThirdLevelNodes.Contains(c.Name)).ToList();

                        foreach (HtmlNode thirdLevelNode in thirdLevelNodes)
                        {
                            if (thirdLevelNode.Name == "ul" || thirdLevelNode.Name == "ol")
                            {
                                Description += GetList(thirdLevelNode) + Environment.NewLine;
                            }
                            else
                            {
                                Description += $"<{thirdLevelNode.Name}>{RemoveAnnotationMarks(thirdLevelNode).InnerText}</{thirdLevelNode.Name}>" + Environment.NewLine;
                            }
                        }
                    }
                    else if (secondLevelNode.Name == "ul" || secondLevelNode.Name == "ol")
                    {
                        Description += GetList(secondLevelNode) + Environment.NewLine;
                    }
                    else if (secondLevelNode.InnerText.RemoveSpecialChars().Trim().Length > 0)
                    {
                        Description += $"<{secondLevelNode.Name}>{RemoveAnnotationMarks(secondLevelNode).InnerText}</{secondLevelNode.Name}>" + Environment.NewLine;
                    }
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
                result += $"  <{listNode.Name}>{RemoveAnnotationMarks(listNode).InnerText}</{listNode.Name}>" + Environment.NewLine;
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
    }
}
