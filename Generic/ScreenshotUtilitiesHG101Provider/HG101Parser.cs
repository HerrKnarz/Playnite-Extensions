using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ScreenshotUtilitiesHG101Provider
{
    public class HG101Parser
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private readonly string _searchUrl = "http://www.hardcoregaming101.net/?s=";

        public string GetScreenshotSearchResult(string searchTerm)
        {
            try
            {
                var document = AsyncHelper.RunSync(async () => await _context.OpenAsync($"{_searchUrl}{searchTerm.UrlEncode()}"));

                if (document.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var cells = document.QuerySelectorAll("div.post header.entry-header");

                if (cells == null || !cells.Any())
                {
                    return null;
                }

                var searchResults = new List<ScreenshotSearchResult>();

                foreach (var node in cells.Where(n => n.QuerySelector("a.Review") != null))
                {
                    var result = new ScreenshotSearchResult
                    {
                        Name = WebUtility.HtmlDecode(node.QuerySelector("h2 a")?.TextContent),
                        Description = WebUtility.HtmlDecode(node.QuerySelector(".entry-excerpt p")?.TextContent),
                        Identifier = ((IHtmlAnchorElement)node.QuerySelector("h2 a"))?.Href
                    };

                    if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.Identifier))
                    {
                        continue;
                    }

                    searchResults.Add(result);
                }

                return Serialization.ToJson(searchResults);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return null;
        }

        public async Task<bool> LoadScreenshotsAsync(ScreenshotGroup screenshotGroup)
        {
            var url = screenshotGroup.GameIdentifier;

            var updated = false;

            try
            {
                var document = await _context.OpenAsync(url);

                if (document.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                // Remove related articles section to not fetch its screenshots as well.
                document.QuerySelector(".yarpp")?.Remove();

                updated = GetSideGallery(document, screenshotGroup);
                updated |= GetCoverGallery(document, screenshotGroup);
                updated |= GetCharacterGallery(document, screenshotGroup);
                updated |= GetAttachments(document, screenshotGroup);

                updated |= GetSingleImages(document, screenshotGroup);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return updated;
        }

        private void AddToGroup(ScreenshotGroup screenshotGroup, Screenshot screenshotToAdd, bool refreshOld = true)
        {
            var pathToScreenshot = new Uri(screenshotToAdd.Path).LocalPath;

            var existingScreenshot = screenshotGroup.Screenshots.FirstOrDefault(es => new Uri(es.Path).LocalPath.Equals(pathToScreenshot));

            if (existingScreenshot == default)
            {
                screenshotToAdd.SortOrder = (screenshotGroup.Screenshots?.Count ?? 0) + 1;

                screenshotGroup.Screenshots.Add(screenshotToAdd);
            }
            else if (refreshOld)
            {
                existingScreenshot.Name = screenshotToAdd.Name;
            }
        }

        private bool GetAttachments(IDocument document, ScreenshotGroup screenshotGroup)
        {
            var cells = document.QuerySelectorAll("div.entry-content div")?.Where(d => d.Id?.StartsWith("attachment_") ?? false);

            if (cells == null || (!cells.Any()))
            {
                return false;
            }

            foreach (var node in cells)
            {
                ProcessNode(screenshotGroup,
                    ((IHtmlAnchorElement)node.QuerySelector("a"))?.Href,
                    node.QuerySelector(".wp-caption-text")?.TextContent.Trim(),
                    (IHtmlImageElement)node.QuerySelector("img"),
                    MediaType.Artwork);
            }

            return true;
        }

        private bool GetCharacterGallery(IDocument document, ScreenshotGroup screenshotGroup)
        {
            var cells = document.QuerySelectorAll("div.character");

            if (cells == null || (!cells.Any()))
            {
                return false;
            }

            foreach (var node in cells)
            {
                ProcessNode(screenshotGroup,
                    ((IHtmlAnchorElement)node.QuerySelector("a"))?.Href,
                    node.QuerySelector(".charactername")?.TextContent.Trim(),
                    (IHtmlImageElement)node.QuerySelector("img"),
                    MediaType.Artwork);
            }

            return true;
        }

        private bool GetCoverGallery(IDocument document, ScreenshotGroup screenshotGroup)
        {
            var cells = document.QuerySelectorAll("figure.gallery-item");

            if (cells == null || (!cells.Any()))
            {
                return false;
            }

            foreach (var node in cells)
            {
                var sources = SourceSet.Parse(((IHtmlImageElement)node.QuerySelector("img")).SourceSet);

                var imagePath = sources?.OrderByDescending(s => GetWidth(s.Descriptor)).FirstOrDefault().Url;

                if (imagePath == null)
                {
                    continue;
                }

                var name = node.QuerySelector("figcaption")?.TextContent.Trim();
                var mediaType = MediaType.Artwork;

                if (name != null && name.Contains("cover"))
                {
                    mediaType = MediaType.BoxFront;
                }

                ProcessNode(screenshotGroup,
                    imagePath,
                    name,
                    (IHtmlImageElement)node.QuerySelector("img"),
                    mediaType);
            }

            return true;
        }

        private bool GetSideGallery(IDocument document, ScreenshotGroup screenshotGroup)
        {
            var cells = document.QuerySelectorAll("div.left-sidebar-gallery a").OfType<IHtmlAnchorElement>();

            if (cells == null || (!cells.Any()))
            {
                return false;
            }

            foreach (var node in cells)
            {
                ProcessNode(screenshotGroup,
                    node.Href,
                    null,
                    (IHtmlImageElement)node.QuerySelector("img"),
                    MediaType.Screenshot);
            }

            return true;
        }

        private bool GetSingleImages(IDocument document, ScreenshotGroup screenshotGroup)
        {
            var cells = document.QuerySelectorAll("div.entry-content a")?.Where(d => d.QuerySelectorAll("img")?.Any() ?? false).OfType<IHtmlAnchorElement>();

            if (cells == null || (!cells.Any()))
            {
                return false;
            }

            foreach (var node in cells)
            {
                ProcessNode(screenshotGroup,
                    node.Href,
                    null,
                    (IHtmlImageElement)node.QuerySelector("img"),
                    MediaType.Unknown,
                    false);
            }

            return true;
        }

        private int GetWidth(string descriptor) => Int32.TryParse(descriptor?.Replace("w", ""), out var result) ? result : 0;

        private void ProcessNode(ScreenshotGroup screenshotGroup, string path, string name, IHtmlImageElement thumbNode, MediaType mediaType, bool refreshOld = true)
        {
            if (path == null || path.EndsWith("/"))
            {
                return;
            }

            var screenshot = new Screenshot(path, name);

            if (string.IsNullOrEmpty(screenshot.Name))
            {
                screenshot.Name = new Uri(screenshot.Path)?.Segments.Last();
            }

            if (thumbNode != null)
            {
                screenshot.ThumbnailPath = thumbNode.Source;

                if (string.IsNullOrEmpty(screenshot.ThumbnailPath))
                {
                    screenshot.ThumbnailPath = screenshot.Path;
                }
            }

            screenshot.Type = mediaType;

            AddToGroup(screenshotGroup, screenshot, refreshOld);
        }
    }
}
