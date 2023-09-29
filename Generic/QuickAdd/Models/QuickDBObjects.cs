using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuickAdd.Models
{
    public class QuickDBObjects : ObservableCollection<QuickDBObject>
    {
        /// <summary>
        ///     Gets a collection of all database objects of a certain type
        /// </summary>
        /// <param name="checkedObjects"></param>
        /// <param name="type">Type of the database object</param>
        /// <returns>Collection of all of all database objects of a certain type</returns>
        internal static QuickDBObjects GetObjects(List<Guid> checkedObjects, FieldType type)
        {
            QuickDBObjects result = new QuickDBObjects();

            switch (type)
            {
                case FieldType.Category:
                    if (API.Instance.Database.Categories == null)
                    {
                        return result;
                    }

                    foreach (Category dbObject in API.Instance.Database.Categories.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, checkedObjects));
                    }

                    break;
                case FieldType.Feature:
                    if (API.Instance.Database.Features == null)
                    {
                        return result;
                    }

                    foreach (GameFeature dbObject in API.Instance.Database.Features.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, checkedObjects));
                    }

                    break;
                case FieldType.Tag:
                    if (API.Instance.Database.Tags == null)
                    {
                        return result;
                    }

                    foreach (Tag dbObject in API.Instance.Database.Tags.OrderBy(x => x.Name))
                    {
                        result.Add(CreateObject(dbObject, checkedObjects));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }

        internal static QuickDBObject CreateObject(DatabaseObject dbObject, List<Guid> checkedObjects)
        {
            return new QuickDBObject
            {
                Id = dbObject.Id,
                Name = dbObject.Name,
                Add = checkedObjects?.Any(x => x == dbObject.Id) ?? false
            };
        }
    }
}