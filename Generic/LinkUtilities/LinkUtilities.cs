﻿using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.ViewModels;
using LinkUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace LinkUtilities
{
    /// <summary>
    ///     Class of the actual playnite extension
    /// </summary>
    public class LinkUtilities : GenericPlugin
    {
        public LinkUtilities(IPlayniteAPI api) : base(api)
        {
            api.Database.Games.ItemUpdated += Games_ItemUpdated;

            IsUpdating = false;

            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Settings.WriteSettingsToLinkActions();

            PlayniteApi.UriHandler.RegisterSource("LinkUtilities", args =>
            {
                if (!HandleUriActions.Instance().ProcessArgs(args))
                {
                    return;
                }

                var games = PlayniteApi.MainView.SelectedGames.Distinct().ToList();

                DoForAll(games, HandleUriActions.Instance(), true, HandleUriActions.Instance().Action);
            });

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "luLinkIcon", "\xef71" },
                { "luAddIcon", "\xec3e" },
                { "luSearchIcon", "\xed1b" },
                { "luLibraryIcon", "\xef65" },
                { "luClipboardIcon", "\xec4c" },
                { "luCheckIcon", "\xf021" },
                { "luCleanIcon", "\xe99c" },
                { "luSortIcon", "\xefee" },
                { "luRemoveIcon", "\xec7e" },
                { "luReviewIcon", "\xeaeb" },
                { "luDuplicateIcon", "\xedea" },
                { "luRenameIcon", "\xeded" },
                { "luTagIcon", "\xf004" },
                { "luSteamIcon", "\xe93e" },
                { "luWebIcon", "\xf028" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        /// <summary>
        ///     The global GUID to identify the extension in playnite
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("f692b4bb-238d-4080-ae76-4aaefde6f7a1");

        /// <summary>
        ///     Is set to true, while the library is updated via the sortLinks function. Is used to avoid an endless loop in the
        ///     function.
        /// </summary>
        internal bool IsUpdating { get; set; }

        /// <summary>
        ///     The settings view model of the extension
        /// </summary>
        public SettingsViewModel Settings { get; set; }

        public void CheckLinks(List<Game> games)
        {
            try
            {
                var viewModel = new CheckLinksViewModel(games, Settings.Settings.HideOkOnLinkCheck);

                if (!viewModel.CheckLinks.Links?.Any() ?? true)
                {
                    API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCLinkUtilitiesDialogNoLinksFound"));
                    return;
                }

                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesCheckLinksWindowName"));
                var view = new CheckLinksView { DataContext = viewModel };

                window.Content = view;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing CheckLinksView", true);
            }
        }

        /// <summary>
        ///     Executes a specific action for all games in a list. Shows a progress bar and result dialog and uses buffered update
        ///     mode if the
        ///     list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="linkAction">Instance of the action to be executed</param>
        /// <param name="showDialog">If true a dialog will be shown after completion</param>
        /// <param name="actionModifier">specifies the type of action to execute, if more than one is possible.</param>
        public void DoForAll(List<Game> games, ILinkAction linkAction, bool showDialog = false, ActionModifierTypes actionModifier = ActionModifierTypes.None)
        {
            // While sorting Links we set IsUpdating to true, so the library update event knows it doesn't need to sort again.
            IsUpdating = true;

            try
            {
                if (games.Count == 1)
                {
                    linkAction.Execute(games.First(), actionModifier, false);
                }
                // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
                else if (games.Count > 1)
                {
                    var gamesAffected = 0;

                    using (PlayniteApi.Database.BufferedUpdate())
                    {
                        if (!linkAction.Prepare(actionModifier))
                        {
                            return;
                        }

                        var globalProgressOptions = new GlobalProgressOptions(
                            $"{ResourceProvider.GetString("LOCLinkUtilitiesName")} - {ResourceProvider.GetString(linkAction.ProgressMessage)}",
                            true
                        )
                        {
                            IsIndeterminate = false
                        };

                        PlayniteApi.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                        {
                            try
                            {
                                activateGlobalProgress.ProgressMaxValue = games.Count;

                                foreach (var game in games)
                                {
                                    activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCLinkUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(linkAction.ProgressMessage)}{Environment.NewLine}{game.Name}";

                                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    if (linkAction.Execute(game, actionModifier))
                                    {
                                        gamesAffected++;
                                    }

                                    activateGlobalProgress.CurrentProgressValue++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                            }
                        }, globalProgressOptions);
                    }

                    // Shows a dialog with the number of games actually affected.
                    if (showDialog)
                    {
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(linkAction.ResultMessage), gamesAffected));
                    }
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }

        /// <summary>
        ///     Event that get's triggered after updating the game database. Is used to sort Links after updating.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments. Contains a list of all updated games.</param>
        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (!Settings.Settings.SortAfterChange || IsUpdating)
            {
                return;
            }

            var games = args.UpdatedItems.Where(item =>
                    item.OldData == null ||
                    (item.NewData.Links != null &&
                     item.NewData.Links.Count > 0 &&
                     (item.OldData.Links == null ||
                     !item.OldData.Links.IsListEqualExact(item.NewData.Links))))
                .Select(item => item.NewData).Distinct().ToList();
            DoForAll(games, DoAfterChange.Instance());
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");
            var menuAddLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLinkTo");
            var menuSearchLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuSearchLinkTo");
            var menuBrowserSearchLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuBrowserSearchLinkTo");

            var menuItems = new List<GameMenuItem>();
            var menuAddItems = new List<GameMenuItem>();
            var menuSearchItems = new List<GameMenuItem>();
            var menuBrowserSearchItems = new List<GameMenuItem>();

            var games = args.Games.Distinct().ToList();

            // Adds all linkable websites to the "add link to" and "search link to" sub menus.
            foreach (var link in AddWebsiteLinks.Instance().Links.OrderBy(x => x.LinkName))
            {
                if (link.Settings.ShowInMenus & (link.AddType != LinkAddTypes.None))
                {
                    menuAddItems.Add(new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuAddLinks}",
                        Action = a => DoForAll(games, link, true, ActionModifierTypes.Add)
                    });
                }

                if (link.Settings.ShowInMenus & link.CanBeSearched)
                {
                    menuSearchItems.Add(new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuSearchLinks}",
                        Action = a => DoForAll(games, link, true, ActionModifierTypes.Search)
                    });
                }

                if (games.Count == 1 && link.Settings.ShowInMenus && link.CanBeBrowserSearched)
                {
                    menuBrowserSearchItems.Add(new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuBrowserSearchLinks}",
                        Action = a => DoForAll(games, link, true, ActionModifierTypes.SearchInBrowser)
                    });
                }
            }

            if (games.Any(g => AddLibraryLinks.Instance().LibraryLinks.ContainsKey(g.PluginId)))
            {
                // Adds the "add library Links" item to the game menu.
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLibraryLink"),
                    MenuSection = menuSection,
                    Icon = "luLibraryIcon",
                    Action = a => DoForAll(games, AddLibraryLinks.Instance(), true)
                });
            }

            menuItems.AddRange(menuAddItems);

            menuItems.AddRange(new List<GameMenuItem>
            {
                // Adds the "All configured websites" item to the "add link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = menuSection,
                    Icon = "luAddIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), true, ActionModifierTypes.Add)
                },
                // Adds the "selected websites..." item to the "add link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSelectedWebsites"),
                    MenuSection = menuSection,
                    Icon = "luAddIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), true, ActionModifierTypes.AddSelected)
                },
                // Adds a separator to the "add link to" sub menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                }
            });

            menuItems.AddRange(menuSearchItems);

            menuItems.AddRange(new List<GameMenuItem>
            {
                // Adds the "All configured websites" item to the "search link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = menuSection,
                    Icon = "luSearchIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), true, ActionModifierTypes.Search)
                },
                // Adds the "All missing websites" item to the "search link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllMissingWebsites"),
                    MenuSection = menuSection,
                    Icon = "luSearchIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), true, ActionModifierTypes.SearchMissing)
                },
                // Adds the "selected websites..." item to the "search link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSelectedWebsites"),
                    MenuSection = menuSection,
                    Icon = "luSearchIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), true, ActionModifierTypes.SearchSelected)
                },
                // Adds a separator to the "search link to" sub menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                }
            });

            menuItems.AddRange(menuBrowserSearchItems);

            menuItems.AddRange(new List<GameMenuItem>
            {
                // Adds the "All missing websites" item to the browser search sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllMissingWebsites"),
                    MenuSection = menuSection,
                    Icon = "luWebIcon",
                    Action = a => DoForAll(games, AddWebsiteLinks.Instance(), false, ActionModifierTypes.SearchInBrowser)
                },
                // Adds a separator to the browser search sub menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                },
                // Adds the "Add link from clipboard" item to the game menu
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLinkFromClipboard"),
                    MenuSection = menuSection,
                    Icon = "luClipboardIcon",
                    Action = a => DoForAll(games, AddLinkFromClipboard.Instance(), true)
                },
                // Adds a separator to the game menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                },
                // Adds the "Check links" item to the game menu
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCheckLinks"),
                    MenuSection = menuSection,
                    Icon = "luCheckIcon",
                    Action = a => CheckLinks(games)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = menuSection,
                    Icon = "luCleanIcon",
                    Action = a => DoForAll(games, DoAfterChange.Instance(), true)
                },
                // Adds a separator to the game menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                },
                // Adds the "sort Links by name" item to the game menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksByName"),
                    MenuSection = menuSection,
                    Icon = "luSortIcon",
                    Action = a => DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.Name)
                }
            });

            // Adds the "sort Links by sort order" item to the game menu.
            if (SortLinks.Instance().SortOrder?.Any() ?? false)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = menuSection,
                    Icon = "luSortIcon",
                    Action = a => DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.SortOrder)
                });
            }

            // Adds the "Remove specific links" item to the game menu.
            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveSpecificLinks"),
                MenuSection = menuSection,
                Icon = "luDuplicateIcon",
                Action = a => LinkHelper.RemoveSpecificLinks(games, this)
            });

            // Adds the "Remove duplicate links" item to the game menu.
            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = menuSection,
                Icon = "luDuplicateIcon",
                Action = a => DoForAll(games, RemoveDuplicates.Instance(), true)
            });

            // Adds the "Review duplicate links" item to the game menu.
            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuReviewDuplicateLinks"),
                MenuSection = menuSection,
                Icon = "luReviewIcon",
                Action = a => RemoveDuplicates.ShowReviewDuplicatesView(games)
            });

            // Adds the "Remove unwanted links" item to the game menu.
            if (RemoveLinks.Instance().RemovePatterns?.Any() ?? false)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = menuSection,
                    Icon = "luRemoveIcon",
                    Action = a => DoForAll(games, RemoveLinks.Instance(), true)
                });
            }

            // Adds the "Rename links" item to the game menu.
            if (RenameLinks.Instance().RenamePatterns?.Any() ?? false)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = menuSection,
                    Icon = "luRenameIcon",
                    Action = a => DoForAll(games, RenameLinks.Instance(), true)
                });
            }

            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToApp"),
                MenuSection = menuSection,
                Icon = "luSteamIcon",
                Action = a => DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.AppLink)
            });

            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToWeb"),
                MenuSection = menuSection,
                Icon = "luSteamIcon",
                Action = a => DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.WebLink)
            });

            // Adds the "Tag missing links" item to the game menu.
            if (TagMissingLinks.Instance().MissingLinkPatterns?.Any() ?? false)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = menuSection,
                    Icon = "luTagIcon",
                    Action = a => DoForAll(games, TagMissingLinks.Instance(), true)
                });
            }

            return menuItems;
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");
            var menuFilteredGames = ResourceProvider.GetString("LOCLinkUtilitiesMenuFilteredGames");
            var menuAllGames = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllGames");

            var menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luCleanIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, DoAfterChange.Instance(), true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luCleanIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, DoAfterChange.Instance(), true);
                    }
                },
                // Adds a separator
                new MainMenuItem
                {
                    Description = "-",
                    MenuSection = $"@{menuSection}|{menuAllGames}"
                },
                new MainMenuItem
                {
                    Description = "-",
                    MenuSection = $"@{menuSection}|{menuFilteredGames}"
                },
                // Adds the "sort Links by name" item to the main menu.
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksByName"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luSortIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.Name);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksByName"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luSortIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.Name);
                    }
                }
            };

            // Adds the "sort Links by sort order" item to the main menu.
            if (SortLinks.Instance().SortOrder?.Any() ?? false)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luSortIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.SortOrder);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luSortIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, SortLinks.Instance(), true, ActionModifierTypes.SortOrder);
                    }
                });
            }

            // Adds the "Remove duplicate links" item to the main menu.
            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuAllGames}",
                Icon = "luDuplicateIcon",
                Action = a =>
                {
                    var games = PlayniteApi.Database.Games.Distinct().ToList();
                    DoForAll(games, RemoveDuplicates.Instance(), true);
                }
            });

            // Adds the "Review duplicate links" item to the main menu.
            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuReviewDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuAllGames}",
                Icon = "luReviewIcon",
                Action = a =>
                {
                    var games = PlayniteApi.Database.Games.Distinct().Where(x => !x.Hidden).ToList();
                    RemoveDuplicates.ShowReviewDuplicatesView(games);
                }
            });

            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuFilteredGames}",
                Icon = "luDuplicateIcon",
                Action = a =>
                {
                    var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                    DoForAll(games, RemoveDuplicates.Instance(), true);
                }
            });

            // Adds the "Review duplicate links" item to the main menu.
            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuReviewDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuFilteredGames}",
                Icon = "luReviewIcon",
                Action = a =>
                {
                    var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                    RemoveDuplicates.ShowReviewDuplicatesView(games);
                }
            });

            // Adds the "Remove unwanted links" item to the main menu.
            if (RemoveLinks.Instance().RemovePatterns?.Any() ?? false)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luRemoveIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, RemoveLinks.Instance(), true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luRemoveIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, RemoveLinks.Instance(), true);
                    }
                });
            }

            // Adds the "Rename links" item to the main menu.
            if (RenameLinks.Instance().RenamePatterns?.Any() ?? false)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luRenameIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, RenameLinks.Instance(), true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luRenameIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, RenameLinks.Instance(), true);
                    }
                });
            }

            // Adds the "Change steam links" item to the main menu.
            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToApp"),
                MenuSection = $"@{menuSection}|{menuAllGames}",
                Icon = "luSteamIcon",
                Action = a =>
                {
                    var games = PlayniteApi.Database.Games.Distinct().ToList();
                    DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.AppLink);
                }
            });

            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToApp"),
                MenuSection = $"@{menuSection}|{menuFilteredGames}",
                Icon = "luSteamIcon",
                Action = a =>
                {
                    var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                    DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.AppLink);
                }
            });

            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToWeb"),
                MenuSection = $"@{menuSection}|{menuAllGames}",
                Icon = "luSteamIcon",
                Action = a =>
                {
                    var games = PlayniteApi.Database.Games.Distinct().ToList();
                    DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.WebLink);
                }
            });

            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuChangeSteamLinksToWeb"),
                MenuSection = $"@{menuSection}|{menuFilteredGames}",
                Icon = "luSteamIcon",
                Action = a =>
                {
                    var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                    DoForAll(games, ChangeSteamLinks.Instance(), true, ActionModifierTypes.WebLink);
                }
            });

            // Adds the "Tag missing links" item to the main menu.
            if (TagMissingLinks.Instance().MissingLinkPatterns?.Any() ?? false)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Icon = "luTagIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, TagMissingLinks.Instance(), true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Icon = "luTagIcon",
                    Action = a =>
                    {
                        var games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, TagMissingLinks.Instance(), true);
                    }
                });
            }

            return menuItems;
        }

        /// <summary>
        ///     Gets the settings of the extension
        /// </summary>
        /// <param name="firstRunSettings">True, if it's the first time the settings are fetched</param>
        /// <returns>Settings interface</returns>
        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        /// <summary>
        ///     Gets the settings view to be shown in playnite
        /// </summary>
        /// <param name="firstRunSettings">True, if it's the first time the settings view is fetched</param>
        /// <returns>Settings view</returns>
        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (Settings.Settings.AddLinksToNewGames)
            {
                var games = PlayniteApi.Database.Games
                    .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate).ToList();

                DoForAll(games, AddWebsiteLinks.Instance(), false, ActionModifierTypes.Add);
            }

            Settings.Settings.LastAutoLibUpdate = DateTime.Now;
            SavePluginSettings(Settings.Settings);
        }
    }
}