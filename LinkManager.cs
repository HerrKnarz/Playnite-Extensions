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

        public static void SortLinks(List<Game> games)
        {
            int gamesAffected = 0;

            foreach (Game game in games)
            {
                if (LinkHelper.SortLinks(game))
                    gamesAffected++;
            }

            if (games.Count > 1)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCLinkManagerSortedMessage"), gamesAffected));
            }
        }

        public static void AddLibraryLink(List<Game> games)
        {
            int gamesAffected = 0;
            Libraries libraries = new Libraries();
            ILinkAssociation library;

            foreach (Game game in games)
            {
                library = libraries.Find(x => x.AssociationId == game.PluginId);

                if (library is object)
                {
                    if (library.AddLink(game))
                        gamesAffected++;
                }
            }

            if (games.Count > 1)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCLinkManagerAddedMessage"), gamesAffected));
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

        //private static readonly ILogger logger = LogManager.GetLogger();

        private LinkManagerSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("f692b4bb-238d-4080-ae76-4aaefde6f7a1");

        public LinkManager(IPlayniteAPI api) : base(api)
        {
            Settings = new LinkManagerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        /*public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }*/

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