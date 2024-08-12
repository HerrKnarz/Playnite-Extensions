using KNARZhelper;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MetadataUtilities.Models
{
    public class MetadataObjects : ObservableCollection<SettableMetadataObject>
    {
        /// <summary>
        /// Only used for deserializing the settings. Needs to use the "ResetReferences" method of
        /// the settings afterwards!
        /// </summary>
        public MetadataObjects()
        { }

        public MetadataObjects(Settings settings) => Settings = settings;

        [DontSerialize]
        public Settings Settings { get; set; }

        public void AddItems(SettableFieldType type)
        {
            List<SettableMetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(type, Settings);

            if (items.Count == 0)
            {
                return;
            }

            AddItems(items);
        }

        public void AddItems(List<SettableMetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (SettableMetadataObject item in items.Where(item => this.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Add(new SettableMetadataObject(Settings)
                {
                    Name = item.Name,
                    Type = item.Type
                });
            }

            this.Sort(x => x.TypeAndName);
        }

        public SettableMetadataObject AddNewItem(SettableFieldType type, string prefix = "", bool enableTypeSelection = true, bool addToDb = false)
        {
            SettableMetadataObject newItem = new SettableMetadataObject(Settings)
            {
                Type = type,
                Prefix = prefix
            };

            Window window = AddNewObjectViewModel.GetWindow(Settings, newItem, enableTypeSelection);

            if (window == null)
            {
                return null;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return null;
            }

            if (this.Any(x => x.TypeAndName == newItem.TypeAndName))
            {
                return null;
            }

            if (addToDb)
            {
                newItem.Id = DatabaseObjectHelper.AddDbObject(newItem.Type, newItem.Name);
            }

            Add(newItem);

            this.Sort(x => x.TypeAndName);

            return newItem;
        }

        public void LoadGameMetadata(List<Game> games)
        {
            List<SettableMetadataObject> temporaryList = new List<SettableMetadataObject>();

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
                        => new SettableMetadataObject(Settings)
                        {
                            Id = ageRating.Id,
                            Name = ageRating.Name,
                            Type = SettableFieldType.AgeRating
                        }));

                    temporaryList.AddRange(categories.Select(category
                        => new SettableMetadataObject(Settings)
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Type = SettableFieldType.Category
                        }));

                    temporaryList.AddRange(features.Select(feature
                        => new SettableMetadataObject(Settings)
                        {
                            Id = feature.Id,
                            Name = feature.Name,
                            Type = SettableFieldType.Feature
                        }));

                    temporaryList.AddRange(genres.Select(genre
                        => new SettableMetadataObject(Settings)
                        {
                            Id = genre.Id,
                            Name = genre.Name,
                            Type = SettableFieldType.Genre
                        }));

                    temporaryList.AddRange(seriesList.Select(series
                        => new SettableMetadataObject(Settings)
                        {
                            Id = series.Id,
                            Name = series.Name,
                            Type = SettableFieldType.Series
                        }));

                    temporaryList.AddRange(tags.Select(tag
                        => new SettableMetadataObject(Settings)
                        {
                            Id = tag.Id,
                            Name = tag.Name,
                            Type = SettableFieldType.Tag
                        }));

                    UpdateGameCounts(temporaryList, Settings.IgnoreHiddenGamesInGameCount);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
        }

        public void LoadMetadata(bool showGameNumber = true, SettableFieldType? type = null)
        {
            Log.Debug("=== LoadMetadata: Start ===");
            DateTime ts = DateTime.Now;

            List<SettableMetadataObject> temporaryList = new List<SettableMetadataObject>();

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
                    if (type == null || type == SettableFieldType.AgeRating)
                    {
                        temporaryList.AddRange(API.Instance.Database.AgeRatings.Select(ageRating
                            => new SettableMetadataObject(Settings)
                            {
                                Id = ageRating.Id,
                                Name = ageRating.Name,
                                Type = SettableFieldType.AgeRating
                            }));
                    }

                    if (type == null || type == SettableFieldType.Category)
                    {
                        temporaryList.AddRange(API.Instance.Database.Categories.Select(category
                            => new SettableMetadataObject(Settings)
                            {
                                Id = category.Id,
                                Name = category.Name,
                                Type = SettableFieldType.Category
                            }));
                    }

                    if (type == null || type == SettableFieldType.Feature)
                    {
                        temporaryList.AddRange(API.Instance.Database.Features.Select(feature
                            => new SettableMetadataObject(Settings)
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                Type = SettableFieldType.Feature
                            }));
                    }

                    if (type == null || type == SettableFieldType.Genre)
                    {
                        temporaryList.AddRange(API.Instance.Database.Genres.Select(genre
                            => new SettableMetadataObject(Settings)
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                Type = SettableFieldType.Genre
                            }));
                    }

                    if (type == null || type == SettableFieldType.Series)
                    {
                        temporaryList.AddRange(API.Instance.Database.Series.Select(series
                            => new SettableMetadataObject(Settings)
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Type = SettableFieldType.Series
                            }));
                    }

                    if (type == null || type == SettableFieldType.Tag)
                    {
                        temporaryList.AddRange(API.Instance.Database.Tags.Select(tag
                            => new SettableMetadataObject(Settings)
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Type = SettableFieldType.Tag
                            }));
                    }

                    if (showGameNumber)
                    {
                        UpdateGameCounts(temporaryList, Settings.IgnoreHiddenGamesInGameCount);
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

        public void RemoveItems(IEnumerable<SettableMetadataObject> items)
        {
            foreach (SettableMetadataObject item in items.ToList().Cast<SettableMetadataObject>())
            {
                Remove(item);
            }
        }

        private static void UpdateGameCounts(IEnumerable<SettableMetadataObject> itemList, bool ignoreHiddenGames)
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
    }
}