using KNARZhelper.WebCommon;
using LinkUtilities.Settings;
using LinkUtilities.ViewModels;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Models
{
    public class Pipeline : LinkWorker
    {
        public Pipeline(int id) : base(id)
        {
        }

        public List<CheckGameLink> CheckedLinks { get; set; } = new List<CheckGameLink>();
        public Game Game { get; set; }
        public List<Link> Links { get; set; } = new List<Link>();

        public void CheckLinks(bool hideOkOnLinkCheck)
        {
            CheckedLinks.Clear();

            if (Game == null || Links == null || Links.Count == 0)
            {
                return;
            }

            foreach (var link in Links)
            {
                var linkCheckResult = LoadUrl(link.Url, DocumentType.Empty, GlobalSettings.Instance().DebugMode, "", null, false);

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
}