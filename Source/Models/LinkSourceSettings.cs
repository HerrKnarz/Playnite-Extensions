using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LinkUtilities.Models
{
    public class LinkSourceSetting
    {
        [JsonProperty("linkName")]
        public string LinkName { get; set; }
        [JsonProperty("isAddable")]
        public bool? IsAddable { get; set; }
        [JsonProperty("isSearchable")]
        public bool? IsSearchable { get; set; }
    }

    public class LinkSourceSettings : ObservableCollection<LinkSourceSetting>
    {
        public void PopulateLinkSources(LinkUtilities plugin)
        {
            if (plugin != null)
            {
                foreach (Linker.Link link in plugin.AddWebsiteLinks.Links)
                {
                    Add(new LinkSourceSetting
                    {
                        LinkName = link.LinkName,
                        IsAddable = link.IsAddable ? true : (bool?)null,
                        IsSearchable = link.IsSearchable ? true : (bool?)null
                    });
                }
            }
        }

    }
}
