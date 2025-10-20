using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace KNARZtools
{
    // ReSharper disable once InconsistentNaming
    public class KNARZtools : GenericPlugin
    {
        private KNARZtoolsSettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("f36aaef9-9f87-40ad-a2b5-40e50bf56b95");

        public KNARZtools(IPlayniteAPI api) : base(api)
        {
            Settings = new KNARZtoolsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public void GetFamilySharedIds()
        {
            var idList = new List<string>();

            foreach (var game in API.Instance.Database.Games.Where(g => g.Categories?.Any(c => c.Name.Equals("Family Shared")) ?? false).ToList())
            {
                idList.Add($"{SteamHelper.GetSteamId(game)};{game.Name}\n");
            }

            API.Instance.Dialogs.ShowSelectableString("", "Family Sharing Ids", string.Concat(idList));
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            var menuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Get family shared games",
                    MenuSection = "@KNARZtools",
                    Action = a => GetFamilySharedIds()
                }
            };

            return menuItems;
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
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
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new KNARZtoolsSettingsView();
    }
}