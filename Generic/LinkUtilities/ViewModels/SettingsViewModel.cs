using LinkUtilities.LinkActions;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LinkUtilities
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkUtilities _plugin;

        private string _exampleName = "Baldur's Gate 3";
        private string _exampleResult = "";

        private CustomLinkProfile _selectedItem;

        private LinkUtilitiesSettings _settings;

        public SettingsViewModel(LinkUtilities plugin)
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

            if (Settings.CustomLinkProfiles is null)
            {
                Settings.CustomLinkProfiles = new ObservableCollection<CustomLinkProfile>();
            }
            else
            {
                SortCustomLinkProfiles();
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

        public RelayCommand AddCustomLinkProfileCommand => new RelayCommand(() => Settings.CustomLinkProfiles.Add(new CustomLinkProfile()));

        public RelayCommand AddDefaultLinkNamePatternsCommand => new RelayCommand(() => Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern));

        public RelayCommand AddDefaultMissingLinkPatternsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.AddDefaultPatterns(PatternTypes.MissingLinkPatterns));

        public RelayCommand AddDefaultRemovePatternsCommand => new RelayCommand(() => Settings.RemovePatterns.AddDefaultPatterns(PatternTypes.RemovePattern));

        public RelayCommand AddDefaultRenamePatternsCommand => new RelayCommand(() => Settings.RenamePatterns.AddDefaultPatterns(PatternTypes.RenamePattern));

        public RelayCommand AddLinkNamePatternCommand => new RelayCommand(() => Settings.LinkNamePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddMissingLinkPatternCommand => new RelayCommand(() => Settings.MissingLinkPatterns.Add(new LinkNamePattern()));

        public RelayCommand AddRemovePatternCommand => new RelayCommand(() => Settings.RemovePatterns.Add(new LinkNamePattern()));

        public RelayCommand AddRenamePatternCommand => new RelayCommand(() => Settings.RenamePatterns.Add(new LinkNamePattern()));

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

        public string ExampleName
        {
            get => _exampleName;
            set
            {
                _exampleName = value;
                ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string ExampleResult
        {
            get => _exampleResult;
            set
            {
                _exampleResult = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand HelpBookmarkletCommand => new RelayCommand(() =>
            Process.Start(new ProcessStartInfo(
                "https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-URL-handler-and-bookmarklet")));

        public RelayCommand HelpCustomLinkProfileCommand => new RelayCommand(() =>
            Process.Start(new ProcessStartInfo(
                "https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-Custom-link-profiles")));

        public RelayCommand<IList<object>> RemoveCustomLinkProfileCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (CustomLinkProfile item in items.ToList().Cast<CustomLinkProfile>())
            {
                Settings.CustomLinkProfiles.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveLinkNamePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern linkPattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.LinkNamePatterns.Remove(linkPattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveMissingLinkPatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern missingLinkPattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.MissingLinkPatterns.Remove(missingLinkPattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveRemovePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern removePattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.RemovePatterns.Remove(removePattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveRenamePatternsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkNamePattern renamePattern in items.ToList().Cast<LinkNamePattern>())
            {
                Settings.RenamePatterns.Remove(renamePattern);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveSortItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (SortItem item in items.ToList().Cast<SortItem>())
            {
                Settings.SortOrder.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public CustomLinkProfile SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public LinkUtilitiesSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SortBookmarkletItemsCommand => new RelayCommand(() => Settings.LinkNamePatterns.SortPatterns());
        public RelayCommand SortCustomLinkProfileCommand => new RelayCommand(SortCustomLinkProfiles);
        public RelayCommand SortMissingLinkItemsCommand => new RelayCommand(() => Settings.MissingLinkPatterns.SortPatterns());
        public RelayCommand SortRemoveItemsCommand => new RelayCommand(() => Settings.RemovePatterns.SortPatterns());
        public RelayCommand SortRenameItemsCommand => new RelayCommand(() => Settings.RenamePatterns.SortPatterns());
        public RelayCommand SortSortItemsCommand => new RelayCommand(SortSortItems);

        public RelayCommand TestCustomLinkProfileCommand => new RelayCommand(() =>
            ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty);

        private LinkUtilitiesSettings EditingClone { get; set; }

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
            Settings.CustomLinkProfiles = EditingClone.CustomLinkProfiles;
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

        public void WriteSettingsToLinkActions()
        {
            AddWebsiteLinks.Instance().CustomLinkProfiles = Settings.CustomLinkProfiles.ToList();
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

        private void SortCustomLinkProfiles()
        {
            Settings.CustomLinkProfiles = new ObservableCollection<CustomLinkProfile>(Settings.CustomLinkProfiles
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
        }

        private void SortSortItems()
        {
            Settings.SortOrder = new ObservableCollection<SortItem>(Settings.SortOrder
                .OrderBy(x => x.Position)
                .ThenBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
        }
    }
}