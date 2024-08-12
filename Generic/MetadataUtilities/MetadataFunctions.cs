using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Linq;
using KNARZhelper;
using MetadataUtilities.ViewModels;

namespace MetadataUtilities
{
    public static class MetadataFunctions
    {
        public static List<SettableMetadataObject> GetItemsFromAddDialog(SettableFieldType type, Settings settings)
        {
            MetadataObjects items = new MetadataObjects(settings);

            items.LoadMetadata(false, type);

            string label;

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    label = ResourceProvider.GetString("LOCAgeRatingsLabel");
                    break;

                case SettableFieldType.Category:
                    label = ResourceProvider.GetString("LOCCategoriesLabel");
                    break;

                case SettableFieldType.Feature:
                    label = ResourceProvider.GetString("LOCFeaturesLabel");
                    break;

                case SettableFieldType.Genre:
                    label = ResourceProvider.GetString("LOCGenresLabel");
                    break;

                case SettableFieldType.Series:
                    label = ResourceProvider.GetString("LOCSeriesLabel");
                    break;

                case SettableFieldType.Tag:
                    label = ResourceProvider.GetString("LOCTagsLabel");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Window window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<SettableMetadataObject>();
        }

        public static List<SettableMetadataObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            List<SettableMetadataObject> temporaryList = new List<SettableMetadataObject>();

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
                    temporaryList.AddRange(API.Instance.Database.AgeRatings
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.AgeRatingIds?.Contains(x.Id) ?? false)))
                        .Select(ageRating
                            => new SettableMetadataObject(settings)
                            {
                                Id = ageRating.Id,
                                Name = ageRating.Name,
                                Type = SettableFieldType.AgeRating
                            }));

                    temporaryList.AddRange(API.Instance.Database.Categories
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.CategoryIds?.Contains(x.Id) ?? false)))
                        .Select(category
                            => new SettableMetadataObject(settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = SettableFieldType.Category
                            }));

                    temporaryList.AddRange(API.Instance.Database.Features
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.FeatureIds?.Contains(x.Id) ?? false)))
                        .Select(feature
                            => new SettableMetadataObject(settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = SettableFieldType.Feature
                            }));

                    temporaryList.AddRange(API.Instance.Database.Genres
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.GenreIds?.Contains(x.Id) ?? false)))
                        .Select(genre
                            => new SettableMetadataObject(settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = SettableFieldType.Genre
                            }));

                    temporaryList.AddRange(API.Instance.Database.Series
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.SeriesIds?.Contains(x.Id) ?? false)))
                        .Select(series
                            => new SettableMetadataObject(settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = SettableFieldType.Series
                            }));

                    temporaryList.AddRange(API.Instance.Database.Tags
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.TagIds?.Contains(x.Id) ?? false)))
                        .Select(tag
                            => new SettableMetadataObject(settings)
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Type = SettableFieldType.Tag
                            }));

                    if (temporaryList.Any() && (settings.UnusedItemsWhiteList?.Any() ?? false))
                    {
                        temporaryList = temporaryList.Where(x =>
                            settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (SettableMetadataObject item in temporaryList)
                    {
                        DatabaseObjectHelper.RemoveDbObject(item.Type, item.Id, settings.IgnoreHiddenGamesInRemoveUnused);
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