using CompanyCompanion.Models;
using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyCompanion
{
    public class MergeCompanies : ObservableObject
    {
        public ObservableCollection<MergeItem> MergeList { get; set; }

        internal List<string> BusinessEntityDescriptors { get; } = new List<string>()
        {
            "Co",
            "Corp",
            "GmbH",
            "Inc",
            "LLC",
            "Ltd",
            "srl",
            "sro",

        };

        internal List<string> GenericDescriptors { get; } = new List<string>()
        {
            "Corporation",
            "Digital",
            "Entertainment",
            "Games",
            "Interactive",
            "Multimedia",
            "Productions",
            "Publishing",
            "Software",
            "Studios",
            "The",
        };

        public MergeCompanies()
        {
            MergeList = new ObservableCollection<MergeItem>();
        }

        public void MergeGroup(string groupName)
        {

        }

        internal string RemoveWords(string name, List<string> wordList)
        {
            return string.Join(" ", name.Split().Where(w => !wordList.Contains(w.RemoveSpecialChars().Replace("-", ""), StringComparer.InvariantCultureIgnoreCase)));

        }

        public string CleanUpCompanyName(string name)
        {
            name = RemoveWords(name, BusinessEntityDescriptors).CollapseWhitespaces().Trim();

            if (name.EndsWith(","))
            {
                name = name.Substring(0, name.Length - 1);
            }

            return name;
        }

        public void FindMatches()
        {
            ObservableCollection<MergeItem> internalList = new ObservableCollection<MergeItem>();

            foreach (Company c in API.Instance.Database.Companies)
            {
                string cleanedUpName = CleanUpCompanyName(c.Name);

                internalList.Add(
                    new MergeItem
                    {
                        CompanyName = c.Name,
                        CleanedUpName = cleanedUpName,
                        Group = RemoveWords(cleanedUpName, GenericDescriptors)
                            .RemoveDiacritics()
                            .RemoveSpecialChars()
                            .ToLower()
                            .Replace(" ", ""),
                        Merge = true
                    }
                );
            }

            // Using a dictionary with all groups and the number of entries we can filter out all companies that unique.
            Dictionary<string, int> groups = internalList.GroupBy(x => x.Group)
              .Where(g => g.Count() > 1)
              .ToDictionary(x => x.Key, y => y.Count());


            MergeList.Clear();

            MergeList = new ObservableCollection<MergeItem>(
                internalList
                    .Where(x => x.CleanedUpName != x.CompanyName || (groups.ContainsKey(x.Group) && groups[x.Group] > 1))
                    .OrderBy(x => x.Group)
                    .ThenBy(x => x.CleanedUpName)
                    .ToList());

            foreach (MergeItem item in MergeList)
            {
                if (item == MergeList.Where(x => x.Group == item.Group).First())
                {
                    item.UseAsName = true;
                }
            }
        }
    }
}
