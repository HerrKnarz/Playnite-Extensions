using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyCompanion
{
    public class MergeCompanies : ViewModelBase
    {
        private ObservableCollection<MergeGroup> mergeList;

        public ObservableCollection<MergeGroup> MergeList
        {
            get
            {
                return mergeList;
            }
            set
            {
                mergeList = value;
                OnPropertyChanged("MergeList");
            }
        }

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
            MergeList = new ObservableCollection<MergeGroup>();
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

        public void GetMergeList(bool cleanUpName = false, bool findSimilar = false)
        {
            try
            {
                MergeList.Clear();

                List<MergeItem> companyList = API.Instance.Database.Companies
                    .Select(c => new MergeItem
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CleanedUpName = (cleanUpName) ? CleanUpCompanyName(c.Name) : c.Name,
                        GroupName = (findSimilar) ? RemoveWords(CleanUpCompanyName(c.Name), GenericDescriptors)
                            .RemoveDiacritics()
                            .RemoveSpecialChars()
                            .ToLower()
                            .Replace(" ", "") : c.Name,
                        Merge = true
                    }).OrderBy(c => c.CleanedUpName).ToList();

                IEnumerable<IGrouping<string, MergeItem>> mergeGroups;

                if (cleanUpName)
                {
                    mergeGroups = companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1 || g.First().Name != g.First().CleanedUpName);
                }
                else
                {
                    mergeGroups = companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1);
                }

                MergeList = mergeGroups.Select(g => new MergeGroup
                {
                    Owner = this,
                    Key = g.Key,
                    CompanyName = g.First().CleanedUpName,
                    CompanyId = g.First().Id,
                    Companies = g.ToList(),
                }).OrderBy(g => g.Key).ToList().ToObservable();

                foreach (MergeGroup group in mergeList)
                {
                    foreach (MergeItem item in group.Companies)
                    {
                        item.Owner = group;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "", true);
            }
        }

        public void Merge(MergeGroup mergeGroup = null)
        {
            int gameCount = 0;

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                   $"CompanyCompanion - updating companies",
                   true
               )
            {
                IsIndeterminate = false
            };

            List<Game> gameList;

            if (mergeGroup == null)
            {
                gameList = API.Instance.Database.Games.ToList();
            }
            else
            {
                gameList = API.Instance.Database.Games
                    .Where(g =>
                        (
                            g.DeveloperIds != null &&
                            g.DeveloperIds.Intersect(mergeGroup.Companies.Select(c => c.Id).ToList()).Any()
                        ) ||
                        (
                            g.PublisherIds != null &&
                            g.PublisherIds.Intersect(mergeGroup.Companies.Select(c => c.Id).ToList()).Any()
                        )
                    ).ToList();
            }

            ObservableCollection<MergeGroup> groups;

            if (mergeGroup == null)
            {
                groups = MergeList;
            }
            else
            {
                groups = new ObservableCollection<MergeGroup> { mergeGroup };
            }

            if (groups.Count > 0)
            {
                API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = gameList.Count;

                        foreach (Game game in gameList)
                        {
                            if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            bool mustUpdateGame = false;


                            foreach (MergeGroup group in groups)
                            {
                                mustUpdateGame = group.UpdateGame(game) || mustUpdateGame;
                            }


                            if (mustUpdateGame)
                            {
                                API.Instance.Database.Games.Update(game);

                                gameCount++;
                            }

                            activateGlobalProgress.CurrentProgressValue++;
                        }

                        foreach (MergeGroup group in groups)
                        {
                            group.CleanUpCompanies();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "", true);
                    }
                }, globalProgressOptions);
            }

            API.Instance.Dialogs.ShowMessage($"Updated companies in {gameCount} games and merged {groups.Select(g => g.Companies.Where(c => c.Merge).ToList().Count).Sum()} companies");

            groups = null;

            if (mergeGroup != null)
            {
                MergeList.Remove(mergeGroup);
            }

        }

        public static void MergeDuplicates()
        {
            MergeCompanies merger = new MergeCompanies();

            merger.GetMergeList();

            if (merger.MergeList.Count > 0)
            {
                merger.Merge();
            }
            else
            {
                API.Instance.Dialogs.ShowMessage("No duplicate companies were found.");
            }
        }
    }
}
