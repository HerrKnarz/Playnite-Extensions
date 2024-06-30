using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetadataUtilities.Models
{
    public class MetadataObjects : ObservableCollection<MetadataObject>
    {
        private readonly Settings _settings;

        public MetadataObjects(Settings settings) => _settings = settings;

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

        public static void UpdateGameCounts(List<MetadataObject> itemList, bool ignoreHiddenGames)
        {
            Log.Debug("=== UpdateGameCounts: Start ===");
            DateTime ts = DateTime.Now;

            ConcurrentQueue<Guid> items = new ConcurrentQueue<Guid>();

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            Parallel.ForEach(API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden)), opts, game =>
            {
                game.AgeRatingIds?.ForEach(o => items.Enqueue(o));

                game.CategoryIds?.ForEach(o => items.Enqueue(o));

                game.FeatureIds?.ForEach(o => items.Enqueue(o));

                game.GenreIds?.ForEach(o => items.Enqueue(o));

                game.SeriesIds?.ForEach(o => items.Enqueue(o));

                game.TagIds?.ForEach(o => items.Enqueue(o));
            });

            List<IGrouping<Guid, Guid>> li = items.GroupBy(i => i).ToList();

            Parallel.ForEach(itemList, opts, item => item.GameCount = li.FirstOrDefault(i => i.Key == item.Id)?.Count() ?? 0);

            Log.Debug($"=== UpdateGameCounts: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }

        public static void UpdateGroupDisplay(List<MetadataObject> itemList)
        {
            Log.Debug("=== UpdateGroupDisplay: Start ===");
            DateTime ts = DateTime.Now;

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            Parallel.ForEach(itemList, opts, item => item.CheckGroup(itemList));

            Log.Debug($"=== UpdateGroupDisplay: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }

        public void LoadGameMetadata(List<Game> games)
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
                    List<AgeRating> ageRatings = new List<AgeRating>();
                    List<Category> categories = new List<Category>();
                    List<GameFeature> features = new List<GameFeature>();
                    List<Genre> genres = new List<Genre>();
                    List<Series> seriesList = new List<Series>();
                    List<Tag> tags = new List<Tag>();

                    foreach (Game game in games)
                    {
                        ageRatings.AddMissing(game.AgeRatings);
                        categories.AddMissing(game.Categories);
                        features.AddMissing(game.Features);
                        genres.AddMissing(game.Genres);
                        seriesList.AddMissing(game.Series);
                        tags.AddMissing(game.Tags);
                    }

                    temporaryList.AddRange(ageRatings.Select(ageRating
                        => new MetadataObject(_settings)
                        {
                            Id = ageRating.Id,
                            Name = ageRating.Name,
                            Type = FieldType.AgeRating
                        }));

                    temporaryList.AddRange(categories.Select(category
                        => new MetadataObject(_settings)
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Type = FieldType.Category
                        }));

                    temporaryList.AddRange(features.Select(feature
                        => new MetadataObject(_settings)
                        {
                            Id = feature.Id,
                            Name = feature.Name,
                            Type = FieldType.Feature
                        }));

                    temporaryList.AddRange(genres.Select(genre
                        => new MetadataObject(_settings)
                        {
                            Id = genre.Id,
                            Name = genre.Name,
                            Type = FieldType.Genre
                        }));

                    temporaryList.AddRange(seriesList.Select(series
                        => new MetadataObject(_settings)
                        {
                            Id = series.Id,
                            Name = series.Name,
                            Type = FieldType.Series
                        }));

                    temporaryList.AddRange(tags.Select(tag
                        => new MetadataObject(_settings)
                        {
                            Id = tag.Id,
                            Name = tag.Name,
                            Type = FieldType.Tag
                        }));

                    UpdateGameCounts(temporaryList, _settings.IgnoreHiddenGamesInGameCount);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
        }

        public void LoadMetadata(bool showGameNumber = true, FieldType? type = null)
        {
            Log.Debug("=== LoadMetadata: Start ===");
            DateTime ts = DateTime.Now;

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
                    if (type == null || type == FieldType.AgeRating)
                    {
                        temporaryList.AddRange(API.Instance.Database.AgeRatings.Select(ageRating
                            => new MetadataObject(_settings)
                            {
                                Id = ageRating.Id,
                                Name = ageRating.Name,
                                Type = FieldType.AgeRating
                            }));
                    }

                    if (type == null || type == FieldType.Category)
                    {
                        temporaryList.AddRange(API.Instance.Database.Categories.Select(category
                            => new MetadataObject(_settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = FieldType.Category
                            }));
                    }

                    if (type == null || type == FieldType.Feature)
                    {
                        temporaryList.AddRange(API.Instance.Database.Features.Select(feature
                            => new MetadataObject(_settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = FieldType.Feature
                            }));
                    }

                    if (type == null || type == FieldType.Genre)
                    {
                        temporaryList.AddRange(API.Instance.Database.Genres.Select(genre
                            => new MetadataObject(_settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = FieldType.Genre
                            }));
                    }

                    if (type == null || type == FieldType.Series)
                    {
                        temporaryList.AddRange(API.Instance.Database.Series.Select(series
                            => new MetadataObject(_settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = FieldType.Series
                            }));
                    }

                    if (type == null || type == FieldType.Tag)
                    {
                        temporaryList.AddRange(API.Instance.Database.Tags.Select(tag
                            => new MetadataObject(_settings)
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Type = FieldType.Tag
                            }));
                    }

                    if (showGameNumber)
                    {
                        UpdateGameCounts(temporaryList, _settings.IgnoreHiddenGamesInGameCount);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
            Log.Debug($"=== LoadMetadata: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }
    }
}