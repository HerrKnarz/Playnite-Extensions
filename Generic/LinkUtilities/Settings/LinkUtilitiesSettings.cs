using LinkUtilities.LinkActions;
using LinkUtilities.Linker;
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
    ///     Contains all settings for the extension
    /// </summary>
    public class LinkUtilitiesSettings : ObservableObject
    {
        private bool _addLinksToNewGames;
        private bool _addSteamAchievementLink;
        private bool _addSteamCommunityLink;
        private bool _addSteamDiscussionLink;
        private bool _addSteamGuidesLink;
        private bool _addSteamNewsLink;
        private bool _addSteamStorePageLink;
        private bool _addSteamWorkshopLink;
        private bool _changeSteamLinksAfterChange;
        private bool _hideOkOnLinkCheck;
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private LinkNamePatterns _linkPatterns;
        private LinkSourceSettings _linkSettings;
        private LinkNamePatterns _missingLinkPatterns;
        private string _missingLinkPrefix = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");
        private string _nameSteamAchievementLink = "Achievements";
        private string _nameSteamCommunityLink = "Community Hub";
        private string _nameSteamDiscussionLink = "Discussion";
        private string _nameSteamGuidesLink = "Guides";
        private string _nameSteamNewsLink = "News";
        private string _nameSteamStorePageLink = "Store Page";
        private string _nameSteamWorkshopLink = "Workshop";
        private bool _removeDuplicatesAfterChange;
        private DuplicateTypes _removeDuplicatesType = DuplicateTypes.NameAndUrl;
        private bool _removeLinksAfterChange;
        private LinkNamePatterns _removePatterns;
        private string _renameBlocker = string.Empty;
        private bool _renameLinksAfterChange;
        private LinkNamePatterns _renamePatterns;
        private bool _sortAfterChange;
        private ObservableCollection<SortItem> _sortOrder;
        private bool _tagMissingLibraryLinks;
        private bool _tagMissingLinksAfterChange;
        private bool _useCustomSortOrder;
        private bool _useSteamAppLinks;


        public LinkUtilitiesSettings()
        {
            _linkSettings = new LinkSourceSettings();
            DuplicateTypesWithCaptions = new DuplicateTypesWithCaptions();
        }

        [DontSerialize]
        public DuplicateTypesWithCaptions DuplicateTypesWithCaptions { get; }

        public bool SortAfterChange
        {
            get => _sortAfterChange;
            set => SetValue(ref _sortAfterChange, value);
        }

        public bool AddLinksToNewGames
        {
            get => _addLinksToNewGames;
            set => SetValue(ref _addLinksToNewGames, value);
        }

        public bool UseSteamAppLinks
        {
            get => _useSteamAppLinks;
            set => SetValue(ref _useSteamAppLinks, value);
        }

        public bool AddSteamAchievementLink
        {
            get => _addSteamAchievementLink;
            set => SetValue(ref _addSteamAchievementLink, value);
        }

        public bool AddSteamCommunityLink
        {
            get => _addSteamCommunityLink;
            set => SetValue(ref _addSteamCommunityLink, value);
        }

        public bool AddSteamDiscussionLink
        {
            get => _addSteamDiscussionLink;
            set => SetValue(ref _addSteamDiscussionLink, value);
        }

        public bool AddSteamGuidesLink
        {
            get => _addSteamGuidesLink;
            set => SetValue(ref _addSteamGuidesLink, value);
        }

        public bool AddSteamNewsLink
        {
            get => _addSteamNewsLink;
            set => SetValue(ref _addSteamNewsLink, value);
        }

        public bool AddSteamStorePageLink
        {
            get => _addSteamStorePageLink;
            set => SetValue(ref _addSteamStorePageLink, value);
        }

        public bool AddSteamWorkshopLink
        {
            get => _addSteamWorkshopLink;
            set => SetValue(ref _addSteamWorkshopLink, value);
        }

        public bool ChangeSteamLinksAfterChange
        {
            get => _changeSteamLinksAfterChange;
            set => SetValue(ref _changeSteamLinksAfterChange, value);
        }

        public string NameSteamAchievementLink
        {
            get => _nameSteamAchievementLink;
            set => SetValue(ref _nameSteamAchievementLink, value);
        }

        public string NameSteamCommunityLink
        {
            get => _nameSteamCommunityLink;
            set => SetValue(ref _nameSteamCommunityLink, value);
        }

        public string NameSteamDiscussionLink
        {
            get => _nameSteamDiscussionLink;
            set => SetValue(ref _nameSteamDiscussionLink, value);
        }

        public string NameSteamGuidesLink
        {
            get => _nameSteamGuidesLink;
            set => SetValue(ref _nameSteamGuidesLink, value);
        }

        public string NameSteamNewsLink
        {
            get => _nameSteamNewsLink;
            set => SetValue(ref _nameSteamNewsLink, value);
        }

        public string NameSteamStorePageLink
        {
            get => _nameSteamStorePageLink;
            set => SetValue(ref _nameSteamStorePageLink, value);
        }

        public string NameSteamWorkshopLink
        {
            get => _nameSteamWorkshopLink;
            set => SetValue(ref _nameSteamWorkshopLink, value);
        }

        public bool UseCustomSortOrder
        {
            get => _useCustomSortOrder;
            set => SetValue(ref _useCustomSortOrder, value);
        }

        public bool RemoveDuplicatesAfterChange
        {
            get => _removeDuplicatesAfterChange;
            set => SetValue(ref _removeDuplicatesAfterChange, value);
        }

        public DuplicateTypes RemoveDuplicatesType
        {
            get => _removeDuplicatesType;
            set => SetValue(ref _removeDuplicatesType, value);
        }

        public bool RemoveLinksAfterChange
        {
            get => _removeLinksAfterChange;
            set => SetValue(ref _removeLinksAfterChange, value);
        }

        public bool RenameLinksAfterChange
        {
            get => _renameLinksAfterChange;
            set => SetValue(ref _renameLinksAfterChange, value);
        }

        public bool TagMissingLinksAfterChange
        {
            get => _tagMissingLinksAfterChange;
            set => SetValue(ref _tagMissingLinksAfterChange, value);
        }

        public bool TagMissingLibraryLinks
        {
            get => _tagMissingLibraryLinks;
            set => SetValue(ref _tagMissingLibraryLinks, value);
        }

        public bool HideOkOnLinkCheck
        {
            get => _hideOkOnLinkCheck;
            set => SetValue(ref _hideOkOnLinkCheck, value);
        }

        public string RenameBlocker
        {
            get => _renameBlocker;
            set => SetValue(ref _renameBlocker, value);
        }

        public string MissingLinkPrefix
        {
            get => _missingLinkPrefix;
            set => SetValue(ref _missingLinkPrefix, value);
        }

        public DateTime LastAutoLibUpdate
        {
            get => _lastAutoLibUpdate;
            set => SetValue(ref _lastAutoLibUpdate, value);
        }

        public ObservableCollection<SortItem> SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }

        public LinkSourceSettings LinkSettings
        {
            get => _linkSettings;
            set => SetValue(ref _linkSettings, value);
        }

        public LinkNamePatterns LinkNamePatterns
        {
            get => _linkPatterns;
            set => SetValue(ref _linkPatterns, value);
        }

        public LinkNamePatterns RemovePatterns
        {
            get => _removePatterns;
            set => SetValue(ref _removePatterns, value);
        }

        public LinkNamePatterns RenamePatterns
        {
            get => _renamePatterns;
            set => SetValue(ref _renamePatterns, value);
        }

        public LinkNamePatterns MissingLinkPatterns
        {
            get => _missingLinkPatterns;
            set => SetValue(ref _missingLinkPatterns, value);
        }
    }

    public class LinkUtilitiesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkUtilities _plugin;

        private LinkUtilitiesSettings _settings;

        public LinkUtilitiesSettingsViewModel(LinkUtilities plugin)
        {
            _plugin = plugin;

            LinkUtilitiesSettings savedSettings = plugin.LoadPluginSettings<LinkUtilitiesSettings>();

            if (savedSettings != null)
            {
                Settings = savedSettings;
                Settings.LinkSettings.RefreshLinkSources(AddWebsiteLinks.Instance().Links);
                Settings.LinkSettings = new LinkSourceSettings(Settings.LinkSettings.OrderBy(x => x.LinkName).ToList());
            }
            else
            {
                Settings = new LinkUtilitiesSettings
                {
                    LinkSettings = LinkSourceSettings.GetLinkSources(AddWebsiteLinks.Instance().Links)
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

        private LinkUtilitiesSettings EditingClone { get; set; }

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
            => new RelayCommand(() =>
            {
                int position = 1;

                if (Settings.SortOrder.Any())
                {
                    position = Settings.SortOrder.Max(x => x.Position) + 1;
                }

                Settings.SortOrder.Add(new SortItem
                {
                    LinkName = "",
                    Position = position
                });
            });

        public RelayCommand<IList<object>> RemoveSortItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (SortItem item in items.ToList().Cast<SortItem>())
            {
                Settings.SortOrder.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand SortSortItemsCommand => new RelayCommand(SortSortItems);

        public RelayCommand SortBookmarkletItemsCommand => new RelayCommand(() => Settings.LinkNamePatterns.SortPatterns());

        public RelayCommand SortRemoveItemsCommand => new RelayCommand(() => Settings.RemovePatterns.SortPatterns());

        public RelayCommand SortRenameItemsCommand => new RelayCommand(() => Settings.RenamePatterns.SortPatterns());

        public RelayCommand SortMissingLinkItemsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.SortPatterns());

        public RelayCommand AddLinkNamePatternCommand => new RelayCommand(() => Settings.LinkNamePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultLinkNamePatternsCommand => new RelayCommand(() => Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern));

        public RelayCommand<IList<object>> RemoveLinkNamePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern linkPattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.LinkNamePatterns.Remove(linkPattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand AddRemovePatternCommand => new RelayCommand(() => Settings.RemovePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultRemovePatternsCommand => new RelayCommand(() => Settings.RemovePatterns.AddDefaultPatterns(PatternTypes.RemovePattern));

        public RelayCommand<IList<object>> RemoveRemovePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern removePattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.RemovePatterns.Remove(removePattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand AddRenamePatternCommand => new RelayCommand(() => Settings.RenamePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultRenamePatternsCommand => new RelayCommand(() => Settings.RenamePatterns.AddDefaultPatterns(PatternTypes.RenamePattern));

        public RelayCommand<IList<object>> RemoveRenamePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern renamePattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.RenamePatterns.Remove(renamePattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand AddMissingLinkPatternCommand => new RelayCommand(() => Settings.MissingLinkPatterns.Add(new LinkNamePattern()));

        public RelayCommand AddDefaultMissingLinkPatternsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.AddDefaultPatterns(PatternTypes.MissingLinkPatterns));

        public RelayCommand<IList<object>> RemoveMissingLinkPatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern missingLinkPattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.MissingLinkPatterns.Remove(missingLinkPattern);
            }
        }, items => items?.Any() ?? false);

        public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit()
        {
            Settings.AddLinksToNewGames = EditingClone.AddLinksToNewGames;
            Settings.AddSteamAchievementLink = EditingClone.AddSteamAchievementLink;
            Settings.AddSteamCommunityLink = EditingClone.AddSteamCommunityLink;
            Settings.AddSteamDiscussionLink = EditingClone.AddSteamDiscussionLink;
            Settings.AddSteamGuidesLink = EditingClone.AddSteamGuidesLink;
            Settings.AddSteamNewsLink = EditingClone.AddSteamNewsLink;
            Settings.AddSteamStorePageLink = EditingClone.AddSteamStorePageLink;
            Settings.AddSteamWorkshopLink = EditingClone.AddSteamWorkshopLink;
            Settings.HideOkOnLinkCheck = EditingClone.HideOkOnLinkCheck;
            Settings.LinkNamePatterns = EditingClone.LinkNamePatterns;
            Settings.MissingLinkPatterns = EditingClone.MissingLinkPatterns;
            Settings.MissingLinkPrefix = EditingClone.MissingLinkPrefix;
            Settings.NameSteamAchievementLink = EditingClone.NameSteamAchievementLink;
            Settings.NameSteamCommunityLink = EditingClone.NameSteamCommunityLink;
            Settings.NameSteamDiscussionLink = EditingClone.NameSteamDiscussionLink;
            Settings.NameSteamGuidesLink = EditingClone.NameSteamGuidesLink;
            Settings.NameSteamNewsLink = EditingClone.NameSteamNewsLink;
            Settings.NameSteamStorePageLink = EditingClone.NameSteamStorePageLink;
            Settings.NameSteamWorkshopLink = EditingClone.NameSteamWorkshopLink;
            Settings.RemoveDuplicatesAfterChange = EditingClone.RemoveDuplicatesAfterChange;
            Settings.RemoveDuplicatesType = EditingClone.RemoveDuplicatesType;
            Settings.RemoveLinksAfterChange = EditingClone.RemoveLinksAfterChange;
            Settings.RemovePatterns = EditingClone.RemovePatterns;
            Settings.RenameBlocker = EditingClone.RenameBlocker;
            Settings.RenameLinksAfterChange = EditingClone.RenameLinksAfterChange;
            Settings.RenamePatterns = EditingClone.RenamePatterns;
            Settings.SortAfterChange = EditingClone.SortAfterChange;
            Settings.SortOrder = EditingClone.SortOrder;
            Settings.TagMissingLinksAfterChange = EditingClone.TagMissingLinksAfterChange;
            Settings.TagMissingLibraryLinks = EditingClone.TagMissingLibraryLinks;
            Settings.UseCustomSortOrder = EditingClone.UseCustomSortOrder;
            Settings.UseSteamAppLinks = EditingClone.UseSteamAppLinks;
            Settings.ChangeSteamLinksAfterChange = EditingClone.ChangeSteamLinksAfterChange;


            foreach (LinkSourceSetting originalItem in Settings.LinkSettings)
            {
                LinkSourceSetting clonedItem = EditingClone.LinkSettings.FirstOrDefault(x => x.LinkName == originalItem.LinkName);

                if (clonedItem == null)
                {
                    continue;
                }

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

        public void EndEdit()
        {
            Settings.RenamePatterns.SortPatterns();

            Settings.RemovePatterns.RemoveEmpty();

            _plugin.SavePluginSettings(Settings);

            WriteSettingsToLinkActions();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            HashSet<string> hashset = new HashSet<string>();

            if (Settings.SortOrder.All(item => hashset.Add(item.LinkName)))
            {
                return true;
            }

            errors.Add(ResourceProvider.GetString("LOCLinkUtilitiesErrorDuplicates"));
            return false;
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
            SortLinks.Instance().SortOrder = Settings.SortOrder.ToDictionary(x => x.LinkName, x => x.Position);
            SortLinks.Instance().SortAfterChange = Settings.SortAfterChange;
            SortLinks.Instance().UseCustomSortOrder = Settings.UseCustomSortOrder;
            HandleUriActions.Instance().LinkNamePatterns = Settings.LinkNamePatterns;
            AddLinkFromClipboard.Instance().LinkNamePatterns = Settings.LinkNamePatterns;
            RemoveLinks.Instance().RemovePatterns = Settings.RemovePatterns;
            RemoveLinks.Instance().RemoveLinksAfterChange = Settings.RemoveLinksAfterChange;
            RemoveDuplicates.Instance().RemoveDuplicatesAfterChange = Settings.RemoveDuplicatesAfterChange;
            RemoveDuplicates.Instance().RemoveDuplicatesType = Settings.RemoveDuplicatesType;
            RenameLinks.Instance().RenamePatterns = Settings.RenamePatterns;
            RenameLinks.Instance().RenameLinksAfterChange = Settings.RenameLinksAfterChange;
            RenameLinks.Instance().RenameBlocker = Settings.RenameBlocker;
            TagMissingLinks.Instance().MissingLinkPatterns = Settings.MissingLinkPatterns;
            TagMissingLinks.Instance().TagMissingLinksAfterChange = Settings.TagMissingLinksAfterChange;
            TagMissingLinks.Instance().TagMissingLibraryLinks = Settings.TagMissingLibraryLinks;
            TagMissingLinks.Instance().MissingLinkPrefix = Settings.MissingLinkPrefix;
            ChangeSteamLinks.Instance().ChangeSteamLinksAfterChange = Settings.ChangeSteamLinksAfterChange;

            LibraryLinkSteam steamLink = (LibraryLinkSteam)AddWebsiteLinks.Instance().Links?.FirstOrDefault(x => x.LinkName == "Steam");

            if (steamLink != null)
            {
                steamLink.UseAppLinks = Settings.UseSteamAppLinks;
                steamLink.AddAchievementLink = Settings.AddSteamAchievementLink;
                steamLink.AddCommunityLink = Settings.AddSteamCommunityLink;
                steamLink.AddDiscussionLink = Settings.AddSteamDiscussionLink;
                steamLink.AddGuidesLink = Settings.AddSteamGuidesLink;
                steamLink.AddNewsLink = Settings.AddSteamNewsLink;
                steamLink.AddStorePageLink = Settings.AddSteamStorePageLink;
                steamLink.AddWorkshopLink = Settings.AddSteamWorkshopLink;
                steamLink.NameAchievementLink = Settings.NameSteamAchievementLink;
                steamLink.NameCommunityLink = Settings.NameSteamCommunityLink;
                steamLink.NameDiscussionLink = Settings.NameSteamDiscussionLink;
                steamLink.NameGuidesLink = Settings.NameSteamGuidesLink;
                steamLink.NameNewsLink = Settings.NameSteamNewsLink;
                steamLink.NameStorePageLink = Settings.NameSteamStorePageLink;
                steamLink.NameWorkshopLink = Settings.NameSteamWorkshopLink;
            }

            LibraryLinkSteam steamLibLink = (LibraryLinkSteam)AddLibraryLinks.Instance().Libraries?[Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")];

            if (steamLibLink == null)
            {
                return;
            }

            steamLibLink.UseAppLinks = Settings.UseSteamAppLinks;
            steamLibLink.AddAchievementLink = Settings.AddSteamAchievementLink;
            steamLibLink.AddCommunityLink = Settings.AddSteamCommunityLink;
            steamLibLink.AddDiscussionLink = Settings.AddSteamDiscussionLink;
            steamLibLink.AddGuidesLink = Settings.AddSteamGuidesLink;
            steamLibLink.AddNewsLink = Settings.AddSteamNewsLink;
            steamLibLink.AddStorePageLink = Settings.AddSteamStorePageLink;
            steamLibLink.AddWorkshopLink = Settings.AddSteamWorkshopLink;
            steamLibLink.NameAchievementLink = Settings.NameSteamAchievementLink;
            steamLibLink.NameCommunityLink = Settings.NameSteamCommunityLink;
            steamLibLink.NameDiscussionLink = Settings.NameSteamDiscussionLink;
            steamLibLink.NameGuidesLink = Settings.NameSteamGuidesLink;
            steamLibLink.NameNewsLink = Settings.NameSteamNewsLink;
            steamLibLink.NameStorePageLink = Settings.NameSteamStorePageLink;
            steamLibLink.NameWorkshopLink = Settings.NameSteamWorkshopLink;
        }
    }
}