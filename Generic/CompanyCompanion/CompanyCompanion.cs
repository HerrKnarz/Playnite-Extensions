using CompanyCompanion.Helper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace CompanyCompanion
{
    public class CompanyCompanion : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private CompanyCompanionSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("b76edaec-1aa8-48ef-83a4-2a49ab031029");

        public CompanyCompanion(IPlayniteAPI api) : base(api)
        {
            settings = new CompanyCompanionSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public void FindBusinessEntityDescriptors()
        {
            List<string> badWords = new List<string>()
            {
                "Co",
                "Corp",
                "GmbH",
                "Inc",
                "LLC",
                "Ltd",
                "srl",
                "sro",

            };

            // Für Duplikatsuche eventuell Dinge wie Studios, Games, Interactive, Productions, Multimedia, The, Digital, Corporation, Software, Entertainment, Publishing etc zum Suchen entfernen, damit die als potentielle Duplikate erscheinen.

            string names = string.Empty;

            foreach (Company c in API.Instance.Database.Companies)
            {
                string newName = c.Name;


                newName = string.Join(" ", newName.Split().Where(w => !badWords.Contains(w.RemoveSpecialChars().Replace("-", "").Replace(" ", ""), StringComparer.InvariantCultureIgnoreCase)));

                if (newName != c.Name)
                {
                    if (newName.EndsWith(","))
                    {
                        newName = newName.Substring(0, newName.Length - 1);
                    }

                    names += $"{c.Name} => {newName}{Environment.NewLine}";
                }
            }

            API.Instance.Dialogs.ShowMessage(names);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = "Find business entity descriptors",
                    MenuSection = $"@Company Companion",
                    Action = a =>
                    {
                        FindBusinessEntityDescriptors();
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
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CompanyCompanionSettingsView();
        }
    }
}