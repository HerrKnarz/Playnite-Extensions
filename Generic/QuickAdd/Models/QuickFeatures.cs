using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QuickAdd.Models
{
    public class QuickFeatures : ObservableCollection<QuickFeature>
    {
        /// <summary>
        ///     Gets a collection of all features
        /// </summary>
        /// <returns>Collection of all features</returns>
        internal static QuickFeatures GetFeatures(List<Guid> checkedFeatures)
        {
            QuickFeatures result = new QuickFeatures();

            if (API.Instance.Database.Features == null)
            {
                return result;
            }

            foreach (GameFeature feature in API.Instance.Database.Features.OrderBy(x => x.Name))
            {
                QuickFeature newFeature = new QuickFeature
                {
                    Id = feature.Id,
                    Name = feature.Name,
                    Add = checkedFeatures?.Any(x => x == feature.Id) ?? false
                };

                result.Add(newFeature);
            }

            return result;
        }
    }
}