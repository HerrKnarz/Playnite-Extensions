using LinkUtilities.Linker;
using LinkUtilities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.Settings
{
    public class LinkSourceSettings : ObservableCollection<LinkSourceSetting>
    {
        public LinkSourceSettings(List<LinkSourceSetting> items)
        {
            this.AddMissing(items);
        }

        public LinkSourceSettings()
        {
        }

        /// <summary>
        /// Refreshes a LinkSourceCollecion with the actual link sources present in the plugin. Is needed after updates when
        /// link sources get added or had to be removed.
        /// </summary>
        /// <param name="links">Link sources to be added</param>
        public void RefreshLinkSources(Links links)
        {
            LinkSourceSettings newSources = GetLinkSources(links);

            foreach (LinkSourceSetting item in this.Where(x1 => newSources.All(x2 => x2.LinkName != x1.LinkName)).ToList())
            {
                Remove(item);
            }

            foreach (LinkSourceSetting itemNew in newSources)
            {
                LinkSourceSetting itemOld = this.FirstOrDefault(x => x.LinkName == itemNew.LinkName);

                if (itemOld != null)
                {
                    if (itemNew.IsAddable != null)
                    {
                        itemNew.IsAddable = itemOld.IsAddable;
                    }

                    if (itemNew.IsSearchable != null)
                    {
                        itemNew.IsSearchable = itemOld.IsSearchable;
                    }

                    itemNew.ShowInMenus = itemOld.ShowInMenus;
                    itemNew.ApiKey = itemOld.ApiKey;

                    Remove(itemOld);
                }
                Add(itemNew);
            }
        }

        /// <summary>
        /// Gets a collection of the settings to all link sources in the plugin.
        /// </summary>
        /// <param name="links">Link sources to be added</param>
        /// <returns>Collection of the settings to all link sources in the plugin</returns>
        public static LinkSourceSettings GetLinkSources(Links links)
        {
            LinkSourceSettings result = new LinkSourceSettings();

            if (links != null)
            {
                foreach (Link link in links)
                {
                    result.Add(link.Settings);
                }
            }
            return result;
        }
    }
}
