using LinkUtilities.Linker;
using Playnite;
using System.Collections.ObjectModel;

namespace LinkUtilities.Models;

public class LinkSourceSettings : ObservableCollection<LinkSourceSetting>
{
    public LinkSourceSettings(IEnumerable<LinkSourceSetting> items)
    {
        this.AddMissing(items);
    }

    public LinkSourceSettings()
    { }

    /// <summary>
    /// Gets a collection of the settings to all link sources in the plugin.
    /// </summary>
    /// <param name="links">Link sources to be added</param>
    /// <returns>Collection of the settings to all link sources in the plugin</returns>
    internal static LinkSourceSettings GetLinkSources(Links links)
    {
        var result = new LinkSourceSettings();

        if (links == null)
        {
            return result;
        }

        foreach (var link in links.Where(link => !link.Settings.IsCustomSource))
        {
            result.Add(link.Settings);
        }

        return result;
    }

    /// <summary>
    /// Refreshes a LinkSourceCollection with the actual link sources present in the plugin. Is
    /// needed after updates when link sources get added or had to be removed.
    /// </summary>
    /// <param name="links">Link sources to be added</param>
    internal void RefreshLinkSources(Links links)
    {
        var newSources = GetLinkSources(links);

        foreach (var item in this.Where(x1 => newSources.All(x2 => x2.Id != x1.Id)).ToList())
        {
            Remove(item);
        }

        foreach (var itemNew in newSources)
        {
            var itemOld = this.FirstOrDefault(x => x.Id == itemNew.Id);

            if (itemOld != null)
            {
                if (itemNew.IsAddable != null)
                {
                    itemNew.IsAddable = itemOld.IsAddable == true;
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
}