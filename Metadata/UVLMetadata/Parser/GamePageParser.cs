using AngleSharp.Dom;
using KNARZhelper;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UVLMetadata.Models;

namespace UVLMetadata.Parser;

internal static class CssSelectors
{
    public static string AccordionItem => ".accordion > .accordion-item";
    public static string ArticleBlock => ".articleblock";
    public static string BodySection => ".body-section";
    public static string Companies => "header .page-data";
    public static string GameName => "header h1";
    public static string InfoCard => "section > .container-fluid > .row > .col-12:nth-child(2) .card-body:nth-child(1)";
    public static string Links => ".spaced-buttons > a";
    public static string MainGenre => $"{InfoCard} span:nth-child(1)";
    public static string Perspective => $"{InfoCard} span:nth-child(2)";
    public static string Platforms => "a[data-type=platform]";
    public static string ReleaseDate => "header .page-data b:last-of-type";
}

/// <summary>
/// Parses the given page to get the relevant metadata infos.
/// </summary>
/// <param name="settings">Settings of the plugin</param>
/// <param name="uvlConnect">UVL connection</param>
public class GamePageParser(PluginSettings settings, UVLConnect uvlConnect)
{
    private readonly List<string> _gameEngines = [];
    private Dictionary<string, UVLTag> _tagDictionary;
    public UVLGameMetadata GameMetadata { get; set; }

    /// <summary>
    /// Parses the page to get the relevant metadata infos.
    /// </summary>
    /// <param name="url">URL of the page</param>
    /// <param name="gameData">IDocument containing the page data.</param>
    public void Parse(string url, IDocument gameData)
    {
        GameMetadata = new UVLGameMetadata
        {
            Url = url,
            Links = [new Link("UVL", url)]
        };

        if (gameData.Body == null)
        {
            return;
        }

        try
        {
            var infoCardHtml = gameData.QuerySelector(CssSelectors.InfoCard)?.InnerHtml;
            var accordionItems = gameData.QuerySelectorAll(CssSelectors.AccordionItem);

            GameMetadata.GalleryUrl = gameData.QuerySelector("nav.header a")?.GetAttribute("href");

            GameMetadata.Name = gameData.QuerySelector(CssSelectors.GameName)?.TextContent.Trim();
            GameMetadata.ReleaseDate = GetDate(gameData.QuerySelector(CssSelectors.ReleaseDate));
            GameMetadata.Platforms = [.. GetPlatforms(gameData.QuerySelector(CssSelectors.Platforms))];
            GameMetadata.Genres = [.. GetValueAsList(gameData.QuerySelector(CssSelectors.MainGenre), true)];
            GameMetadata.Description = GetDescription(accordionItems);
            GameMetadata.CriticScore = GetCriticScore(accordionItems);

            GetDevAndPublisher(gameData.QuerySelector(CssSelectors.Companies));

            _tagDictionary = uvlConnect.Tags.GetTagDictionary();

            GameMetadata.Tags = [];
            GameMetadata.Features = [];
            GameMetadata.Series = [];
            GameMetadata.AgeRatings = [];

            //Tags in UVL can be series, genres, features, age ratings and normal tags.
            GetInfoCardTags(infoCardHtml);
            GetTechnicalSpecs(accordionItems);
            GetTags(accordionItems);

            GameMetadata.Links.AddRange(GetTopLinks(gameData.QuerySelectorAll(CssSelectors.Links)));
            GameMetadata.Links.AddRange(GetAccordionLinks(accordionItems));
            GameMetadata.Links.AddRange(GetAccordionLinks(accordionItems, "Xref"));

            //TODO: Add project team as tags
            //TODO: Maybe add the site as a screenshot utilities source later

            GameMetadata.Tags = [.. GameMetadata.Tags.OrderBy(t => (t as MetadataNameProperty).Name)];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error parsing page");
            throw;
        }
    }

    private void AddToField(string name, TagCategory tagCategory, List<MetadataProperty> fieldOverride = null)
    {
        if (!tagCategory.Enabled && fieldOverride is null)
        {
            return;
        }

        var valueName = $"{tagCategory.Prefix}{name}";

        var field = tagCategory.ImportAs switch
        {
            MetadataField.Features => GameMetadata.Features,
            MetadataField.Genres => GameMetadata.Genres,
            MetadataField.Tags => GameMetadata.Tags,
            _ => throw new NotImplementedException(),
        };

        if (fieldOverride is not null)
        {
            field = fieldOverride;
            valueName = name;
        }

        if (!field.Any(t => (t as MetadataNameProperty).Name == valueName))
        {
            field.Add(new MetadataNameProperty(valueName));
        }
    }

