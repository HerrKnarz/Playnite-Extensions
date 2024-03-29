﻿using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
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

        public void RenameTags()
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                "KNARZtools - updating tags",
                true
            )
            {
                IsIndeterminate = false
            };

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    List<Tag> tags = API.Instance.Database.Tags.Where(t => t.Name.StartsWith("🧑")).ToList();

                    activateGlobalProgress.ProgressMaxValue = tags.Count;

                    foreach (Tag tag in tags)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        string tagName = tag.Name;

                        tagName = tagName
                            .Replace(" director:", "🎬")
                            .Replace(" producer:", "🗓")
                            .Replace(" designer:", "🖼")
                            .Replace(" programmer:", "⌨️")
                            .Replace(" artist:", "🎨")
                            .Replace(" writer:", "📝")
                            .Replace(" composer:", "🎵");

                        if (tagName != tag.Name)
                        {
                            Tag existingTag = API.Instance.Database.Tags.FirstOrDefault(t => t.Name == tagName);

                            if (existingTag != null)
                            {
                                foreach (Game game in API.Instance.Database.Games.Where(g => (g.TagIds?.Any() ?? false) && g.Tags.Exists(t => t.Name == tag.Name)))
                                {
                                    game.TagIds.Remove(tag.Id);
                                    game.TagIds.AddMissing(existingTag.Id);
                                    API.Instance.Database.Games.Update(game);
                                }

                                API.Instance.Database.Tags.Remove(tag.Id);
                            }
                            else
                            {
                                tag.Name = tagName;
                                API.Instance.Database.Tags.Update(tag);
                            }
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

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                // Adds the "clean up" item to the main menu.
                new MainMenuItem
                {
                    Description = "Rename tags",
                    MenuSection = "KNARZtools",
                    Action = a => RenameTags()
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