using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.WebCommon;
using System.Net;

namespace LinkUtilities.Models;

public class Pipeline(int id) : WebWorker(id, LinkUtilitiesPlugin.PlayniteApi, LinkUtilitiesPlugin.Settings.DebugMode)
{
    public List<CheckGameLink> CheckedLinks { get; set; } = [];
    public Game? Game { get; set; }
    public List<WebLink> Links { get; set; } = [];

    public async Task CheckLinksAsync(bool hideOkOnLinkCheck)
    {
        CheckedLinks.Clear();

        if (Game is null || !Links.HasItems())
        {
            return;
        }

        foreach (var link in Links)
        {
            if (link.Url.IsNullOrEmpty())
            {
                continue;
            }

            var loadUrlArgs = new LoadUrlArgs
            {
                Url = link.Url,
                DocumentType = DocumentType.Empty,
                WaitForCallback = false
            };

            var linkCheckResult = await LoadUrlAsync(loadUrlArgs);

            if (!hideOkOnLinkCheck || linkCheckResult.StatusCode != HttpStatusCode.OK)
            {
                var checkLink = new CheckGameLink
                {
                    Game = Game,
                    Link = link,
                    LinkCheckResult = linkCheckResult,
                    UrlIsEqual = WebHelper.CleanUpUrl(linkCheckResult.ResponseUrl) == WebHelper.CleanUpUrl(link.Url)
                };

                CheckedLinks.Add(checkLink);
            }
        }
    }
}