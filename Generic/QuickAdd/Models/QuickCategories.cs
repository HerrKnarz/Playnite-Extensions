using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuickAdd.Models
{
    public class QuickCategories : ObservableCollection<QuickCategory>
    {
        /// <summary>
        ///     Gets a collection of all categories
        /// </summary>
        /// <returns>Collection of all categories</returns>
        internal static QuickCategories GetCategories(List<Guid> checkedCategories)
        {
            QuickCategories result = new QuickCategories();

            if (API.Instance.Database.Categories == null)
            {
                return result;
            }

            foreach (Category category in API.Instance.Database.Categories.OrderBy(x => x.Name))
            {
                QuickCategory newCategory = new QuickCategory
                {
                    Id = category.Id,
                    Name = category.Name,
                    Checked = checkedCategories?.Any(x => x == category.Id) ?? false
                };

                result.Add(newCategory);
            }

            return result;
        }
    }
}