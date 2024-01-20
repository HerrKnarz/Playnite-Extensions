using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AdvancedMetadataTools
{
    public enum FieldType
    {
        Category,
        Feature,
        Genre,
        Tag
    }

    public class AdvancedMetadataTools : GenericPlugin
    {
        public AdvancedMetadataTools(IPlayniteAPI api) : base(api)
        {
            Settings = new AdvancedMetadataToolsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        private AdvancedMetadataToolsSettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        private void ShowManager()
        {
            try
            {
                MetadataManagerView managerView = new MetadataManagerView(this);

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCAdvancedMetadataToolsManager"), 1200, 800);

                window.Content = managerView;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing MetadataManager", true);
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCAdvancedMetadataToolsMenuManager"),
                    Action = a => ShowManager()
                }
            };

            return menuItems;
        }


        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
    }
}