using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities
{
    /// <summary>
    /// Contains all settings for the extension
    /// </summary>
    public class LinkUtilitiesSettings : ObservableObject
    {
        private bool sortAfterChange = false;
        private ObservableCollection<LinkSourceSettings> linkSettings;

        /// <summary>
        /// sets whether the Links shall be sorted after a game is updated in the database
        /// </summary>
        public bool SortAfterChange { get => sortAfterChange; set => SetValue(ref sortAfterChange, value); }
        /// <summary>
        /// Collection with the settings of all link sources
        /// </summary>
        public ObservableCollection<LinkSourceSettings> LinkSettings { get => linkSettings; set => SetValue(ref linkSettings, value); }

        public LinkUtilitiesSettings()
        {
            linkSettings = new ObservableCollection<LinkSourceSettings>();
        }

        /// <summary>
        /// Refreshes a LinkSourceCollecion with the actual link sources present in the plugin. Is needed after updates when
        /// link sources get added or had to be removed.
        /// </summary>
        /// <param name="links">Link sources to be added</param>
        public void RefreshLinkSources(Links links)
        {
            ObservableCollection<LinkSourceSettings> newSources = GetLinkSources(links);

            foreach (LinkSourceSettings item in linkSettings.Where(x1 => newSources.All(x2 => x2.LinkName != x1.LinkName)).ToList())
            {
                LinkSettings.Remove(item);
            }

            foreach (LinkSourceSettings itemNew in newSources)
            {
                LinkSourceSettings itemOld = linkSettings.FirstOrDefault(x => x.LinkName == itemNew.LinkName);

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
                    itemNew.NeedsApiKey = itemOld.NeedsApiKey;

                    linkSettings.Remove(itemOld);
                }
                linkSettings.Add(itemNew);
            }

            LinkSettings = new ObservableCollection<LinkSourceSettings>(LinkSettings.OrderBy(x => x.LinkName));
        }
        /// <summary>
        /// Gets a collection of the settings to all link sources in the plugin.
        /// </summary>
        /// <param name="links">Link sources to be added</param>
        /// <returns>Collection of the settings to all link sources in the plugin</returns>
        public ObservableCollection<LinkSourceSettings> GetLinkSources(Links links)
        {
            ObservableCollection<LinkSourceSettings> result = new ObservableCollection<LinkSourceSettings>();

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

    public class LinkUtilitiesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkUtilities plugin;
        private LinkUtilitiesSettings EditingClone { get; set; }

        private LinkUtilitiesSettings settings;
        public LinkUtilitiesSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public LinkUtilitiesSettingsViewModel(LinkUtilities plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            LinkUtilitiesSettings savedSettings = plugin.LoadPluginSettings<LinkUtilitiesSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
                Settings.RefreshLinkSources(plugin.AddWebsiteLinks.Links);
            }
            else
            {
                Settings = new LinkUtilitiesSettings();
                Settings.LinkSettings = Settings.GetLinkSources(plugin.AddWebsiteLinks.Links);
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}