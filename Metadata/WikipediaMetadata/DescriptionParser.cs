using HtmlAgilityPack;
using KNARZhelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WikipediaMetadata
{
    public class DescriptionParser
    {
        private readonly string pageHtmlUrl = "https://en.wikipedia.org/api/rest_v1/page/html/{0}";

        public string Description { get; } = string.Empty;
        public DescriptionParser(string gameKey)
        {
            string apiUrl = string.Format(pageHtmlUrl, gameKey.UrlEncode());

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(apiUrl);

            HtmlNodeCollection topLevelSections = doc.DocumentNode.SelectNodes("//body/section");

            foreach (HtmlNode topLevelSection in topLevelSections)
            {
                string[] allowdSecondLevelNodes = { "h2", "p", "ul", "ol", "section" };

                List<HtmlNode> secondLevelNodes = topLevelSection.ChildNodes.Where(c => allowdSecondLevelNodes.Contains(c.Name)).ToList();

                foreach (HtmlNode secondLevelNode in secondLevelNodes)
                {
                    string[] unwantedParagraphs = { "see also", "notes", "references", "external links", "further reading", "sources" };

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
