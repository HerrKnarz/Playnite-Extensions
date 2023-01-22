using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
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
        private bool useCustomSortOrder = false;
        private bool removeDuplicatesAfterChange = false;
        private DuplicateTypes removeDuplicatesType = DuplicateTypes.NameAndUrl;
        private bool removeLinksAfterChange = false;
        private bool renameLinksAfterChange = false;
        private bool tagMissingLinksAfterChange = false;
        private string missingLinkPrefix = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");
        private ObservableCollection<SortItem> sortOrder;
        private LinkSourceSettings linkSettings;
        private LinkNamePatterns linkPatterns;
        private LinkNamePatterns removePatterns;
        private LinkNamePatterns renamePatterns;
        private LinkNamePatterns missingLinkPatterns;

        [DontSerialize]
        public DuplicateTypesWithCaptions DuplicateTypesWithCaptions { get; }

        public bool SortAfterChange { get => sortAfterChange; set => SetValue(ref sortAfterChange, value); }

        public bool UseCustomSortOrder { get => useCustomSortOrder; set => SetValue(ref useCustomSortOrder, value); }

        public bool RemoveDuplicatesAfterChange { get => removeDuplicatesAfterChange; set => SetValue(ref removeDuplicatesAfterChange, value); }

        public DuplicateTypes RemoveDuplicatesType { get => removeDuplicatesType; set => SetValue(ref removeDuplicatesType, value); }

        public bool RemoveLinksAfterChange { get => removeLinksAfterChange; set => SetValue(ref removeLinksAfterChange, value); }

        public bool RenameLinksAfterChange { get => renameLinksAfterChange; set => SetValue(ref renameLinksAfterChange, value); }

        public bool TagMissingLinksAfterChange { get => tagMissingLinksAfterChange; set => SetValue(ref tagMissingLinksAfterChange, value); }

        public string MissingLinkPrefix { get => missingLinkPrefix; set => SetValue(ref missingLinkPrefix, value); }

        public ObservableCollection<SortItem> SortOrder { get => sortOrder; set => SetValue(ref sortOrder, value); }

        public LinkSourceSettings LinkSettings { get => linkSettings; set => SetValue(ref linkSettings, value); }

        public LinkNamePatterns LinkNamePatterns { get => linkPatterns; set => SetValue(ref linkPatterns, value); }

        public LinkNamePatterns RemovePatterns { get => removePatterns; set => SetValue(ref removePatterns, value); }

        public LinkNamePatterns RenamePatterns { get => renamePatterns; set => SetValue(ref renamePatterns, value); }
        public LinkNamePatterns MissingLinkPatterns { get => missingLinkPatterns; set => SetValue(ref missingLinkPatterns, value); }

        public LinkUtilitiesSettings()
        {
            linkSettings = new LinkSourceSettings();
            DuplicateTypesWithCaptions = new DuplicateTypesWithCaptions();
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

        public RelayCommand AddSortItemCommand
        {
            get => new RelayCommand(() =>
            {
                int position = 1;

                if (Settings.SortOrder.Count > 0)
                {
                    position = Settings.SortOrder.Max(x => x.Position) + 1;
                }

                Settings.SortOrder.Add(new SortItem()
                {
                    LinkName = "",
                    Position = position
                });
            });
        }

        public RelayCommand<IList<object>> RemoveSortItemsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (SortItem item in items.ToList().Cast<SortItem>())
                {
                    Settings.SortOrder.Remove(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand SortSortItemsCommand
        {
            get => new RelayCommand(() =>
            {
                SortSortItems();
            });
        }

        public RelayCommand SortBookmarkletItemsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.LinkNamePatterns.SortPatterns();
            });
        }

        public RelayCommand SortRemoveItemsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RemovePatterns.SortPatterns();
            });
        }

        public RelayCommand SortRenameItemsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.RenamePatterns.SortPatterns();
            });
        }

        public RelayCommand SortMissingLinkItemsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.MissingLinkPatterns.SortPatterns();
            });
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

        public RelayCommand AddMissingLinkPatternCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.MissingLinkPatterns.Add(new LinkNamePattern());
            });
        }

        public RelayCommand AddDefaultMissingLinkPatternsCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.MissingLinkPatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern);
            });
        }

        public RelayCommand<IList<object>> RemoveMissingLinkPatternsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (LinkNamePattern missingLinkPattern in items.ToList().Cast<LinkNamePattern>())
                {
                    Settings.MissingLinkPatterns.Remove(missingLinkPattern);
                }
            }, (items) => items != null && items.Count > 0);
        }

        private void SortSortItems()
        {
            Settings.SortOrder = new ObservableCollection<SortItem>(Settings.SortOrder
                .OrderBy(x => x.Position)
                .ThenBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
        }

        public void WriteSettingsToLinkActions()
        {
            plugin.SortLinks.SortOrder = Settings.SortOrder.ToDictionary(x => x.LinkName, x => x.Position);
            plugin.HandleUriActions.LinkNamePatterns = Settings.LinkNamePatterns;
            plugin.RemoveLinks.RemovePatterns = Settings.RemovePatterns;
            plugin.RenameLinks.RenamePatterns = Settings.RenamePatterns;
            plugin.TagMissingLinks.MissingLinkPatterns = Settings.MissingLinkPatterns;
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

            if (Settings.SortOrder == null)
            {
                Settings.SortOrder = new ObservableCollection<SortItem>();
            }
            else
            {
                SortSortItems();
            }

            if (Settings.LinkNamePatterns == null)
            {
                Settings.LinkNamePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.LinkNamePatterns.SortPatterns();
            }

            if (Settings.RemovePatterns == null)
            {
                Settings.RemovePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.RemovePatterns.SortPatterns();
            }

            if (Settings.RenamePatterns == null)
            {
                Settings.RenamePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.RenamePatterns.SortPatterns();
            }

            if (Settings.MissingLinkPatterns == null)
            {
                Settings.MissingLinkPatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.MissingLinkPatterns.SortPatterns();
            }
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings.SortAfterChange = EditingClone.SortAfterChange;
            Settings.UseCustomSortOrder = EditingClone.UseCustomSortOrder;
            Settings.RemoveDuplicatesAfterChange = EditingClone.RemoveDuplicatesAfterChange;
            Settings.RemoveDuplicatesType = EditingClone.RemoveDuplicatesType;
            Settings.RemoveLinksAfterChange = EditingClone.RemoveLinksAfterChange;
            Settings.RenameLinksAfterChange = EditingClone.RenameLinksAfterChange;
            Settings.TagMissingLinksAfterChange = EditingClone.TagMissingLinksAfterChange;
            Settings.MissingLinkPrefix = EditingClone.MissingLinkPrefix;
            Settings.SortOrder = EditingClone.SortOrder;
            Settings.LinkNamePatterns = EditingClone.LinkNamePatterns;
            Settings.RemovePatterns = EditingClone.RemovePatterns;
            Settings.RenamePatterns = EditingClone.RenamePatterns;
            Settings.MissingLinkPatterns = EditingClone.MissingLinkPatterns;

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
            Settings.RenamePatterns.SortPatterns();

            plugin.SavePluginSettings(Settings);

            WriteSettingsToLinkActions();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            HashSet<string> hashset = new HashSet<string>();
            foreach (SortItem item in Settings.SortOrder)
            {
                if (!hashset.Add(item.LinkName))
                {
                    errors.Add(ResourceProvider.GetString("LOCLinkUtilitiesErrorDuplicates"));
                    return false;
                }
            }

            return true;
        }
    }
}