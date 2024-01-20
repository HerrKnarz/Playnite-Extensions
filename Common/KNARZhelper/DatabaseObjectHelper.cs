using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace KNARZhelper
{
    public enum FieldType
    {
        Category,
        Feature,
        Genre,
        Tag
    }

    public static class DatabaseObjectHelper
    {
        public static bool UpdateName(FieldType type, Guid id, string oldName, string newName)
        {
            if (oldName != null && oldName != newName && newName.DbObjectExists(type, id))
            {
                API.Instance.Dialogs.ShowMessage("A value with that name already exists.");
                return false;

                //TODO: Stattdessen Status zurückmelden und Dialog außerhalb aufrufen. AUßerdem Meldung übersetzbar machen und "Value" durch wirklichen Typen ersetzen. 
            }

            UpdateDbObject(type, id, newName);

            return true;
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
    }
}