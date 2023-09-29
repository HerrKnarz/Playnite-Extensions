using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuickAdd.Models
{
    public class QuickDbObjects : ObservableCollection<QuickDBObject>
    {
        /// <summary>
        ///     Gets a collection of all database objects of a certain type
        /// </summary>
        /// <param name="oldObjects"></param>
        /// <param name="type">Type of the database object</param>
        /// <returns>Collection of all of all database objects of a certain type</returns>
        internal static QuickDbObjects GetObjects(List<QuickDBObject> oldObjects, FieldType type)
        {
            QuickDbObjects result = new QuickDbObjects();

            switch (type)
            {
                case FieldType.Category:
                    if (API.Instance.Database.Categories == null)
                    {
                        return result;
                    }

                    foreach (Category dbObject in API.Instance.Database.Categories.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, oldObjects));
                    }

                    break;
                case FieldType.Feature:
                    if (API.Instance.Database.Features == null)
                    {
                        return result;
                    }

                    foreach (GameFeature dbObject in API.Instance.Database.Features.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, oldObjects));
                    }

                    break;
                case FieldType.Tag:
                    if (API.Instance.Database.Tags == null)
                    {
                        return result;
                    }

                    foreach (Tag dbObject in API.Instance.Database.Tags.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, oldObjects));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }

        internal static QuickDBObject CreateObject(DatabaseObject dbObject, List<QuickDBObject> oldObjects)
        {
            QuickDBObject oldObject = oldObjects?.FirstOrDefault(x => x.Id == dbObject.Id);

            return new QuickDBObject
            {
                Id = dbObject.Id,
                Name = dbObject.Name,
                Add = oldObject?.Add ?? false,
                Remove = oldObject?.Remove ?? false,
                Toggle = oldObject?.Toggle ?? false
            };
        }
    }
}