    private IElement GetAccordionItem(IHtmlCollection<IElement> accordionItems, string itemTitle, string itemToGet = ".accordion-body")
    {
        foreach (var item in accordionItems)
        {
            var titleElement = item.QuerySelector(".accordion-header span");

            if (titleElement is null || !titleElement.TextContent.Trim().StartsWith(itemTitle, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var contentElement = item.QuerySelector(itemToGet);

            return contentElement;
        }

        return null;
    }

    private IEnumerable<Link> GetAccordionLinks(IHtmlCollection<IElement> accordionItems, string itemTitle = "Links")
    {
        var linkElements = GetAccordionItem(accordionItems, itemTitle)?.QuerySelectorAll("a");

        if (linkElements is null)
        {
            yield break;
        }

        foreach (var element in linkElements)
        {
            var linkName = Resources.LinkPairs.GetNameForString(element.TextContent.Trim());
            var linkUrl = element.GetAttribute("href");

            if (linkUrl.Contains("www.google.com/search") || GameMetadata.Links.Any(l => l.Url == linkUrl || l.Name == linkName))
            {
                continue;
            }

            yield return new Link(linkName, linkUrl);
        }
    }

    private int GetCriticScore(IHtmlCollection<IElement> accordionItems)
    {
        //TODO: Maybe add option to favor specific single ratings like Metacritic

        var criticScoreValues = GetAccordionItem(accordionItems, "External reviews", ".accordion-button span")?.QuerySelectorAll("b");

        if (criticScoreValues is null)
        {
            return -1;
        }

        var valueToUse = (settings.RatingToUse == RatingToUse.Average) || criticScoreValues.Length == 1 ? 0 : 1;

        return (int)Math.Round(criticScoreValues[valueToUse].TextContent.ExtractNumber());
    }

    /// <summary>
    /// Gets the release date.
    /// </summary>
    /// <param name="date">element containing the date</param>
    /// <returns>The found release date or null</returns>
    private ReleaseDate? GetDate(IElement date)
    {
        try
        {
            if (date is null)
            {
                return null;
            }

            PartialDate partialDate = null;

            var dateString = date.TextContent.Trim().Replace(" ", "");

            if (DateTime.TryParseExact(dateString, Resources.DateFormatStringsFull, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
            {
                partialDate = new PartialDate(dateTime);
            }
            else if (DateTime.TryParseExact(dateString, Resources.DateFormatStringsYearMonth, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
            {
                partialDate = new PartialDate(dateTime, false);
            }
            else if (DateTime.TryParseExact(dateString, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime))
            {
                partialDate = new PartialDate(dateTime, false, false);
            }

            return partialDate is null
                ? null
                : partialDate.HasDay
                        ? new ReleaseDate(partialDate.Date)
                        : partialDate.HasMonth
                            ? new ReleaseDate(partialDate.Date.Year, partialDate.Date.Month)
                            : new ReleaseDate(partialDate.Date.Year);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error parsing date");
        }

        return null;
    }

    private string GetDescription(IHtmlCollection<IElement> accordionItems)
    {
        string GetDescriptionElements(string elementName)
        {
            var descriptionElement = GetAccordionItem(accordionItems, elementName);

            var description = string.Empty;

            var descriptionBlocks = descriptionElement?.QuerySelectorAll(CssSelectors.ArticleBlock);

            if (descriptionBlocks == null || descriptionBlocks.Length == 0)
            {
                return null;
            }

            foreach (var articleBlock in descriptionBlocks)
            {
                if (articleBlock != descriptionBlocks.First())
                {
                    if (settings.OnlyUseFirstDescription)
                    {
                        break;
                    }

                    description += Resources.HorizontalLineHtml;
                }

                description += articleBlock?.InnerHtml.RemoveTextBetween("<div class=\"mt-2\">", "</div>").Trim();
            }

            return description;
        }

        var officialDescription = GetDescriptionElements("Official description");

        var description = GetDescriptionElements("Description");

        return settings.DescriptionToUse switch
        {
            DescriptionToUse.OfficialDescription => officialDescription ?? description,
            DescriptionToUse.Description => description ?? officialDescription,
            DescriptionToUse.Both => !officialDescription.IsNullOrEmpty() && !description.IsNullOrEmpty()
                ? $"{officialDescription}{Resources.HorizontalLineHtml}{description}"
                : officialDescription ?? description,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void GetDevAndPublisher(IElement element)
    {
        void ExtractCompanies(string companyString, bool isDeveloper)
        {
            if (companyString.IsNullOrEmpty())
            {
                return;
            }

            var Document = uvlConnect.GetDocFromString(companyString);

            var companies = GetValues([.. Document.QuerySelectorAll("a")?.Where(a => a.GetAttribute("href").StartsWith("/companies/"))]).ToList();

            if (!companies.Any())
            {
                return;
            }

            companies.RemoveAll(c => ((MetadataNameProperty)c).Name == "author");

            if (isDeveloper)
            {
                GameMetadata.Developers = companies;
            }
            else
            {
                GameMetadata.Publishers = companies;
            }
        }

        var devString = element.InnerHtml.TextBetween("developed by", "published by", false);

        if (!devString.IsNullOrEmpty())
        {
            ExtractCompanies(devString, true);

            ExtractCompanies(element.InnerHtml.Split(["published by"], StringSplitOptions.None)[1], false);

            return;
        }

        // If we don't have a construct with developed by and published by, all companies in the
        // element are developers and publishers alike.
        ExtractCompanies(element.InnerHtml, true);

        GameMetadata.Publishers = [.. GameMetadata.Developers];
    }

    private void GetInfoCardTags(string infoCardHtml)
    {
        foreach (var category in settings.TagCategories.Where(c => c.Key is TagCategoryId.GameEngine or TagCategoryId.Perspective or TagCategoryId.PlayerOptions))
        {
            if (!category.Value.Enabled || category.Value.Caption.IsNullOrEmpty())
            {
                continue;
            }

            var tagDocument = uvlConnect.GetDocFromString(infoCardHtml.TextBetween(category.Value.Caption, "<br>", false));

            if (tagDocument is null)
            {
                continue;
            }

            foreach (var tag in GetValueAsList(tagDocument.QuerySelector("a, b"), true))
            {
                AddToField(((MetadataNameProperty)tag).Name, category.Value);

                if (category.Key == TagCategoryId.GameEngine)
                {
                    _gameEngines.Add(((MetadataNameProperty)tag).Name);
                }
            }
        }
    }

    private IEnumerable<MetadataProperty> GetPlatforms(IElement platformElement)
    {
        if (platformElement == null)
        {
            yield break;
        }

        var platformName = platformElement.TextContent?.Trim();

        if (string.IsNullOrEmpty(platformName))
        {
            yield break;
        }

        foreach (var platform in uvlConnect.PlatformHelper.GetPlatforms(platformName))
        {
            yield return platform;
        }
    }

    private void GetTags(IHtmlCollection<IElement> accordionItems)
    {
        var tagElements = GetAccordionItem(accordionItems, "Tags")?.QuerySelectorAll("span > a");

        if (tagElements is null)
        {
            return;
        }

        foreach (var element in tagElements)
        {
            if (!_tagDictionary.TryGetValue(element.GetAttribute("href"), out var tag))
            {
                continue;
            }

            // Skip game engine tags that are already added from the info card
            if (tag.Category == TagCategoryId.Software && _gameEngines.Any(e => e == tag.ShortName))
            {
                continue;
            }

            var fieldOverride = tag.Type == TagType.Series ? GameMetadata.Series : null;

            if (tag.Category == TagCategoryId.Culture && tag.Type == TagType.Concept && tag.Name.StartsWith("Rating:"))
            {
                fieldOverride = GameMetadata.AgeRatings;
                tag.ShortName = tag.ShortName.Replace("Rating:", "").Trim();
            }

            AddToField(tag.ShortName, settings.TagCategories[tag.Category], fieldOverride);
        }
    }

    private void GetTechnicalSpecs(IHtmlCollection<IElement> accordionItems)
    {
        var technicalSpecsSections = GetAccordionItem(accordionItems, "Technical specs")?.QuerySelectorAll(CssSelectors.BodySection);

        if (technicalSpecsSections is null || technicalSpecsSections.Length == 0)
        {
            return;
        }

        var category = settings.TagCategories[TagCategoryId.Display];

        foreach (var section in technicalSpecsSections)
        {
            var sectionContent = section.TextContent.Trim();

            if (!sectionContent.Contains(category.Caption, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var value in GetValues([.. section.QuerySelectorAll("a, b")], false))
            {
                AddToField(((MetadataNameProperty)value).Name, category);
            }
        }
    }

    private IEnumerable<Link> GetTopLinks(IHtmlCollection<IElement> elements)
    {
        foreach (var element in elements)
        {
            yield return new Link(Resources.LinkPairs.GetNameForString(element.TextContent.Trim()), element.GetAttribute("href"));
        }
    }

    private IEnumerable<MetadataProperty> GetValueAsList(IElement element, bool splitByComma = false)
    {
        if (element is null)
        {
            yield break;
        }

        if (splitByComma)
        {
            foreach (var part in element.TextContent.Split([","], StringSplitOptions.RemoveEmptyEntries))
            {
                yield return new MetadataNameProperty(part.Trim());
            }
        }
        else
        {
            yield return new MetadataNameProperty(element.TextContent.Trim());
        }
    }

    private IEnumerable<MetadataProperty> GetValues(List<IElement> elements, bool splitByComma = false)
    {
        foreach (var element in elements)
        {
            var value = element.TextContent.Trim();

            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (splitByComma)
            {
                foreach (var part in value.Split([","], StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return new MetadataNameProperty(part.Trim());
                }
            }
            else
            {
                yield return new MetadataNameProperty(value);
            }
        }
    }
}
