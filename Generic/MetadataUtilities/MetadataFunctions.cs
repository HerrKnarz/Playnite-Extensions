using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Linq;
using KNARZhelper;

namespace MetadataUtilities
{
    public class MetadataFunctions
    {
        public static List<MetadataObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            List<MetadataObject> temporaryList = new List<MetadataObject>();

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
                            => new MetadataObject(settings)
                            {
                                Id = ageRating.Id,
                                Name = ageRating.Name,
                                Type = FieldType.AgeRating
                            }));

                    temporaryList.AddRange(API.Instance.Database.Categories
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.CategoryIds?.Contains(x.Id) ?? false)))
                        .Select(category
                            => new MetadataObject(settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = FieldType.Category
                            }));

                    temporaryList.AddRange(API.Instance.Database.Features
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.FeatureIds?.Contains(x.Id) ?? false)))
                        .Select(feature
                            => new MetadataObject(settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = FieldType.Feature
                            }));

                    temporaryList.AddRange(API.Instance.Database.Genres
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.GenreIds?.Contains(x.Id) ?? false)))
                        .Select(genre
                            => new MetadataObject(settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = FieldType.Genre
                            }));

                    temporaryList.AddRange(API.Instance.Database.Series
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.SeriesIds?.Contains(x.Id) ?? false)))
                        .Select(series
                            => new MetadataObject(settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = FieldType.Series
                            }));

                    temporaryList.AddRange(API.Instance.Database.Tags
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.TagIds?.Contains(x.Id) ?? false)))
                        .Select(tag
                            => new MetadataObject(settings)
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Type = FieldType.Tag
                            }));

                    if (temporaryList.Any() && (settings.UnusedItemsWhiteList?.Any() ?? false))
                    {
                        temporaryList = temporaryList.Where(x =>
                            settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (MetadataObject item in temporaryList)
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