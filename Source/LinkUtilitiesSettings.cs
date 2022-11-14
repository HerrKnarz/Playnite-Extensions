using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities
{
    /// <summary>
    /// Contains all settings for the extension
    /// </summary>
    public class LinkUtilitiesSettings : ObservableObject
    {
        private bool sortAfterChange = false;
        private LinkSourceSettings linkSettings;
        private LinkNamePatterns linkPatterns;
        private LinkNamePatterns removePatterns;

        /// <summary>
        /// sets whether the Links shall be sorted after a game is updated in the database
        /// </summary>
        public bool SortAfterChange { get => sortAfterChange; set => SetValue(ref sortAfterChange, value); }
        /// <summary>
        /// Collection with the settings of all link sources
        /// </summary>
        public LinkSourceSettings LinkSettings { get => linkSettings; set => SetValue(ref linkSettings, value); }

        public LinkNamePatterns LinkNamePatterns { get => linkPatterns; set => SetValue(ref linkPatterns, value); }

        public LinkNamePatterns RemovePatterns { get => removePatterns; set => SetValue(ref removePatterns, value); }

        public LinkUtilitiesSettings()
        {
            linkSettings = new LinkSourceSettings();
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

        public RelayCommand AddLinkNamePatternCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.LinkNamePatterns.Add(new LinkNamePattern());
            });
        }

        public RelayCommand AddDefaultLinkNamePatternsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern);
            });
        }

        public RelayCommand<IList<object>> RemoveLinkNamePatternsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (LinkNamePattern linkPattern in items.ToList().Cast<LinkNamePattern>())
                {
                    Settings.LinkNamePatterns.Remove(linkPattern);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand AddRemovePatternCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RemovePatterns.Add(new LinkNamePattern());
            });
        }

        public RelayCommand AddDefaultRemovePatternsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RemovePatterns.AddDefaultPatterns(PatternTypes.RemovePattern);
            });
        }

        public RelayCommand<IList<object>> RemoveRemovePatternsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (LinkNamePattern removePattern in items.ToList().Cast<LinkNamePattern>())
                {
                    Settings.RemovePatterns.Remove(removePattern);
                }
            }, (items) => items != null && items.Count > 0);
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
                Settings.LinkSettings.RefreshLinkSources(plugin.AddWebsiteLinks.Links);
                Settings.LinkSettings = new LinkSourceSettings(Settings.LinkSettings.OrderBy(x => x.LinkName).ToList());
            }
            else
            {
                Settings = new LinkUtilitiesSettings
                {
                    LinkSettings = LinkSourceSettings.GetLinkSources(plugin.AddWebsiteLinks.Links)
                };
            }

            if (Settings.LinkNamePatterns == null)
            {
                Settings.LinkNamePatterns = new LinkNamePatterns();
            }

            Settings.LinkNamePatterns = new LinkNamePatterns(Settings.LinkNamePatterns.OrderBy(x => x.LinkName).ToList());

            if (Settings.RemovePatterns == null)
            {
                Settings.RemovePatterns = new LinkNamePatterns();
            }

            Settings.RemovePatterns = new LinkNamePatterns(Settings.RemovePatterns.OrderBy(x => x.NamePattern).ThenBy(x => x.UrlPattern).ToList());
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings.SortAfterChange = EditingClone.SortAfterChange;
            Settings.LinkNamePatterns = EditingClone.LinkNamePatterns;
            Settings.RemovePatterns = EditingClone.RemovePatterns;

            foreach (LinkSourceSetting originalItem in Settings.LinkSettings)
            {
                LinkSourceSetting clonedItem = EditingClone.LinkSettings.FirstOrDefault(x => x.LinkName == originalItem.LinkName);

                if (clonedItem != null)
                {
                    if (originalItem.IsAddable != null)
                    {
                        originalItem.IsAddable = clonedItem.IsAddable;
                    }

                    if (originalItem.IsSearchable != null)
                    {
                        originalItem.IsSearchable = clonedItem.IsSearchable;
                    }

                    originalItem.ShowInMenus = clonedItem.ShowInMenus;
                    originalItem.ApiKey = clonedItem.ApiKey;
                }
            }
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(Settings);

            plugin.HandleUriActions.LinkNamePatterns = Settings.LinkNamePatterns;
            plugin.RemoveLinks.RemovePatterns = Settings.RemovePatterns;
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}