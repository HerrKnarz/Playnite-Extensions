using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public static class MetadataFunctions
    {
        public static List<MetadataObject> GetItemsFromAddDialog(FieldType type, Settings settings)
        {
            ObservableCollection<MetadataObject> items = LoadMetadata(type, settings);

            string label;

            switch (type)
            {
                case FieldType.AgeRating:
                    label = ResourceProvider.GetString("LOCAgeRatingsLabel");
                    break;

                case FieldType.Category:
                    label = ResourceProvider.GetString("LOCCategoriesLabel");
                    break;

                case FieldType.Developer:
                    label = ResourceProvider.GetString("LOCDevelopersLabel");
                    break;

                case FieldType.Feature:
                    label = ResourceProvider.GetString("LOCFeaturesLabel");
                    break;

                case FieldType.Genre:
                    label = ResourceProvider.GetString("LOCGenresLabel");
                    break;

                case FieldType.Platform:
                    label = ResourceProvider.GetString("LOCPlatformsTitle");
                    break;

                case FieldType.Publisher:
                    label = ResourceProvider.GetString("LOCPublishersLabel");
                    break;

                case FieldType.Series:
                    label = ResourceProvider.GetString("LOCSeriesLabel");
                    break;

                case FieldType.Source:
                    label = ResourceProvider.GetString("LOCSourcesLabel");
                    break;

                case FieldType.Tag:
                    label = ResourceProvider.GetString("LOCTagsLabel");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Window window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<MetadataObject>();
        }

        //TODO: See, if some things can be deleted and replaced with MetadataObjects class or put into Type classes

        public static ObservableCollection<MetadataObject> LoadMetadata(FieldType type, Settings settings)
        {
            List<MetadataObject> temporaryList = new List<MetadataObject>();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCLoadingLabel"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    switch (type)
                    {
                        case FieldType.AgeRating:
                            temporaryList.AddRange(API.Instance.Database.AgeRatings.Select(ageRating
                                => new MetadataObject(settings)
                                {
                                    Id = ageRating.Id,
                                    Name = ageRating.Name,
                                    Type = FieldType.AgeRating
                                }));
                            break;

                        case FieldType.Category:
                            temporaryList.AddRange(API.Instance.Database.Categories.Select(category
                                => new MetadataObject(settings)
                                {
                                    Id = category.Id,
                                    Name = category.Name,
                                    Type = FieldType.Category
                                }));
                            break;

                        case FieldType.Developer:
                            temporaryList.AddRange(API.Instance.Database.Companies.Select(company
                                => new MetadataObject(settings)
                                {
                                    Id = company.Id,
                                    Name = company.Name,
                                    Type = FieldType.Developer
                                }));
                            break;

                        case FieldType.Feature:
                            temporaryList.AddRange(API.Instance.Database.Features.Select(feature
                                => new MetadataObject(settings)
                                {
                                    Id = feature.Id,
                                    Name = feature.Name,
                                    Type = FieldType.Feature
                                }));
                            break;

                        case FieldType.Genre:
                            temporaryList.AddRange(API.Instance.Database.Genres.Select(genre
                                => new MetadataObject(settings)
                                {
                                    Id = genre.Id,
                                    Name = genre.Name,
                                    Type = FieldType.Genre
                                }));
                            break;

                        case FieldType.Platform:
                            temporaryList.AddRange(API.Instance.Database.Platforms.Select(platform
                                => new MetadataObject(settings)
                                {
                                    Id = platform.Id,
                                    Name = platform.Name,
                                    Type = FieldType.Developer
                                }));
                            break;

                        case FieldType.Publisher:
                            temporaryList.AddRange(API.Instance.Database.Companies.Select(company
                                => new MetadataObject(settings)
                                {
                                    Id = company.Id,
                                    Name = company.Name,
                                    Type = FieldType.Publisher
                                }));
                            break;

                        case FieldType.Series:
                            temporaryList.AddRange(API.Instance.Database.Series.Select(series
                                => new MetadataObject(settings)
                                {
                                    Id = series.Id,
                                    Name = series.Name,
                                    Type = FieldType.Series
                                }));
                            break;

                        case FieldType.Source:
                            temporaryList.AddRange(API.Instance.Database.Sources.Select(source
                                => new MetadataObject(settings)
                                {
                                    Id = source.Id,
                                    Name = source.Name,
                                    Type = FieldType.Source
                                }));
                            break;

                        case FieldType.Tag:
                            temporaryList.AddRange(API.Instance.Database.Tags.Select(tag
                                => new MetadataObject(settings)
                                {
                                    Id = tag.Id,
                                    Name = tag.Name,
                                    Type = FieldType.Tag
                                }));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            return temporaryList.OrderBy(x => x.TypeAndName).ToObservable();
        }

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