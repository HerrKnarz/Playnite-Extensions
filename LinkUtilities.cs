using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities
{
    /// <summary>
    /// Class of the actual playnite extension
    /// </summary>
    public class LinkUtilities : GenericPlugin
    {
        public LinkUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new LinkUtilitiesSettingsViewModel(this);
            api.Database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            api.Database.Games.ItemUpdated += Games_ItemUpdated;
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            SortLinks = new SortLinks(Settings.Settings);
            AddLibraryLinks = new AddLibraryLinks(Settings.Settings);
            IsUpdating = false;
        }

        /// <summary>
        /// Class to sort the links of a game
        /// </summary>
        public SortLinks SortLinks;

        /// <summary>
        /// Class to add a link to the store page in the library of a game
        /// </summary>
        public AddLibraryLinks AddLibraryLinks;

        /// <summary>
        /// Is set to true, while the library is updated via the SortLinks function. Is used to avoid an endless loop in the function.
        /// </summary>
        public bool IsUpdating { get; set; }

        /// <summary>
        /// Executes a specific action for all games in a list. Shows a progress bar and result dialog and uses buffered update mode if the
        /// list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="linkAction">Instance of the action to be executed</param>
        private void DoForAll(List<Game> games, ILinkAction linkAction)
        {
            // While sorting links we set IsUpdating to true, so the libraby update event knows it doesn't need to sort again.
            IsUpdating = true;
            try
            {
                if (games.Count == 1)
                {
                    linkAction.Execute(games.First());
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

                                    if (linkAction.Execute(game))
                                    {
                                        gamesAffected++;
                                    }

                                    activateGlobalProgress.CurrentProgressValue++;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }
                        }, globalProgressOptions);

                    }

                    // Shows a dialog with the number of games actually affected.
                    PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(linkAction.ResultMessage), gamesAffected));
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }

        /// <summary>
        /// Event that get's triggered after updating the game database. Is used to sort links after updating.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments. Contains a list of all updated games.</param>
        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (Settings.Settings.SortAfterChange && !IsUpdating)
            {
                List<Game> games = args.UpdatedItems.Select(item => item.NewData).Distinct().ToList();
                DoForAll(games, SortLinks);
            }
        }

        /// <summary>
        /// Event that get's triggered after games are added or deleted. Is used to sort links for new games.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments. Contains a list of all added games.</param>
        public void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> args)
        {
            if (Settings.Settings.SortAfterChange && !IsUpdating)
            {
                if (args.AddedItems.Count > 0)
                {
                    List<Game> games = args.AddedItems.Distinct().ToList();
                    DoForAll(games, SortLinks);
                }
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCLinkUtilitiesName");

            return new List<GameMenuItem>
            {
                // Adds the "sort links" item to the game menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesSortLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, SortLinks);
                    }
                },
                // Adds the "add library links" item to the game menu.
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkUtilitiesAddLibraryLink"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, AddLibraryLinks);
                    }
                }
            };
        }

        private static readonly ILogger logger = LogManager.GetLogger();

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