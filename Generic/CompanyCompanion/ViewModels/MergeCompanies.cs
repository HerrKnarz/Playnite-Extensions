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

        private readonly CompanyCompanion plugin;

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

        public MergeCompanies(CompanyCompanion plugin)
        {
            MergeList = new ObservableCollection<MergeGroup>();
            this.plugin = plugin;
        }

        internal string RemoveWords(string name, ObservableCollection<string> wordList)
        {
            if (name != null)
            {
                return string.Join(" ", name.Split().Where(w => !wordList.Contains(w.RemoveSpecialChars().Replace("-", ""), StringComparer.InvariantCultureIgnoreCase)));
            }
            else
            {
                return string.Empty;
            }

        }

        public string CleanUpCompanyName(string name)
        {
            name = RemoveWords(name, plugin.Settings.Settings.BusinessEntityDescriptors).CollapseWhitespaces().Trim();

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
                        GroupName = (findSimilar) ? RemoveWords(CleanUpCompanyName(c.Name), plugin.Settings.Settings.IgnoreWords)
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
                    Plugin = plugin,
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

        public static void MergeDuplicates(CompanyCompanion plugin)
        {
            MergeCompanies merger = new MergeCompanies(plugin);

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
        public static void RemoveBusinessEntityDescriptors(CompanyCompanion plugin)
        {
            MergeCompanies merger = new MergeCompanies(plugin);

            merger.GetMergeList(true);

            if (merger.MergeList.Count > 0)
            {
                merger.Merge();
            }
            else
            {
                API.Instance.Dialogs.ShowMessage("No companies with business entity descriptors found.");
            }
        }
    }
}
