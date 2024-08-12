using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper
{
    public enum DbInteractionResult
    {
        Updated,
        Created,
        IsDuplicate,
        Error
    }

    public enum FieldType
    {
        AgeRating = 5,
        Category = 0,
        Developer = 6,
        Feature = 1,
        Genre = 2,
        Platform = 7,
        Publisher = 8,
        Series = 3,
        Source = 9,
        Tag = 4,
    }

    public enum SettableFieldType
    {
        AgeRating = FieldType.AgeRating,
        Category = FieldType.Category,
        Feature = FieldType.Feature,
        Genre = FieldType.Genre,
        Series = FieldType.Series,
        Tag = FieldType.Tag,
    }

    public static class DatabaseObjectHelper
    {
        public static Guid AddDbObject(SettableFieldType type, string name)
        {
            switch (type)
            {
                case SettableFieldType.AgeRating:
                    AgeRating ageRating = API.Instance.Database.AgeRatings.Add(name);

                    return ageRating.Id;

                case SettableFieldType.Category:
                    Category category = API.Instance.Database.Categories.Add(name);

                    return category.Id;

                case SettableFieldType.Feature:
                    GameFeature feature = API.Instance.Database.Features.Add(name);

                    return feature.Id;

                case SettableFieldType.Genre:
                    Genre genre = API.Instance.Database.Genres.Add(name);

                    return genre.Id;

                case SettableFieldType.Series:
                    Series series = API.Instance.Database.Series.Add(name);

                    return series.Id;

                case SettableFieldType.Tag:
                    Tag tag = API.Instance.Database.Tags.Add(name);

                    return tag.Id;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool AddDbObjectToGame(Game game, SettableFieldType type, Guid id)
        {
            List<Guid> ids;

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    ids = game.AgeRatingIds ?? (game.AgeRatingIds = new List<Guid>());
                    break;

                case SettableFieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());
                    break;

                case SettableFieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());
                    break;

                case SettableFieldType.Genre:
                    ids = game.GenreIds ?? (game.GenreIds = new List<Guid>());
                    break;

                case SettableFieldType.Series:
                    ids = game.SeriesIds ?? (game.SeriesIds = new List<Guid>());
                    break;

                case SettableFieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return API.Instance.MainView.UIDispatcher.Invoke(() => ids?.AddMissing(id) ?? false);
        }

        public static bool AddDbObjectToGame(Game game, SettableFieldType type, List<Guid> idList)
        {
            List<Guid> ids;

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    ids = game.AgeRatingIds ?? (game.AgeRatingIds = new List<Guid>());
                    break;

                case SettableFieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());
                    break;

                case SettableFieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());
                    break;

                case SettableFieldType.Genre:
                    ids = game.GenreIds ?? (game.GenreIds = new List<Guid>());
                    break;

                case SettableFieldType.Series:
                    ids = game.SeriesIds ?? (game.SeriesIds = new List<Guid>());
                    break;

                case SettableFieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return API.Instance.MainView.UIDispatcher.Invoke(() => ids?.AddMissing(idList) ?? false);
        }

        public static bool AddDbObjectToGame(Game game, SettableFieldType type, string name) => AddDbObjectToGame(game, type, AddDbObject(type, name));

        public static bool DbObjectExists(string name, SettableFieldType type)
        {
            switch (type)
            {
                case SettableFieldType.AgeRating:
                    return API.Instance.Database.AgeRatings?.Any(x => x.Name == name) ?? false;

                case SettableFieldType.Category:
                    return API.Instance.Database.Categories?.Any(x => x.Name == name) ?? false;

                case SettableFieldType.Feature:
                    return API.Instance.Database.Features?.Any(x => x.Name == name) ?? false;

                case SettableFieldType.Genre:
                    return API.Instance.Database.Genres?.Any(x => x.Name == name) ?? false;

                case SettableFieldType.Series:
                    return API.Instance.Database.Series?.Any(x => x.Name == name) ?? false;

                case SettableFieldType.Tag:
                    return API.Instance.Database.Tags?.Any(x => x.Name == name) ?? false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool DbObjectInGame(Game game, FieldType type, Guid id)
        {
            switch (type)
            {
                case FieldType.AgeRating:
                    return game.AgeRatingIds?.Contains(id) ?? false;

                case FieldType.Category:
                    return game.CategoryIds?.Contains(id) ?? false;

                case FieldType.Feature:
                    return game.FeatureIds?.Contains(id) ?? false;

                case FieldType.Developer:
                    return game.DeveloperIds?.Contains(id) ?? false;

                case FieldType.Genre:
                    return game.GenreIds?.Contains(id) ?? false;

                case FieldType.Platform:
                    return game.PlatformIds?.Contains(id) ?? false;

                case FieldType.Publisher:
                    return game.PublisherIds?.Contains(id) ?? false;

                case FieldType.Series:
                    return game.SeriesIds?.Contains(id) ?? false;

                case FieldType.Source:
                    return game.Source != null && game.Source.Id == id;

                case FieldType.Tag:
                    return game.TagIds?.Contains(id) ?? false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool DbObjectInUse(SettableFieldType type, Guid id)
        {
            switch (type)
            {
                case SettableFieldType.AgeRating:
                    return API.Instance.Database.Games.Any(x => x.AgeRatingIds?.Contains(id) ?? false);

                case SettableFieldType.Category:
                    return API.Instance.Database.Games.Any(x => x.CategoryIds?.Contains(id) ?? false);

                case SettableFieldType.Feature:
                    return API.Instance.Database.Games.Any(x => x.FeatureIds?.Contains(id) ?? false);

                case SettableFieldType.Genre:
                    return API.Instance.Database.Games.Any(x => x.GenreIds?.Contains(id) ?? false);

                case SettableFieldType.Series:
                    return API.Instance.Database.Games.Any(x => x.SeriesIds?.Contains(id) ?? false);

                case SettableFieldType.Tag:
                    return API.Instance.Database.Games.Any(x => x.TagIds?.Contains(id) ?? false);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static Guid GetDbObjectId(string name, FieldType type)
        {
            DatabaseObject item;

            switch (type)
            {
                case FieldType.AgeRating:
                    item = API.Instance.Database.AgeRatings?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Category:
                    item = API.Instance.Database.Categories?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Developer:
                    item = API.Instance.Database.Companies?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Feature:
                    item = API.Instance.Database.Features?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Genre:
                    item = API.Instance.Database.Genres?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Platform:
                    item = API.Instance.Database.Platforms?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Publisher:
                    item = API.Instance.Database.Companies?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Series:
                    item = API.Instance.Database.Series?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Source:
                    item = API.Instance.Database.Sources?.FirstOrDefault(x => x.Name == name);
                    break;

                case FieldType.Tag:
                    item = API.Instance.Database.Tags?.FirstOrDefault(x => x.Name == name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return item?.Id ?? Guid.Empty;
        }

        public static int GetGameCount(FieldType type, Guid id, bool ignoreHidden = false)
        {
            switch (type)
            {
                case FieldType.AgeRating:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.AgeRatingIds?.Contains(id) ?? false));

                case FieldType.Category:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.CategoryIds?.Contains(id) ?? false));

                case FieldType.Developer:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.DeveloperIds?.Contains(id) ?? false));

                case FieldType.Feature:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.FeatureIds?.Contains(id) ?? false));

                case FieldType.Genre:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.GenreIds?.Contains(id) ?? false));

                case FieldType.Platform:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.PlatformIds?.Contains(id) ?? false));

                case FieldType.Publisher:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.PublisherIds?.Contains(id) ?? false));

                case FieldType.Series:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.SeriesIds?.Contains(id) ?? false));

                case FieldType.Source:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) && g.Source != null && g.Source.Id == id);

                case FieldType.Tag:
                    return API.Instance.Database.Games.Count(g
                        => !(ignoreHidden && g.Hidden) &&
                           (g.TagIds?.Contains(id) ?? false));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool NameExists(this string str, SettableFieldType type, Guid id)
        {
            switch (type)
            {
                case SettableFieldType.AgeRating:
                    return API.Instance.Database.AgeRatings?.Any(x => x.Name == str && x.Id != id) ?? false;

                case SettableFieldType.Category:
                    return API.Instance.Database.Categories?.Any(x => x.Name == str && x.Id != id) ?? false;

                case SettableFieldType.Feature:
                    return API.Instance.Database.Features?.Any(x => x.Name == str && x.Id != id) ?? false;

                case SettableFieldType.Genre:
                    return API.Instance.Database.Genres?.Any(x => x.Name == str && x.Id != id) ?? false;

                case SettableFieldType.Series:
                    return API.Instance.Database.Series?.Any(x => x.Name == str && x.Id != id) ?? false;

                case SettableFieldType.Tag:
                    return API.Instance.Database.Tags?.Any(x => x.Name == str && x.Id != id) ?? false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool RemoveDbObject(SettableFieldType type, Guid id, bool checkIfUsed = true)
        {
            if (checkIfUsed)
            {
                return ReplaceDbObject(API.Instance.Database.Games.ToList(), type, id)?.Count() > 0;
            }

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.AgeRatings.Remove(id));
                    break;

                case SettableFieldType.Category:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Categories.Remove(id));
                    break;

                case SettableFieldType.Feature:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Features.Remove(id));
                    break;

                case SettableFieldType.Genre:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Genres.Remove(id));
                    break;

                case SettableFieldType.Series:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Series.Remove(id));
                    break;

                case SettableFieldType.Tag:
                    API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Tags.Remove(id));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return false;
        }

        public static bool RemoveObjectFromGame(Game game, SettableFieldType type, List<Guid> ids)
        {
            if (ids.Count == 0)
            {
                return false;
            }

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.AgeRatingIds?.Remove(id) ?? false));

                case SettableFieldType.Category:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.CategoryIds?.Remove(id) ?? false));

                case SettableFieldType.Feature:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.FeatureIds?.Remove(id) ?? false));

                case SettableFieldType.Genre:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.GenreIds?.Remove(id) ?? false));

                case SettableFieldType.Series:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.SeriesIds?.Remove(id) ?? false));

                case SettableFieldType.Tag:
                    return ids.Aggregate(false, (current, id) => current | API.Instance.MainView.UIDispatcher.Invoke(() => game.TagIds?.Remove(id) ?? false));

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static IEnumerable<Guid> ReplaceDbObject(List<Game> games, SettableFieldType type, Guid id, SettableFieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            switch (type)
            {
                case SettableFieldType.AgeRating:
                    foreach (Game game in games.Where(g => g.AgeRatingIds?.Contains(id) ?? false))
                    {
                        if (!game.AgeRatingIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.AgeRating, id))
                    {
                        RemoveDbObject(SettableFieldType.AgeRating, id, false);
                    }

                    break;

                case SettableFieldType.Category:
                    foreach (Game game in games.Where(g => g.CategoryIds?.Contains(id) ?? false))
                    {
                        if (!game.CategoryIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.Category, id))
                    {
                        RemoveDbObject(SettableFieldType.Category, id, false);
                    }

                    break;

                case SettableFieldType.Feature:
                    foreach (Game game in games.Where(g => g.FeatureIds?.Contains(id) ?? false))
                    {
                        if (!game.FeatureIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.Feature, id))
                    {
                        RemoveDbObject(SettableFieldType.Feature, id, false);
                    }

                    break;

                case SettableFieldType.Genre:
                    foreach (Game game in games.Where(g => g.GenreIds?.Contains(id) ?? false))
                    {
                        if (!game.GenreIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.Genre, id))
                    {
                        RemoveDbObject(SettableFieldType.Genre, id, false);
                    }

                    break;

                case SettableFieldType.Series:
                    foreach (Game game in games.Where(g => g.SeriesIds?.Contains(id) ?? false))
                    {
                        if (!game.SeriesIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.Series, id))
                    {
                        RemoveDbObject(SettableFieldType.Series, id, false);
                    }

                    break;

                case SettableFieldType.Tag:
                    foreach (Game game in games.Where(g => g.TagIds?.Contains(id) ?? false))
                    {
                        if (!game.TagIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (SettableFieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter && !DbObjectInUse(SettableFieldType.Tag, id))
                    {
                        RemoveDbObject(SettableFieldType.Tag, id, false);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void UpdateDbObject(SettableFieldType type, Guid id, string name)
        {
            switch (type)
            {
                case SettableFieldType.AgeRating:
                    AgeRating ageRating = API.Instance.Database.AgeRatings?.FirstOrDefault(x => x.Id == id);

                    if (ageRating == null)
                    {
                        return;
                    }

                    ageRating.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.AgeRatings.Update(ageRating);
                    });

                    return;

                case SettableFieldType.Category:
                    Category category = API.Instance.Database.Categories?.FirstOrDefault(x => x.Id == id);

                    if (category == null)
                    {
                        return;
                    }

                    category.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.Categories.Update(category);
                    });

                    return;

                case SettableFieldType.Feature:
                    GameFeature feature = API.Instance.Database.Features?.FirstOrDefault(x => x.Id == id);

                    if (feature == null)
                    {
                        return;
                    }

                    feature.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.Features.Update(feature);
                    });

                    return;

                case SettableFieldType.Genre:
                    Genre genre = API.Instance.Database.Genres?.FirstOrDefault(x => x.Id == id);

                    if (genre == null)
                    {
                        return;
                    }

                    genre.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.Genres.Update(genre);
                    });

                    return;

                case SettableFieldType.Series:
                    Series series = API.Instance.Database.Series?.FirstOrDefault(x => x.Id == id);

                    if (series == null)
                    {
                        return;
                    }

                    series.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.Series.Update(series);
                    });

                    return;

                case SettableFieldType.Tag:
                    Tag tag = API.Instance.Database.Tags?.FirstOrDefault(x => x.Id == id);

                    if (tag == null)
                    {
                        return;
                    }

                    tag.Name = name;

                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        API.Instance.Database.Tags.Update(tag);
                    });

                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static DbInteractionResult UpdateName(SettableFieldType type, Guid id, string oldName, string newName)
        {
            if (oldName != null && oldName != newName && newName.NameExists(type, id))
            {
                return DbInteractionResult.IsDuplicate;
            }

            UpdateDbObject(type, id, newName);

            return DbInteractionResult.Updated;
        }
    }
}