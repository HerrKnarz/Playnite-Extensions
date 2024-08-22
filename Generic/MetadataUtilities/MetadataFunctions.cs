using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KNARZhelper.DatabaseObjectTypes;

namespace MetadataUtilities
{
    public static class MetadataFunctions
    {
        public static List<MetadataObject> GetItemsFromAddDialog(FieldType type, Settings settings)
        {
            string label = type.GetTypeManager().LabelPlural;

            MetadataObjects items = new MetadataObjects(settings, type);

            items.LoadMetadata(false);

            Window window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<MetadataObject>();
        }

        public static List<MetadataObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            List<MetadataObject> temporaryList = new List<MetadataObject>();

            //TODO: Add other types, once it is configurable, what the user wants to remove
            List<IDatabaseObjectType> types = new List<IDatabaseObjectType>
            {
                new TypeAgeRating(),
                new TypeCategory(),
                new TypeFeature(),
                new TypeGenre(),
                new TypeSeries(),
                new TypeTag()
            };

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCMetadataUtilitiesProgressRemovingUnused"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    foreach (IDatabaseObjectType type in types)
                    {
                        temporaryList.AddRange(type.LoadUnusedMetadata(settings.IgnoreHiddenGamesInRemoveUnused).Select(x
                            => new MetadataObject(settings)
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Type = type.Type
                            }));
                    }

                    if (temporaryList.Any() && (settings.UnusedItemsWhiteList?.Any() ?? false))
                    {
                        temporaryList = temporaryList.Where(x =>
                            settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (MetadataObject item in temporaryList)
                    {
                        item.RemoveFromDb(settings.IgnoreHiddenGamesInRemoveUnused);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            if (temporaryList.Count != 0)
            {
                if (autoMode)
                {
                    string items = string.Join(Environment.NewLine, temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));

                    API.Instance.Notifications.Add("MetadataUtilities",
                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        NotificationType.Info);
                }
                else
                {
                    string items = string.Join(", ", temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));
                    API.Instance.Dialogs.ShowMessage($"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (!autoMode)
            {
                API.Instance.Dialogs.ShowMessage(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoUnusedItemsFound"),
                    ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return temporaryList;
        }
    }
}