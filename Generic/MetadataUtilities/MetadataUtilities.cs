using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities
{
    public class MetadataUtilities : GenericPlugin
    {
        public MetadataUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Dictionary<string, string> iconResourcesToAdd = new Dictionary<string, string>
            {
                { "muEditorIcon", "\xef10" }
            };

            foreach (KeyValuePair<string, string> iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        private SettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        private void ShowEditor()
        {
            try
            {
                MetadataEditorView editorView = new MetadataEditorView(this);

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesEditor"), 700, 600, false, true);

                window.Content = editorView;

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Metadata Editor", true);
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");

            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuEditor"),
                    MenuSection = $"@{menuSection}",
                    Icon = "muEditorIcon",
                    Action = a => ShowEditor()
                }
            };

            return menuItems;
        }


        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
    }
}