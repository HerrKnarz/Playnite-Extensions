using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper
{
    public enum FieldType
    {
        Category,
        Feature,
        Genre,
        Series,
        Tag
    }

    public enum DbInteractionResult
    {
        Updated,
        Created,
        IsDuplicate,
        Error
    }

    public static class DatabaseObjectHelper
    {
        public static DbInteractionResult UpdateName(FieldType type, Guid id, string oldName, string newName)
        {
            if (oldName != null && oldName != newName && newName.NameExists(type, id))
            {
                return DbInteractionResult.IsDuplicate;
            }

            UpdateDbObject(type, id, newName);

            return DbInteractionResult.Updated;
        }

        public static bool NameExists(this string str, FieldType type, Guid id)
        {
            switch (type)
            {
                case FieldType.Category:
                    return API.Instance.Database.Categories?.Any(x => x.Name == str && x.Id != id) ?? false;
                case FieldType.Feature:
                    return API.Instance.Database.Features?.Any(x => x.Name == str && x.Id != id) ?? false;
                case FieldType.Genre:
                    return API.Instance.Database.Genres?.Any(x => x.Name == str && x.Id != id) ?? false;
                case FieldType.Series:
                    return API.Instance.Database.Series?.Any(x => x.Name == str && x.Id != id) ?? false;
                case FieldType.Tag:
                    return API.Instance.Database.Tags?.Any(x => x.Name == str && x.Id != id) ?? false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool DbObjectExists(string name, FieldType type)
        {
            switch (type)
            {
                case FieldType.Category:
                    return API.Instance.Database.Categories?.Any(x => x.Name == name) ?? false;
                case FieldType.Feature:
                    return API.Instance.Database.Features?.Any(x => x.Name == name) ?? false;
                case FieldType.Genre:
                    return API.Instance.Database.Genres?.Any(x => x.Name == name) ?? false;
                case FieldType.Series:
                    return API.Instance.Database.Series?.Any(x => x.Name == name) ?? false;
                case FieldType.Tag:
                    return API.Instance.Database.Tags?.Any(x => x.Name == name) ?? false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static Guid GetDbObjectId(string name, FieldType type)
        {
            DatabaseObject item;

            switch (type)
            {
                case FieldType.Category:
                    item = API.Instance.Database.Categories?.FirstOrDefault(x => x.Name == name);
                    break;
                case FieldType.Feature:
                    item = API.Instance.Database.Features?.FirstOrDefault(x => x.Name == name);
                    break;
                case FieldType.Genre:
                    item = API.Instance.Database.Genres?.FirstOrDefault(x => x.Name == name);
                    break;
                case FieldType.Series:
                    item = API.Instance.Database.Series?.FirstOrDefault(x => x.Name == name);
                    break;
                case FieldType.Tag:
                    item = API.Instance.Database.Tags?.FirstOrDefault(x => x.Name == name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return item?.Id ?? Guid.Empty;
        }

        public static void UpdateDbObject(FieldType type, Guid id, string name)
        {
            switch (type)
            {
                case FieldType.Category:
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
                case FieldType.Feature:
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
                case FieldType.Genre:
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
                case FieldType.Series:
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
                case FieldType.Tag:
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

        public static bool RemoveDbObject(FieldType type, Guid id, bool checkIfUsed = true)
        {
            if (checkIfUsed)
            {
                return ReplaceDbObject(API.Instance.Database.Games.ToList(), type, id)?.Count() > 0;
            }
            else
            {
                switch (type)
                {
                    case FieldType.Category:
                        API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Categories.Remove(id));
                        break;
                    case FieldType.Feature:
                        API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Features.Remove(id));
                        break;
                    case FieldType.Genre:
                        API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Genres.Remove(id));
                        break;
                    case FieldType.Series:
                        API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Series.Remove(id));
                        break;
                    case FieldType.Tag:
                        API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Tags.Remove(id));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                return false;
            }
        }

        public static IEnumerable<Guid> ReplaceDbObject(List<Game> games, FieldType type, Guid id, FieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            switch (type)
            {
                case FieldType.Category:
                    foreach (Game game in games.Where(g => g.CategoryIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.CategoryIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Categories.Remove(id);
                        });
                    }

                    break;
                case FieldType.Feature:
                    foreach (Game game in games.Where(g => g.FeatureIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.FeatureIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Features.Remove(id);
                        });
                    }

                    break;
                case FieldType.Genre:
                    foreach (Game game in games.Where(g => g.GenreIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.GenreIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Genres.Remove(id);
                        });
                    }

                    break;
                case FieldType.Series:
                    foreach (Game game in games.Where(g => g.SeriesIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.SeriesIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Series.Remove(id);
                        });
                    }

                    break;
                case FieldType.Tag:
                    foreach (Game game in games.Where(g => g.TagIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.TagIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObjectToGame(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Games.Update(game);
                        });

                        yield return game.Id;
                    }

                    if (removeAfter)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            API.Instance.Database.Tags.Remove(id);
                        });
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static Guid AddDbObject(FieldType type, string name)
        {
            switch (type)
            {
                case FieldType.Category:
                    Category category = API.Instance.Database.Categories.Add(name);

                    return category.Id;
                case FieldType.Feature:
                    GameFeature feature = API.Instance.Database.Features.Add(name);

                    return feature.Id;
                case FieldType.Genre:
                    Genre genre = API.Instance.Database.Genres.Add(name);

                    return genre.Id;
                case FieldType.Series:
                    Series series = API.Instance.Database.Series.Add(name);

                    return series.Id;
                case FieldType.Tag:
                    Tag tag = API.Instance.Database.Tags.Add(name);

                    return tag.Id;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool AddDbObjectToGame(Game game, FieldType type, Guid id)
        {
            List<Guid> ids;

            switch (type)
            {
                case FieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());
                    break;
                case FieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());
                    break;
                case FieldType.Genre:
                    ids = game.GenreIds ?? (game.GenreIds = new List<Guid>());
                    break;
                case FieldType.Series:
                    ids = game.SeriesIds ?? (game.SeriesIds = new List<Guid>());
                    break;
                case FieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return API.Instance.MainView.UIDispatcher.Invoke(() => ids?.AddMissing(id) ?? false);
        }

        public static bool AddDbObjectToGame(Game game, FieldType type, List<Guid> idList)
        {
            List<Guid> ids;

            switch (type)
            {
                case FieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());
                    break;
                case FieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());
                    break;
                case FieldType.Genre:
                    ids = game.GenreIds ?? (game.GenreIds = new List<Guid>());
                    break;
                case FieldType.Series:
                    ids = game.SeriesIds ?? (game.SeriesIds = new List<Guid>());
                    break;
                case FieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return API.Instance.MainView.UIDispatcher.Invoke(() => ids?.AddMissing(idList) ?? false);
        }

        public static bool AddDbObjectToGame(Game game, FieldType type, string name) => AddDbObjectToGame(game, type, AddDbObject(type, name));

        public static bool RemoveObjectFromGame(Game game, FieldType type, Guid id)
        {
            switch (type)
            {
                case FieldType.Category:
                    return API.Instance.MainView.UIDispatcher.Invoke(() => game.CategoryIds?.Remove(id) ?? false);
                case FieldType.Feature:
                    return API.Instance.MainView.UIDispatcher.Invoke(() => game.FeatureIds?.Remove(id) ?? false);
                case FieldType.Genre:
                    return API.Instance.MainView.UIDispatcher.Invoke(() => game.GenreIds?.Remove(id) ?? false);
                case FieldType.Series:
                    return API.Instance.MainView.UIDispatcher.Invoke(() => game.SeriesIds?.Remove(id) ?? false);
                case FieldType.Tag:
                    return API.Instance.MainView.UIDispatcher.Invoke(() => game.TagIds?.Remove(id) ?? false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}