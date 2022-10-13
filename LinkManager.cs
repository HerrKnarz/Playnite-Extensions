using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Game = Playnite.SDK.Models.Game;

namespace LinkManager
{
    public class LinkManager : GenericPlugin
    {
        public SortLinks SortLinks;
        public AddLibraryLinks AddLibraryLinks;

        public LinkManager(IPlayniteAPI api) : base(api)
        {
            Settings = new LinkManagerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            SortLinks = new SortLinks(Settings.Settings);
            AddLibraryLinks = new AddLibraryLinks(Settings.Settings);
        }

        private void DoForAll(List<Game> games, ILinkAction linkAction)
        {
            if (games.Count == 1)
            {
                linkAction.Execute(games.First());
            }
            else if (games.Count > 1)
            {
                int gamesAffected = 0;

                using (PlayniteApi.Database.BufferedUpdate())
                {
                    GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                        $"LinkManager - {ResourceProvider.GetString(linkAction.ProgressMessage)}",
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

                                if (linkAction.Execute(game))
                                {
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
                    PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(linkAction.ResultMessage), gamesAffected));
                }
            }
        }

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
                        List<Game> games = args.Games.Distinct().ToList();
                        DoForAll(games, SortLinks);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCLinkManagerAddLibraryLink"),
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