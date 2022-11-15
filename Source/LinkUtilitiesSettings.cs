using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
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
        private bool removeLinksAfterChange = false;
        private bool renameLinksAfterChange = false;
        private LinkSourceSettings linkSettings;
        private LinkNamePatterns linkPatterns;
        private LinkNamePatterns removePatterns;
        private LinkNamePatterns renamePatterns;

        public bool SortAfterChange { get => sortAfterChange; set => SetValue(ref sortAfterChange, value); }

        public bool RemoveLinksAfterChange { get => removeLinksAfterChange; set => SetValue(ref removeLinksAfterChange, value); }

        public bool RenameLinksAfterChange { get => renameLinksAfterChange; set => SetValue(ref renameLinksAfterChange, value); }

        public LinkSourceSettings LinkSettings { get => linkSettings; set => SetValue(ref linkSettings, value); }

        public LinkNamePatterns LinkNamePatterns { get => linkPatterns; set => SetValue(ref linkPatterns, value); }

        public LinkNamePatterns RemovePatterns { get => removePatterns; set => SetValue(ref removePatterns, value); }

        public LinkNamePatterns RenamePatterns { get => renamePatterns; set => SetValue(ref renamePatterns, value); }

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

        public RelayCommand AddRenamePatternCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RenamePatterns.Add(new LinkNamePattern());
            });
        }

        public RelayCommand AddDefaultRenamePatternsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RenamePatterns.AddDefaultPatterns(PatternTypes.RenamePattern);
            });
        }

        public RelayCommand<IList<object>> RemoveRenamePatternsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (LinkNamePattern renamePattern in items.ToList().Cast<LinkNamePattern>())
                {
                    Settings.RenamePatterns.Remove(renamePattern);
                }
            }, (items) => items != null && items.Count > 0);
        }

        private void PreparePatterns(LinkNamePatterns patterns)
        {
            patterns = new LinkNamePatterns(patterns
                .OrderBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
        }

        public LinkUtilitiesSettingsViewModel(LinkUtilities plugin)
        {
            this.plugin = plugin;

            LinkUtilitiesSettings savedSettings = plugin.LoadPluginSettings<LinkUtilitiesSettings>();

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

            if (Settings.RemovePatterns == null)
            {
                Settings.RemovePatterns = new LinkNamePatterns();
            }

            if (Settings.RenamePatterns == null)
            {
                Settings.RenamePatterns = new LinkNamePatterns();
            }

            PreparePatterns(Settings.LinkNamePatterns);
            PreparePatterns(Settings.RemovePatterns);
            PreparePatterns(Settings.RenamePatterns);
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings.SortAfterChange = EditingClone.SortAfterChange;
            Settings.RemoveLinksAfterChange = EditingClone.RemoveLinksAfterChange;
            Settings.RenameLinksAfterChange = EditingClone.RenameLinksAfterChange;
            Settings.LinkNamePatterns = EditingClone.LinkNamePatterns;
            Settings.RemovePatterns = EditingClone.RemovePatterns;
            Settings.RenamePatterns = EditingClone.RenamePatterns;

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
            plugin.RenameLinks.RenamePatterns = Settings.RenamePatterns;
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}