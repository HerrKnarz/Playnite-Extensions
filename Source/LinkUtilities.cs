using LinkUtilities.LinkActions;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            sortLinks = new SortLinks(this);
            AddLibraryLinks = new AddLibraryLinks(this);
            AddWebsiteLinks = new AddWebsiteLinks(this);
            IsUpdating = false;

            Settings = new LinkUtilitiesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            PlayniteApi.UriHandler.RegisterSource("LinkUtilities", (args) =>
            {
                // UNDONE: put this into a proper DoForAll class. Waits for the bugfix in argument parsing for now...
                foreach (Game game in PlayniteApi.MainView.SelectedGames)
                {
                    LinkHelper.AddLink(game, args.Arguments[1], WebUtility.UrlDecode(args.Arguments[2]), Settings.Settings);
                }
            });
        }

        /// <summary>
        /// Class to sort the Links of a game
        /// </summary>
        private readonly SortLinks sortLinks;

        /// <summary>
        /// Class to add a link to the store page in the library of a game
        /// </summary>
        public AddLibraryLinks AddLibraryLinks { get; }

        /// <summary>
        /// Class to add a link to all available websites in the Links list, if a definitive link was found.
        /// </summary>
        public AddWebsiteLinks AddWebsiteLinks { get; }

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
        private void DoForAll(List<Game> games, ILinkAction linkAction, bool showDialog = false, string actionModifier = "")
        {
            // While sorting Links we set IsUpdating to true, so the libraby update event knows it doesn't need to sort again.
            IsUpdating = true;

            try
            {
                if (games.Count == 1)
                {
                    linkAction.Execute(games.First(), actionModifier);
                }
                // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
                else if (games.Count > 1)
                {
                    int gamesAffected = 0;

                    using (PlayniteApi.Database.BufferedUpdate())
                    {
                        GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                            $"LinkUtilities - {ResourceProvider.GetString(linkAction.ProgressMessage)}",
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
                DoForAll(games, sortLinks);
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");
            string menuAddLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuAddLinkTo");
            string menuSearchLinks = ResourceProvider.GetString("LOCLinkUtilitiesMenuSearchLinkTo");

            List<GameMenuItem> menuItems = new List<GameMenuItem>
            {
                // Adds the "sort Links" item to the game menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuSortLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, sortLinks, true);
                    }
                },
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
                // Adds the "All configured websites" item to the "add link to" submenu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = $"{menuSection}|{menuAddLinks}",
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddWebsiteLinks, true, "add");
                    }
                },
                // Adds a separator to the "add link to" submenu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = $"{menuSection}|{menuAddLinks}"
                },
                // Adds the "All configured websites" item to the "search link to" submenu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesMenuAllConfiguredWebsites"),
                    MenuSection = $"{menuSection}|{menuSearchLinks}",
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddWebsiteLinks, true, "search");
                    }
                },
                // Adds a separator to the "search link to" submenu
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = $"{menuSection}|{menuSearchLinks}"
                }
            };

            // Adds all linkable websites to the "add link to" and "search link to" submenus.
            foreach (Linker.Link link in AddWebsiteLinks.Links)
            {
                if (link.IsAddable)
                {
                    menuItems.Add(
                    new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuAddLinks}",
                        Action = a =>
                        {
                            List<Game> games = args.Games.Distinct().ToList();
                            DoForAll(games, link, true, "add");
                        }
                    });
                }

                if (link.IsSearchable)
                {
                    menuItems.Add(
                    new GameMenuItem
                    {
                        Description = link.LinkName,
                        MenuSection = $"{menuSection}|{menuSearchLinks}",
                        Action = a =>
                        {
                            List<Game> games = args.Games.Distinct().ToList();
                            DoForAll(games, link, true, "search");
                        }
                    });
                }
            }

            return menuItems;
            ;
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
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        /// <summary>
        /// Gets the settings view to be shown in playnite
        /// </summary>
        /// <param name="firstRunSettings">True, if it's the first time the settings view is fetched</param>
        /// <returns>Settings view</returns>
        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LinkUtilitiesSettingsView();
        }
    }
}