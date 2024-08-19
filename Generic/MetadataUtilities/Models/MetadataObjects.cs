using KNARZhelper;
using KNARZhelper.Enum;
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
using KNARZhelper.DatabaseObjectTypes;

namespace MetadataUtilities.Models
{
    public class MetadataObjects : ObservableCollection<MetadataObject>
    {
        private readonly IDatabaseObjectType _typeManager;

        /// <summary>
        /// Only used for deserializing the settings. Needs to use the "ResetReferences" method of
        /// the settings afterward!
        /// </summary>
        public MetadataObjects()
        { }

        public MetadataObjects(Settings settings, FieldType? type = null)
        {
            Settings = settings;
            _typeManager = type?.GetTypeManager();
        }

        [DontSerialize]
        public Settings Settings { get; set; }

        public void AddItems(FieldType type)
        {
            List<MetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(type, Settings);

            if (items.Count == 0)
            {
                return;
            }

            AddItems(items);
        }

        public void AddItems(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => this.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Add(new MetadataObject(Settings)
                {
                    Name = item.Name,
                    Type = item.Type
                });
            }

            this.Sort(x => x.TypeAndName);
        }

        public MetadataObject AddNewItem(FieldType type, string prefix = "", bool enableTypeSelection = true, bool addToDb = false)
        {
            MetadataObject newItem = new MetadataObject(Settings)
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
                newItem.AddToDb();
            }

            Add(newItem);

            this.Sort(x => x.TypeAndName);

            return newItem;
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
                        => new MetadataObject(Settings)
                        {
                            Id = ageRating.Id,
                            Name = ageRating.Name,
                            Type = FieldType.AgeRating
                        }));

                    temporaryList.AddRange(categories.Select(category
                        => new MetadataObject(Settings)
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Type = FieldType.Category
                        }));

                    temporaryList.AddRange(features.Select(feature
                        => new MetadataObject(Settings)
                        {
                            Id = feature.Id,
                            Name = feature.Name,
                            Type = FieldType.Feature
                        }));

                    temporaryList.AddRange(genres.Select(genre
                        => new MetadataObject(Settings)
                        {
                            Id = genre.Id,
                            Name = genre.Name,
                            Type = FieldType.Genre
                        }));

                    temporaryList.AddRange(seriesList.Select(series
                        => new MetadataObject(Settings)
                        {
                            Id = series.Id,
                            Name = series.Name,
                            Type = FieldType.Series
                        }));

                    temporaryList.AddRange(tags.Select(tag
                        => new MetadataObject(Settings)
                        {
                            Id = tag.Id,
                            Name = tag.Name,
                            Type = FieldType.Tag
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

        public void LoadMetadata(bool showGameNumber = true, bool onlyMergeAble = true)
        {
            Log.Debug("=== LoadMetadata: Start ===");
            DateTime ts = DateTime.Now;

            List<MetadataObject> temporaryList = new List<MetadataObject>();
            List<IDatabaseObjectType> types;

            if (_typeManager != null)
            {
                types = new List<IDatabaseObjectType> { _typeManager };
            }
            else
            {
                types = new List<IDatabaseObjectType>
                {
                    new TypeAgeRating(),
                    new TypeCategory(),
                    new TypeFeature(),
                    new TypeGenre(),
                    new TypeSeries(),
                    new TypeTag()
                };

                if (!onlyMergeAble)
                {
                    types.Add(new TypeDeveloper());
                    types.Add(new TypePlatform());
                    types.Add(new TypePublisher());
                    types.Add(new TypeSource());
                }
            }

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
                    foreach (IDatabaseObjectType typeManager in types)
                    {
                        temporaryList.AddRange(typeManager.LoadAllMetadata().Select(x => new MetadataObject(Settings)
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Type = typeManager.Type
                        }));
                    }

                    if (showGameNumber)
                    {
                        UpdateGameCounts(temporaryList, Settings.IgnoreHiddenGamesInGameCount, _typeManager, onlyMergeAble);
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

        public void RemoveItems(IEnumerable<MetadataObject> items)
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                Remove(item);
            }
        }

        private static void UpdateGameCounts(IEnumerable<MetadataObject> itemList, bool ignoreHiddenGames, IDatabaseObjectType typeManager = null, bool onlyMergeable = true)
        {
            Log.Debug("=== UpdateGameCounts: Start ===");
            DateTime ts = DateTime.Now;

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            if (typeManager != null)
            {
                Parallel.ForEach(itemList, opts, item => item.GetGameCount());

                return;
            }

            ConcurrentQueue<Guid> items = new ConcurrentQueue<Guid>();

            Parallel.ForEach(API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden)), opts, game =>
            {
                game.AgeRatingIds?.ForEach(o => items.Enqueue(o));
                game.CategoryIds?.ForEach(o => items.Enqueue(o));
                game.FeatureIds?.ForEach(o => items.Enqueue(o));
                game.GenreIds?.ForEach(o => items.Enqueue(o));
                game.SeriesIds?.ForEach(o => items.Enqueue(o));
                game.TagIds?.ForEach(o => items.Enqueue(o));

                if (onlyMergeable)
                {
                    return;
                }

                game.DeveloperIds?.ForEach(o => items.Enqueue(o));
                game.PlatformIds?.ForEach(o => items.Enqueue(o));
                game.PublisherIds?.ForEach(o => items.Enqueue(o));
                items.Enqueue(game.SourceId);
            });

            List<IGrouping<Guid, Guid>> li = items.GroupBy(i => i).ToList();

            Parallel.ForEach(itemList, opts, item => item.GameCount = li.FirstOrDefault(i => i.Key == item.Id)?.Count() ?? 0);

            Log.Debug($"=== UpdateGameCounts: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }
    }
}