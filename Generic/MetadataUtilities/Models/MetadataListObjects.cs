using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetadataUtilities.Models
{
    public class MetadataListObjects : ObservableCollection<MetadataListObject>
    {
        private readonly Settings _settings;

        public MetadataListObjects(Settings settings) => _settings = settings;

        public void LoadMetadata(bool showGameNumber = true, FieldType? type = null)
        {
            Log.Debug("=== LoadMetadata: Start ===");
            DateTime ts = DateTime.Now;

            List<MetadataListObject> temporaryList = new List<MetadataListObject>();

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
                    if (type == null || type == FieldType.Category)
                    {
                        temporaryList.AddRange(API.Instance.Database.Categories.Select(category
                            => new MetadataListObject(_settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = FieldType.Category
                            }));
                    }

                    if (type == null || type == FieldType.Feature)
                    {
                        temporaryList.AddRange(API.Instance.Database.Features.Select(feature
                            => new MetadataListObject(_settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = FieldType.Feature
                            }));
                    }

                    if (type == null || type == FieldType.Genre)
                    {
                        temporaryList.AddRange(API.Instance.Database.Genres.Select(genre
                            => new MetadataListObject(_settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = FieldType.Genre
                            }));
                    }

                    if (type == null || type == FieldType.Series)
                    {
                        temporaryList.AddRange(API.Instance.Database.Series.Select(series
                            => new MetadataListObject(_settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = FieldType.Series
                            }));
                    }

                    if (type == null || type == FieldType.Tag)
                    {
                        temporaryList.AddRange(API.Instance.Database.Tags.Select(tag
                            => new MetadataListObject(_settings)
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

        public static List<MetadataListObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            List<MetadataListObject> temporaryList = new List<MetadataListObject>();

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
                    temporaryList.AddRange(API.Instance.Database.Categories
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.CategoryIds?.Any(t => t == x.Id) ?? false)))
                        .Select(category
                            => new MetadataListObject(settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = FieldType.Category
                            }));

                    temporaryList.AddRange(API.Instance.Database.Features
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.FeatureIds?.Any(t => t == x.Id) ?? false)))
                        .Select(feature
                            => new MetadataListObject(settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = FieldType.Feature
                            }));

                    temporaryList.AddRange(API.Instance.Database.Genres
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.GenreIds?.Any(t => t == x.Id) ?? false)))
                        .Select(genre
                            => new MetadataListObject(settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = FieldType.Genre
                            }));

                    temporaryList.AddRange(API.Instance.Database.Series
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.SeriesIds?.Any(t => t == x.Id) ?? false)))
                        .Select(series
                            => new MetadataListObject(settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = FieldType.Series
                            }));

                    temporaryList.AddRange(API.Instance.Database.Tags
                        .Where(x => !API.Instance.Database.Games.Any(g
                            => !(settings.IgnoreHiddenGamesInRemoveUnused && g.Hidden) &&
                               (g.TagIds?.Any(t => t == x.Id) ?? false)))
                        .Select(tag
                            => new MetadataListObject(settings)
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Type = FieldType.Tag
                            }));

                    foreach (MetadataListObject item in temporaryList)
                    {
                        DatabaseObjectHelper.RemoveDbObject(item.Type, item.Id, settings.IgnoreHiddenGamesInRemoveUnused);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            if (temporaryList.Any())
            {
                string items = string.Join(Environment.NewLine, temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));

                if (autoMode)
                {
                    API.Instance.Notifications.Add("MetadataUtilities",
                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        NotificationType.Info);
                }
                else
                {
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

        public static void UpdateGameCounts(List<MetadataListObject> itemList, bool ignoreHiddenGames)
        {
            Log.Debug("=== UpdateGameCounts: Start ===");
            DateTime ts = DateTime.Now;

            ConcurrentQueue<Guid> items = new ConcurrentQueue<Guid>();

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            Parallel.ForEach(API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden)), opts, game =>
            {
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

        public static void UpdateGroupDisplay(List<MetadataListObject> itemList)
        {
            Log.Debug("=== UpdateGroupDisplay: Start ===");
            DateTime ts = DateTime.Now;

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            Parallel.ForEach(itemList, opts, item => item.CheckGroup(itemList));

            Log.Debug($"=== UpdateGroupDisplay: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }
    }
}