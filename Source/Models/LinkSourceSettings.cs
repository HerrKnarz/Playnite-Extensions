using System.Collections.Generic;

namespace LinkUtilities.Models
{
    public class LinkSourceSetting
    {
        /// <summary>
        /// Name of the link source - corresponds to the LinkName property of Link objects.
        /// </summary>
        public string LinkName { get; set; }
        public bool? IsAddable { get; set; }
        public bool? IsSearchable { get; set; }
    }

    public class LinkSourceSettings : Dictionary<string, LinkSourceSetting>
    {
        public LinkSourceSettings(LinkUtilities plugin)
        {
            foreach (Linker.Link link in plugin.AddWebsiteLinks.Links)
            {
                Add(link.LinkName, new LinkSourceSetting
                {
                    LinkName = link.LinkName,
                    IsAddable = link.IsAddable ? true : (bool?)null,
                    IsSearchable = link.IsSearchable ? true : (bool?)null
                });
            }
        }
    }
}
