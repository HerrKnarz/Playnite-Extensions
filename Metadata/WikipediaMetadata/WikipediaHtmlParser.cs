using HtmlAgilityPack;
using KNARZhelper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace WikipediaMetadata
{
    public class LinkPair
    {
        public string Contains { get; set; }
        public string Name { get; set; }
    }

    public class WikipediaHtmlParser
    {
        private readonly string pageHtmlUrl = "https://en.wikipedia.org/api/rest_v1/page/html/{0}";
        private readonly string[] unwantedParagraphs = { "see also", "notes", "references", "further reading", "sources", "external linkList" };
        private readonly List<LinkPair> LinkPairs = new List<LinkPair>()
        {
            new LinkPair { Contains = "imdb.com",
            Name = "IMDb"},
            new LinkPair { Contains = "mobygames.com",
            Name = "MobyGames"},
            new LinkPair { Contains = "gamefaqs.gamespot.com",
            Name = "GameFAQs"},
            new LinkPair { Contains = "giantbomb.com",
            Name = "Giant Bomb"},
            new LinkPair { Contains = "arcade-museum.com",
            Name = "Killer List of Videogames"},
        };

        public string Description { get; } = string.Empty;

        public List<Link> Links { get; } = new List<Link>();
        public WikipediaHtmlParser(string gameKey)
        {
            string apiUrl = string.Format(pageHtmlUrl, gameKey.UrlEncode());

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(apiUrl);

            HtmlNodeCollection topLevelSections = doc.DocumentNode.SelectNodes("//body/section");

            foreach (HtmlNode topLevelSection in topLevelSections)
            {
                HtmlNode linkNode = topLevelSection.SelectSingleNode("./h2");

                if (linkNode != null && linkNode.InnerText.ToLower() == "external links")
                {
                    HtmlNode linkList = topLevelSection.SelectSingleNode("./ul") ?? topLevelSection.SelectSingleNode("./ol");

                    if (linkList != null)
                    {
                        HtmlNodeCollection listItems = linkList.SelectNodes("./li");
                        if (listItems != null && listItems.Count > 0)
                        {
                            foreach (HtmlNode listItem in listItems)
                            {
                                HtmlNode link = listItem.Descendants("a").FirstOrDefault();
                                if (link != null)
                                {
                                    string name = WebUtility.HtmlDecode(link.InnerText);
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

                string[] allowdSecondLevelNodes = { "h2", "p", "ul", "ol", "section" };

                List<HtmlNode> secondLevelNodes = topLevelSection.ChildNodes.Where(c => allowdSecondLevelNodes.Contains(c.Name)).ToList();

                foreach (HtmlNode secondLevelNode in secondLevelNodes)
                {
                    if (secondLevelNode.Name == "h2" && unwantedParagraphs.Contains(secondLevelNode.InnerText.ToLower()))
                    {
                        break;
                    }
                    else if (secondLevelNode.Name == "section")
                    {
                        string[] allowdThirdLevelNodes = { "h3", "p", "ul", "ol", "section" };

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
        private HtmlNode RemoveAnnotationMarks(HtmlNode text)
        {
            HtmlNodeCollection supNodes = text.SelectNodes("./sup");

            if (supNodes != null && supNodes.Count > 0)
            {
                foreach (HtmlNode annotation in supNodes)
                {
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
