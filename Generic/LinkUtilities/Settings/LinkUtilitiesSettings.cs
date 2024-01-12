using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private ObservableCollection<CustomLinkProfile> _customLinkProfiles;
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


        public ObservableCollection<CustomLinkProfile> CustomLinkProfiles
        {
            get => _customLinkProfiles;
            set => SetValue(ref _customLinkProfiles, value);
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
}