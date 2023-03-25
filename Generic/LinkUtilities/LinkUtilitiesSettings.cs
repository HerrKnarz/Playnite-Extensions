using LinkUtilities.LinkActions;
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
        private bool _sortAfterChange = false;
        private bool _addLinksToNewGames = false;
        private bool _useCustomSortOrder = false;
        private bool _removeDuplicatesAfterChange = false;
        private DuplicateTypes _removeDuplicatesType = DuplicateTypes.NameAndUrl;
        private bool _removeLinksAfterChange = false;
        private bool _renameLinksAfterChange = false;
        private bool _tagMissingLinksAfterChange = false;
        private string _missingLinkPrefix = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private ObservableCollection<SortItem> _sortOrder;
        private LinkSourceSettings _linkSettings;
        private LinkNamePatterns _linkPatterns;
        private LinkNamePatterns _removePatterns;
        private LinkNamePatterns _renamePatterns;
        private LinkNamePatterns _missingLinkPatterns;

        [DontSerialize]
        public DuplicateTypesWithCaptions DuplicateTypesWithCaptions { get; }

        public bool SortAfterChange { get => _sortAfterChange; set => SetValue(ref _sortAfterChange, value); }

        public bool AddLinksToNewGames { get => _addLinksToNewGames; set => SetValue(ref _addLinksToNewGames, value); }

        public bool UseCustomSortOrder { get => _useCustomSortOrder; set => SetValue(ref _useCustomSortOrder, value); }

        public bool RemoveDuplicatesAfterChange { get => _removeDuplicatesAfterChange; set => SetValue(ref _removeDuplicatesAfterChange, value); }

        public DuplicateTypes RemoveDuplicatesType { get => _removeDuplicatesType; set => SetValue(ref _removeDuplicatesType, value); }

        public bool RemoveLinksAfterChange { get => _removeLinksAfterChange; set => SetValue(ref _removeLinksAfterChange, value); }

        public bool RenameLinksAfterChange { get => _renameLinksAfterChange; set => SetValue(ref _renameLinksAfterChange, value); }

        public bool TagMissingLinksAfterChange { get => _tagMissingLinksAfterChange; set => SetValue(ref _tagMissingLinksAfterChange, value); }

        public string MissingLinkPrefix { get => _missingLinkPrefix; set => SetValue(ref _missingLinkPrefix, value); }

        public DateTime LastAutoLibUpdate { get => _lastAutoLibUpdate; set => SetValue(ref _lastAutoLibUpdate, value); }

        public ObservableCollection<SortItem> SortOrder { get => _sortOrder; set => SetValue(ref _sortOrder, value); }

        public LinkSourceSettings LinkSettings { get => _linkSettings; set => SetValue(ref _linkSettings, value); }

        public LinkNamePatterns LinkNamePatterns { get => _linkPatterns; set => SetValue(ref _linkPatterns, value); }

        public LinkNamePatterns RemovePatterns { get => _removePatterns; set => SetValue(ref _removePatterns, value); }

        public LinkNamePatterns RenamePatterns { get => _renamePatterns; set => SetValue(ref _renamePatterns, value); }

        public LinkNamePatterns MissingLinkPatterns { get => _missingLinkPatterns; set => SetValue(ref _missingLinkPatterns, value); }

        public LinkUtilitiesSettings()
        {
            _linkSettings = new LinkSourceSettings();
            DuplicateTypesWithCaptions = new DuplicateTypesWithCaptions();
        }
    }

    public class LinkUtilitiesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkUtilities _plugin;
        private LinkUtilitiesSettings EditingClone { get; set; }

        private LinkUtilitiesSettings _settings;
        public LinkUtilitiesSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
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

        public RelayCommand SortSortItemsCommand => new RelayCommand(() => SortSortItems());

        public RelayCommand SortBookmarkletItemsCommand => new RelayCommand(() => Settings.LinkNamePatterns.SortPatterns());

        public RelayCommand SortRemoveItemsCommand => new RelayCommand(() => Settings.RemovePatterns.SortPatterns());

        public RelayCommand SortRenameItemsCommand => new RelayCommand(() => Settings.RenamePatterns.SortPatterns());

        public RelayCommand SortMissingLinkItemsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.SortPatterns());

        public RelayCommand AddLinkNamePatternCommand => new RelayCommand(() => Settings.LinkNamePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultLinkNamePatternsCommand => new RelayCommand(() => Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern));

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

        public RelayCommand AddRemovePatternCommand => new RelayCommand(() => Settings.RemovePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultRemovePatternsCommand => new RelayCommand(() => Settings.RemovePatterns.AddDefaultPatterns(PatternTypes.RemovePattern));

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

        public RelayCommand AddRenamePatternCommand => new RelayCommand(() => Settings.RenamePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultRenamePatternsCommand => new RelayCommand(() => Settings.RenamePatterns.AddDefaultPatterns(PatternTypes.RenamePattern));

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

        public RelayCommand AddMissingLinkPatternCommand => new RelayCommand(() => Settings.MissingLinkPatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultMissingLinkPatternsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.AddDefaultPatterns(PatternTypes.MissingLinkPatterns));

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
            SortLinks.GetInstance(_plugin).SortOrder = Settings.SortOrder.ToDictionary(x => x.LinkName, x => x.Position);
            HandleUriActions.GetInstance(_plugin).LinkNamePatterns = Settings.LinkNamePatterns;
            RemoveLinks.GetInstance(_plugin).RemovePatterns = Settings.RemovePatterns;
            RenameLinks.GetInstance(_plugin).RenamePatterns = Settings.RenamePatterns;
            TagMissingLinks.GetInstance(_plugin).MissingLinkPatterns = Settings.MissingLinkPatterns;
        }

        public LinkUtilitiesSettingsViewModel(LinkUtilities plugin)
        {
            _plugin = plugin;

            LinkUtilitiesSettings savedSettings = plugin.LoadPluginSettings<LinkUtilitiesSettings>();

            if (savedSettings != null)
            {
                Settings = savedSettings;
                Settings.LinkSettings.RefreshLinkSources(AddWebsiteLinks.GetInstance(plugin).Links);
                Settings.LinkSettings = new LinkSourceSettings(Settings.LinkSettings.OrderBy(x => x.LinkName).ToList());
            }
            else
            {
                Settings = new LinkUtilitiesSettings
                {
                    LinkSettings = LinkSourceSettings.GetLinkSources(AddWebsiteLinks.GetInstance(plugin).Links)
                };
            }

            if (Settings.SortOrder is null)
            {
                Settings.SortOrder = new ObservableCollection<SortItem>();
            }
            else
            {
                SortSortItems();
            }

            if (Settings.LinkNamePatterns is null)
            {
                Settings.LinkNamePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.LinkNamePatterns.SortPatterns();
            }

            if (Settings.RemovePatterns is null)
            {
                Settings.RemovePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.RemovePatterns.SortPatterns();
            }

            if (Settings.RenamePatterns is null)
            {
                Settings.RenamePatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.RenamePatterns.SortPatterns();
            }

            if (Settings.MissingLinkPatterns is null)
            {
                Settings.MissingLinkPatterns = new LinkNamePatterns();
            }
            else
            {
                Settings.MissingLinkPatterns.SortPatterns();
            }
        }

        public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit()
        {
            Settings.SortAfterChange = EditingClone.SortAfterChange;
            Settings.AddLinksToNewGames = EditingClone.AddLinksToNewGames;
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

            _plugin.SavePluginSettings(Settings);

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