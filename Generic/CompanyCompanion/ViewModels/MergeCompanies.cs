﻿using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace CompanyCompanion
{
    /// <summary>
    /// Class to handle finding of company _groups and merging them.
    /// </summary>
    public class MergeCompanies : ViewModelBase
    {
        private ObservableCollection<MergeGroup> _mergeList;

        private readonly CompanyCompanion _plugin;

        private List<Game> _gameList;

        private ObservableCollection<MergeGroup> _groups;

        private int _gameCount = 0;

        private readonly Regex _companyFormRegex;

        /// <summary>
        /// Contains all companies that can be merged.
        /// </summary>
        public ObservableCollection<MergeGroup> MergeList
        {
            get => _mergeList;
            set
            {
                _mergeList = value;
                OnPropertyChanged("MergeList");
            }
        }

        /// <summary>
        /// Initializes the _merge class.
        /// </summary>
        /// <param name="plugin">The plugin itself to have access to the settings etc.</param>
        public MergeCompanies(CompanyCompanion plugin)
        {
            MergeList = new ObservableCollection<MergeGroup>();
            _plugin = plugin;

            string additionalStrings = plugin.Settings.Settings.BusinessEntityDescriptors.Any()
                ? "|" + string.Join("|", plugin.Settings.Settings.BusinessEntityDescriptors.Select(w => w.RemoveSpecialChars().ToLower().Replace("-", "")))
                : string.Empty;

            _companyFormRegex = new Regex(@",?\s+((co[,.\s]+)?ltd|(l\.)?inc|s\.?l|a[./]?s|limited|l\.?l\.?(c|p)|s\.?a(\.?r\.?l)?|s\.?r\.?o|gmbh|ab|corp|pte|ace|co|pty|pty\sltd|srl" + additionalStrings + @")\.?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Removes words from the company name.
        /// </summary>
        /// <param name="name">Name to clean.</param>
        /// <param name="wordList">List of words to remove.</param>
        /// <returns>Name with the words removed</returns>
        private static string RemoveWords(string name, IReadOnlyCollection<string> wordList)
        {
            return name != null
                ? string.Join(" ", name.Split().Where(w => !wordList.Contains(w.RemoveSpecialChars().Replace("-", ""), StringComparer.InvariantCultureIgnoreCase)))
                : string.Empty;
        }

        /// <summary>
        /// Cleans up a company name by removing business entity descriptors etc.
        /// </summary>
        /// <param name="name">Name of the company</param>
        /// <returns>cleaned up name</returns>
        private string CleanUpCompanyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            string newName = name;

            if (newName.EndsWith(")"))
            {
                newName = newName.Remove(newName.Length - 1);
            }

            newName = _companyFormRegex.Replace(newName, string.Empty).CollapseWhitespaces().Trim();

            if (name.EndsWith(")"))
            {
                newName = $"{newName})";
            }

            return newName;
        }

        /// <summary>
        /// Creates the list of companies to _merge.
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
                        .Where(c => !string.IsNullOrEmpty(c.Name))
                        .Select(c => new MergeItem
                        {
                            Id = c.Id,
                            Name = c.Name,
                            CleanedUpName = cleanUpName ? CleanUpCompanyName(c.Name) : c.Name,
                            GroupName = findSimilar
                                ? RemoveWords(CleanUpCompanyName(c.Name), _plugin.Settings.Settings.IgnoreWords.ToList())
                                    .RemoveDiacritics()
                                    .RemoveSpecialChars()
                                    .ToLower()
                                    .Replace(" ", "")
                                : c.Name,
                            Merge = true
                        }).OrderBy(c => c.CleanedUpName).ToList();

                    companyList.RemoveAll(c => c.GroupName == "");

                    foreach (MergeItem item in companyList)
                    {
                        item.PrepareGameInfo();
                    }

                    IEnumerable<IGrouping<string, MergeItem>> mergeGroups = cleanUpName
                        ? companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1 || g.First().Name != g.First().CleanedUpName)
                        : companyList.GroupBy(c => c.GroupName).Where(g => g.Count() > 1);

                    mergeList = mergeGroups.Select(g => new MergeGroup
                    {
                        Plugin = _plugin,
                        Owner = this,
                        Key = g.Key,
                        CompanyName =
                            g.OrderByDescending(
                                    c => c.GamesAsDeveloper.Games.Count + c.GamesAsPublisher.Games.Count)
                                .ThenBy(c => c.CleanedUpName).First().CleanedUpName,
                        CompanyId = g
                            .OrderByDescending(c => c.GamesAsDeveloper.Games.Count + c.GamesAsPublisher.Games.Count)
                            .ThenBy(c => c.CleanedUpName).First().Id,
                        Companies = g.ToList()
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
        /// Merges the selected _groups - used as a progress bar action.
        /// </summary>
        /// <param name="progressArgs">Arguments of the progress bar</param>
        private void ProcessGroups(GlobalProgressActionArgs progressArgs)
        {
            try
            {
                progressArgs.ProgressMaxValue = _gameList.Count;

                foreach (Game game in _gameList)
                {
                    if (progressArgs.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    bool mustUpdateGame = false;

                    foreach (MergeGroup group in _groups)
                    {
                        mustUpdateGame |= group.UpdateGame(game);
                    }

                    if (mustUpdateGame)
                    {
                        API.Instance.Database.Games.Update(game);

                        _gameCount++;
                    }

                    progressArgs.CurrentProgressValue++;
                }

                progressArgs.CurrentProgressValue = 0;
                progressArgs.ProgressMaxValue = _groups.Count;
                progressArgs.Text = $"{ResourceProvider.GetString("LOCCompanyCompanionName")} - {ResourceProvider.GetString("LOCCompanyCompanionProgressMerging")}";

                foreach (MergeGroup group in _groups)
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
        /// Merges the companies of a _merge group.
        /// </summary>
        /// <param name="mergeGroup">Group to _merge</param>
        public void Merge(MergeGroup mergeGroup = null)
        {
            _gameCount = 0;

            _gameList = mergeGroup is null
                ? API.Instance.Database.Games.ToList()
                : API.Instance.Database.Games
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

            _groups = mergeGroup is null ? MergeList : new ObservableCollection<MergeGroup> { mergeGroup };

            if (_groups.Any())
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
                _gameCount,
                _groups.Select(g => g.Companies.Where(c => c.Merge).ToList().Count).Sum()));

            _groups = null;

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

            if (merger.MergeList.Any())
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

            if (merger.MergeList.Any())
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