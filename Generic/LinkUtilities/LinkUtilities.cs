using KNARZhelper;
using LinkUtilities.LinkActions;
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
    /// Class of the actual playnite extension
    /// </summary>
    public class LinkUtilities : GenericPlugin
    {
        public LinkUtilities(IPlayniteAPI api) : base(api)
        {
            api.Database.Games.ItemUpdated += Games_ItemUpdated;

            IsUpdating = false;
            DoAfterChange = new DoAfterChange(this);
            SortLinks = new SortLinks(this);
            AddLibraryLinks = new AddLibraryLinks(this);
            AddWebsiteLinks = new AddWebsiteLinks(this);
            RemoveDuplicates = new RemoveDuplicates(this);
            RemoveLinks = new RemoveLinks(this);
            RenameLinks = new RenameLinks(this);
            TagMissingLinks = new TagMissingLinks(this);
            HandleUriActions = new HandleUriActions(this);

            Settings = new LinkUtilitiesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Settings.WriteSettingsToLinkActions();

            PlayniteApi.UriHandler.RegisterSource("LinkUtilities", (args) =>
            {
                if (HandleUriActions.ProcessArgs(args))
                {
                    List<Game> games = PlayniteApi.MainView.SelectedGames.ToList();

                    DoForAll(games, HandleUriActions, true, HandleUriActions.Action);
                }
            });
        }

        /// <summary>
        /// Class to execute specific actions after the meta data of a game changes
        /// </summary>
        public DoAfterChange DoAfterChange { get; }

        /// <summary>
        /// Class to sort the Links of a game
        /// </summary>
        public SortLinks SortLinks { get; }

        /// <summary>
        /// Class to add a link to the store page in the library of a game
        /// </summary>
        public AddLibraryLinks AddLibraryLinks { get; }

        /// <summary>
        /// Class to add a link to all available websites in the Links list, if a definitive link was found.
        /// </summary>
        public AddWebsiteLinks AddWebsiteLinks { get; }

        /// <summary>
        /// Class to remove duplicate links.
        /// </summary>
        public RemoveDuplicates RemoveDuplicates { get; }

        /// <summary>
        /// Class to remove unwanted links.
        /// </summary>
        public RemoveLinks RemoveLinks { get; }

        /// <summary>
        /// Class to rename links.
        /// </summary>
        public RenameLinks RenameLinks { get; }

        /// <summary>
        /// Class to tag missing links.
        /// </summary>
        public TagMissingLinks TagMissingLinks { get; }

        /// <summary>
        /// Handles UriHandler actions.
        /// </summary>
        public HandleUriActions HandleUriActions { get; }

        /// <summary>
        /// Is set to true, while the library is updated via the sortLinks function. Is used to avoid an endless loop in the function.
        /// </summary>
        public bool IsUpdating { get; set; }

        /// <summary>
        /// Executes a specific action for all games in a list. Shows a progress bar and result dialog and uses buffered update mode if the
        /// list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="linkAction">Instance of the action to be executed</param>
        private void DoForAll(List<Game> games, ILinkAction linkAction, bool showDialog = false, ActionModifierTypes actionModifier = ActionModifierTypes.None)
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
                    int gamesAffected = 0;

                    using (PlayniteApi.Database.BufferedUpdate())
                    {
                        GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                            $"{ResourceProvider.GetString("LOCLinkUtilitiesName")} - {ResourceProvider.GetString(linkAction.ProgressMessage)}",
                            true
                        )
                        {
                            IsIndeterminate = false
                        };

                        PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                        {
                            try
                            {
                                activateGlobalProgress.ProgressMaxValue = games.Count();

                                foreach (Game game in games)
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
        /// Event that get's triggered after updating the game database. Is used to sort Links after updating.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments. Contains a list of all updated games.</param>
        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (Settings.Settings.SortAfterChange && !IsUpdating)
            {
                List<Game> games = args.UpdatedItems.Select(item => item.NewData).Distinct().ToList();
                TagMissingLinks.TagsCache.Clear();
                DoForAll(games, DoAfterChange);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (Settings.Settings.AddLinksToNewGames)
            {
                List<Game> games = PlayniteApi.Database.Games
                    .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate).ToList();

                DoForAll(games, AddWebsiteLinks, false, ActionModifierTypes.Add);
            }

            Settings.Settings.LastAutoLibUpdate = DateTime.Now;
            SavePluginSettings(Settings.Settings);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");
            string menuFilteredGames = ResourceProvider.GetString("LOCLinkUtilitiesMenuFilteredGames");
            string menuAllGames = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllGames");

            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, DoAfterChange, true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, DoAfterChange, true);
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
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.Name);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksByName"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.Name);
                    }
                }
            };

            // Adds the "sort Links by sort order" item to the main menu.
            if (SortLinks.SortOrder != null && SortLinks.SortOrder.Count > 0)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.SortOrder);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.SortOrder);
                    }
                });
            }

            // Adds the "Remove duplicate links" item to the main menu.
            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuAllGames}",
                Action = a =>
                {
                    List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                    DoForAll(games, RemoveDuplicates, true);
                }
            });

            menuItems.Add(new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = $"@{menuSection}|{menuFilteredGames}",
                Action = a =>
                {
                    List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                    DoForAll(games, RemoveDuplicates, true);
                }
            });

            // Adds the "Remove unwanted links" item to the main menu.
            if (RemoveLinks.RemovePatterns != null && RemoveLinks.RemovePatterns.Count > 0)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, RemoveLinks, true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, RemoveLinks, true);
                    }
                });
            }
            // Adds the "Rename links" item to the main menu.
            if (RenameLinks.RenamePatterns != null && RenameLinks.RenamePatterns.Count > 0)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        DoForAll(games, RenameLinks, true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        DoForAll(games, RenameLinks, true);
                    }
                });
            }
            // Adds the "Tag missing links" item to the main menu.
            if (TagMissingLinks.MissingLinkPatterns != null && TagMissingLinks.MissingLinkPatterns.Count > 0)
            {
                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = $"@{menuSection}|{menuAllGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.Database.Games.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, TagMissingLinks, true);
                    }
                });

                menuItems.Add(new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = $"@{menuSection}|{menuFilteredGames}",
                    Action = a =>
                    {
                        List<Game> games = PlayniteApi.MainView.FilteredGames.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, TagMissingLinks, true);
                    }
                });
            }

            return menuItems;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");
            string menuAddLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLinkTo");
            string menuSearchLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuSearchLinkTo");

            List<GameMenuItem> menuItems = new List<GameMenuItem>
            {
                // Adds the "add library Links" item to the game menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLibraryLink"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddLibraryLinks, true);
                    }
                },
                // Adds the "All configured websites" item to the "add link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = $"{menuSection}|{menuAddLinks}",
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddWebsiteLinks, true, ActionModifierTypes.Add);
                    }
                },
                // Adds a separator to the "add link to" sub menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = $"{menuSection}|{menuAddLinks}"
                },
                // Adds the "All configured websites" item to the "search link to" sub menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = $"{menuSection}|{menuSearchLinks}",
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddWebsiteLinks, true, ActionModifierTypes.Search);
                    }
                },
                // Adds a separator to the "search link to" sub menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = $"{menuSection}|{menuSearchLinks}"
                },
                // Adds a separator to the game menu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuCleanUp"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, DoAfterChange, true);
                    }
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
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.Name);
                    }
                }
            };

            // Adds the "sort Links by sort order" item to the game menu.
            if (SortLinks.SortOrder != null && SortLinks.SortOrder.Count > 0)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinksBySortOrder"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, SortLinks, true, ActionModifierTypes.SortOrder);
                    }
                });
            }

            // Adds all linkable websites to the "add link to" and "search link to" sub menus.
            foreach (Linker.Link link in AddWebsiteLinks.Links)
            {
                if (link.Settings.ShowInMenus & link.AddType != Linker.LinkAddTypes.None)
                {
                    menuItems.Add(new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuAddLinks}",
                        Action = a =>
                        {
                            List<Game> games = args.Games.Distinct().ToList();
                            DoForAll(games, link, true, ActionModifierTypes.Add);
                        }
                    });
                }

                if (link.Settings.ShowInMenus & link.CanBeSearched)
                {
                    menuItems.Add(new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuSearchLinks}",
                        Action = a =>
                        {
                            List<Game> games = args.Games.Distinct().ToList();
                            DoForAll(games, link, true, ActionModifierTypes.Search);
                        }
                    });

                }
            }

            // Adds the "Remove duplicate links" item to the game menu.
            menuItems.Add(new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveDuplicateLinks"),
                MenuSection = menuSection,
                Action = a =>
                {
                    List<Game> games = args.Games.Distinct().ToList();
                    DoForAll(games, RemoveDuplicates, true);
                }
            });

            // Adds the "Remove unwanted links" item to the game menu.
            if (RemoveLinks.RemovePatterns != null && RemoveLinks.RemovePatterns.Count > 0)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveUnwantedLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, RemoveLinks, true);
                    }
                });
            }
            // Adds the "Rename links" item to the game menu.
            if (RenameLinks.RenamePatterns != null && RenameLinks.RenamePatterns.Count > 0)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuRenameLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, RenameLinks, true);
                    }
                });
            }
            // Adds the "Tag missing links" item to the game menu.
            if (TagMissingLinks.MissingLinkPatterns != null && TagMissingLinks.MissingLinkPatterns.Count > 0)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuTagMissingLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        TagMissingLinks.TagsCache.Clear();
                        DoForAll(games, TagMissingLinks, true);
                    }
                });
            }

            return menuItems;
        }

        /// <summary>
        /// The settings view model of the extension
        /// </summary>
        public LinkUtilitiesSettingsViewModel Settings { get; set; }

        /// <summary>
        /// The global GUID to identify the extension in playnite
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("f692b4bb-238d-4080-ae76-4aaefde6f7a1");

        /// <summary>
        /// Gets the settings of the extension
        /// </summary>
        /// <param name="firstRunSettings">True, if it's the first time the settings are fetched</param>
        /// <returns>Settings interface</returns>
        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        /// <summary>
        /// Gets the settings view to be shown in playnite
        /// </summary>
        /// <param name="firstRunSettings">True, if it's the first time the settings view is fetched</param>
        /// <returns>Settings view</returns>
        public override UserControl GetSettingsView(bool firstRunSettings) => new LinkUtilitiesSettingsView();
    }
}