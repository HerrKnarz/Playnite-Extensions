using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace LinkManager
{
    public class LinkManager : GenericPlugin
    {
        public LinkManager(IPlayniteAPI api) : base(api)
        {
            Settings = new LinkManagerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public void SortLinks(List<Game> games)
        {
            int gamesAffected = 0;

            using (PlayniteApi.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    $"LinkManager - {ResourceProvider.GetString("LOCLinkManagerLSortLinksProgress")}",
                    true
                )
                {
                    IsIndeterminate = false
                };

                PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = (double)games.Count();

                        foreach (Game game in games)
                        {
                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            if (LinkHelper.SortLinks(game))
                                gamesAffected++;

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Info("LinkManager:" + ex.Message);
                    }
                }, globalProgressOptions);

            }

            if (games.Count > 1)
            {
                PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCLinkManagerSortedMessage"), gamesAffected));
            }
        }

        public void AddLibraryLink(List<Game> games)
        {
            int gamesAffected = 0;
            Libraries libraries = new Libraries(Settings.Settings);
            ILinkAssociation library;

            using (PlayniteApi.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    $"LinkManager - {ResourceProvider.GetString("LOCLinkManagerLibraryLinkProgress")}",
                    true
                )
                {
                    IsIndeterminate = false
                };

                PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = (double)games.Count();

                        foreach (Game game in games)
                        {
                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            library = libraries.Find(x => x.AssociationId == game.PluginId);

                            if (library is object)
                            {
                                if (library.AddLink(game))
                                    gamesAffected++;
                            }

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Info("LinkManager:" + ex.Message);
                    }
                }, globalProgressOptions);
            }

            if (games.Count > 1)
            {
                PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCLinkManagerAddedMessage"), gamesAffected));
            }
        }

        // To add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCLinkManagerName");

            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkManagerSortLinks"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        var games = args.Games.Distinct().ToList();
                        SortLinks(games);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkManagerAddLibraryLink"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        var games = args.Games.Distinct().ToList();
                        AddLibraryLink(games);
                    }
                }
            };
        }

        private static readonly ILogger logger = LogManager.GetLogger();

        public LinkManagerSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("f692b4bb-238d-4080-ae76-4aaefde6f7a1");

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LinkManagerSettingsView();
        }
    }
}