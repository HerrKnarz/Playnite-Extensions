using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

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
            if (oldName != null && oldName != newName && newName.DbObjectExists(type, id))
            {
                return DbInteractionResult.IsDuplicate;
            }

            UpdateDbObject(type, id, newName);

            return DbInteractionResult.Updated;
        }

        public static bool DbObjectExists(this string str, FieldType type, Guid id)
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

        public static void UpdateDbObject(FieldType type, Guid id, string name)
        {
            switch (type)
            {
                case FieldType.Category:
                    Category category = API.Instance.Database.Categories?.FirstOrDefault(x => x.Id == id);

                    if (category != null)
                    {
                        category.Name = name;
                        API.Instance.Database.Categories.Update(category);
                    }

                    return;
                case FieldType.Feature:
                    GameFeature feature = API.Instance.Database.Features?.FirstOrDefault(x => x.Id == id);

                    if (feature != null)
                    {
                        feature.Name = name;
                        API.Instance.Database.Features.Update(feature);
                    }

                    return;
                case FieldType.Genre:
                    Genre genre = API.Instance.Database.Genres?.FirstOrDefault(x => x.Id == id);

                    if (genre != null)
                    {
                        genre.Name = name;
                        API.Instance.Database.Genres.Update(genre);
                    }

                    return;
                case FieldType.Series:
                    Series series = API.Instance.Database.Series?.FirstOrDefault(x => x.Id == id);

                    if (series != null)
                    {
                        series.Name = name;
                        API.Instance.Database.Series.Update(series);
                    }

                    return;
                case FieldType.Tag:
                    Tag tag = API.Instance.Database.Tags?.FirstOrDefault(x => x.Id == id);

                    if (tag != null)
                    {
                        tag.Name = name;
                        API.Instance.Database.Tags.Update(tag);
                    }

                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void RemoveDbObject(FieldType type, Guid id) => ReplaceDbObject(type, id);

        public static void ReplaceDbObject(FieldType type, Guid id, FieldType? newType = null, Guid? newId = null)
        {
            switch (type)
            {
                case FieldType.Category:
                    foreach (Game game in API.Instance.Database.Games.Where(g => g.CategoryIds?.Any(t => t == id) ?? false))
                    {
                        if (game.CategoryIds.Remove(id))
                        {
                            if (newType != null && newId != null)
                            {
                                AddDbObject(game, (FieldType)newType, (Guid)newId);
                            }

                            API.Instance.Database.Games.Update(game);
                        }
                    }

                    API.Instance.Database.Categories.Remove(id);

                    break;
                case FieldType.Feature:
                    foreach (Game game in API.Instance.Database.Games.Where(g => g.FeatureIds?.Any(t => t == id) ?? false))
                    {
                        if (game.FeatureIds.Remove(id))
                        {
                            if (newType != null && newId != null)
                            {
                                AddDbObject(game, (FieldType)newType, (Guid)newId);
                            }

                            API.Instance.Database.Games.Update(game);
                        }
                    }

                    API.Instance.Database.Features.Remove(id);

                    break;
                case FieldType.Genre:
                    foreach (Game game in API.Instance.Database.Games.Where(g => g.GenreIds?.Any(t => t == id) ?? false))
                    {
                        if (game.GenreIds.Remove(id))
                        {
                            if (newType != null && newId != null)
                            {
                                AddDbObject(game, (FieldType)newType, (Guid)newId);
                            }

                            API.Instance.Database.Games.Update(game);
                        }
                    }

                    API.Instance.Database.Genres.Remove(id);

                    break;
                case FieldType.Series:
                    foreach (Game game in API.Instance.Database.Games.Where(g => g.SeriesIds?.Any(t => t == id) ?? false))
                    {
                        if (game.SeriesIds.Remove(id))
                        {
                            if (newType != null && newId != null)
                            {
                                AddDbObject(game, (FieldType)newType, (Guid)newId);
                            }

                            API.Instance.Database.Games.Update(game);
                        }
                    }

                    API.Instance.Database.Series.Remove(id);

                    break;
                case FieldType.Tag:
                    foreach (Game game in API.Instance.Database.Games.Where(g => g.TagIds?.Any(t => t == id) ?? false))
                    {
                        if (!game.TagIds.Remove(id))
                        {
                            continue;
                        }

                        if (newType != null && newId != null)
                        {
                            AddDbObject(game, (FieldType)newType, (Guid)newId);
                        }

                        API.Instance.Database.Games.Update(game);
                    }

                    API.Instance.Database.Tags.Remove(id);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool AddDbObject(Game game, FieldType type, Guid id)
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

            return ids?.AddMissing(id) ?? false;
        }
    }
}