using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CompanyCompanion
{
    public class CompanyCompanion : GenericPlugin
    {
        private CompanyCompanionSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("b76edaec-1aa8-48ef-83a4-2a49ab031029");

        public CompanyCompanion(IPlayniteAPI api) : base(api)
        {
            Settings = new CompanyCompanionSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public void ShowMergeView()
        {
            try
            {
                MergeCompaniesView mergeView = new MergeCompaniesView();
                Window window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
                {
                    ShowMinimizeButton = false,
                });

                window.Height = 800;
                window.Width = 800;
                window.Title = "Company Merger";
                window.Content = mergeView;
                window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            catch (Exception E)
            {
                Log.Error(E, "Error during initializing MergeCompaniesView", true);
            }
        }


        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = "Show Company Merger",
                    MenuSection = $"@Company Companion",
                    Action = a =>
                    {
                        ShowMergeView();
                    }
                },
                new MainMenuItem
                {
                    Description = "Merge duplicates",
                    MenuSection = $"@Company Companion",
                    Action = a =>
                    {
                        MergeCompanies.MergeDuplicates();
                    }
                }
            };

            return menuItems;
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CompanyCompanionSettingsView();
        }
    }
}