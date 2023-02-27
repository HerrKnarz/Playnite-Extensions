using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyCompanion
{
    /// <summary>
    /// Class to handle finding of company groups and merging them.
    /// </summary>
    public class MergeCompanies : ViewModelBase
    {
        private ObservableCollection<MergeGroup> mergeList;

        private readonly CompanyCompanion plugin;

        private List<Game> gameList;

        private ObservableCollection<MergeGroup> groups;

        private int gameCount = 0;

        /// <summary>
        /// List of business entity descriptors with special characters removed.
        /// </summary>
        private readonly List<string> cleanBusinessEntityDescriptors;

        /// <summary>
        /// Contains all companies that can be merged.
        /// </summary>
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

        /// <summary>
        /// Initializes the merge class.
        /// </summary>
        /// <param name="plugin">The plugin itself to have access to the settings etc.</param>
        public MergeCompanies(CompanyCompanion plugin)
        {
            MergeList = new ObservableCollection<MergeGroup>();
            this.plugin = plugin;

            cleanBusinessEntityDescriptors = plugin.Settings.Settings.BusinessEntityDescriptors.Select(w => w.RemoveSpecialChars().Replace("-", "")).ToList();
        }

        /// <summary>
        /// Removes words from the company name.
        /// </summary>
        /// <param name="name">Name to clean.</param>
        /// <param name="wordList">List of words to remove.</param>
        /// <returns>Name with the words removed</returns>
        internal string RemoveWords(string name, List<string> wordList)
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

        /// <summary>
        /// Cleans up a company name by removing business entity descriptors etc.
        /// </summary>
        /// <param name="name">Name of the company</param>
        /// <returns>cleaned up name</returns>
        public string CleanUpCompanyName(string name)
        {
            name = RemoveWords(name, cleanBusinessEntityDescriptors).CollapseWhitespaces().Trim();

            if (name.EndsWith(","))
            {
                name = name.Substring(0, name.Length - 1);
            }

            return name;
        }

        /// <summary>
        /// Creates the list of companies to merge.
        /// </summary>
        /// <param name="cleanUpName">True, if the company names should be cleaned up.</param>
        /// <param name="findSimilar">True, if also similar companies will be searched, where only words in the ignore list differ.</param>
        public void GetMergeList(bool cleanUpName = false, bool findSimilar = false)
        {
            List<MergeGroup> mergeList = new List<MergeGroup>();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                       $"{ResourceProvider.GetString("LOCCompanyCompanionName")} - {ResourceProvider.GetString("LOCCompanyCompanionProgressSearching")}",
                       false
                   )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    List<MergeItem> companyList = API.Instance.Database.Companies
                        .Select(c => new MergeItem
                        {
                            Id = c.Id,
                            Name = c.Name,
                            CleanedUpName = (cleanUpName) ? CleanUpCompanyName(c.Name) : c.Name,
                            GroupName = (findSimilar) ? RemoveWords(CleanUpCompanyName(c.Name), plugin.Settings.Settings.IgnoreWords.ToList())
                                .RemoveDiacritics()
                                .RemoveSpecialChars()
                                .ToLower()
                                .Replace(" ", "") : c.Name,
                            Merge = true
                        }).OrderBy(c => c.CleanedUpName).ToList();

                    companyList.RemoveAll(c => c.GroupName == "");

                    IEnumerable<IGrouping<string, MergeItem>> mergeGroups;

                    if (cleanUpName)
                    {
                        mergeGroups = companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1 || g.First().Name != g.First().CleanedUpName);
                    }
                    else
                    {
                        mergeGroups = companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1);
                    }

                    mergeList = mergeGroups.Select(g => new MergeGroup
                    {
                        Plugin = plugin,
                        Owner = this,
                        Key = g.Key,
                        CompanyName = g.First().CleanedUpName,
                        CompanyId = g.First().Id,
                        Companies = g.ToList(),
                    }).OrderBy(g => g.Key).ToList();

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
            }, globalProgressOptions);

            MergeList.Clear();
            MergeList = mergeList.ToObservable();
        }

        /// <summary>
        /// Merges the selected groups - used as a progress bar action.
        /// </summary>
        /// <param name="progressArgs">Arguments of the progress bar</param>
        internal void ProcessGroups(GlobalProgressActionArgs progressArgs)
        {
            try
            {
                progressArgs.ProgressMaxValue = gameList.Count;

                foreach (Game game in gameList)
                {
                    if (progressArgs.CancelToken.IsCancellationRequested)
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

                    progressArgs.CurrentProgressValue++;
                }

                progressArgs.CurrentProgressValue = 0;
                progressArgs.ProgressMaxValue = groups.Count;
                progressArgs.Text = $"{ResourceProvider.GetString("LOCCompanyCompanionName")} - {ResourceProvider.GetString("LOCCompanyCompanionProgressMerging")}";

                foreach (MergeGroup group in groups)
                {
                    group.CleanUpCompanies();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "", true);
            }
        }

        /// <summary>
        /// Merges the companies of a merge group.
        /// </summary>
        /// <param name="mergeGroup">Group to merge</param>
        public void Merge(MergeGroup mergeGroup = null)
        {
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
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                       $"{ResourceProvider.GetString("LOCCompanyCompanionName")} - {ResourceProvider.GetString("LOCCompanyCompanionProgressUpdating")}",
                       true
                   )
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(ProcessGroups, globalProgressOptions);
            }

            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCCompanyCompanionDialogUpdated"),
                gameCount,
                groups.Select(g => g.Companies.Where(c => c.Merge).ToList().Count).Sum()));

            groups = null;

            if (mergeGroup != null)
            {
                MergeList.Remove(mergeGroup);
            }

        }

        /// <summary>
        /// Merges all simple duplicates.
        /// </summary>
        /// <param name="plugin">The plugin itself to have access to the settings etc.</param>
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
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCCompanyCompanionDialogNoDuplicates"));
            }
        }

        /// <summary>
        /// Removes business entity descriptor from all companies.
        /// </summary>
        /// <param name="plugin">The plugin itself to have access to the settings etc.</param>
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
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCCompanyCompanionDialogNoDescriptors"));
            }
        }
    }
}
