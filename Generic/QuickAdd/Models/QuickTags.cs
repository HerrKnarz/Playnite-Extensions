using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuickAdd.Models
{
    public class QuickTags : ObservableCollection<QuickTag>
    {
        /// <summary>
        ///     Gets a collection of all tags
        /// </summary>
        /// <returns>Collection of all tags</returns>
        internal static QuickTags GetTags(List<Guid> checkedTags)
        {
            QuickTags result = new QuickTags();

            if (API.Instance.Database.Tags == null)
            {
                return result;
            }

            foreach (Tag tag in API.Instance.Database.Tags.OrderBy(x => x.Name))
            {
                QuickTag newTag = new QuickTag
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Add = checkedTags?.Any(x => x == tag.Id) ?? false
                };

                result.Add(newTag);
            }

            return result;
        }
    }
}