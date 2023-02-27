using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyCompanion
{
    /// <summary>
    /// Contains all relevant data to merge similar companies to one.
    /// </summary>
    public class MergeItem : ViewModelBase
    {
        private bool merge = true;

        /// <summary>
        /// Owner of the company. Used to set group name.
        /// </summary>
        public MergeGroup Owner { get; set; }
        /// <summary>
        /// Id of the company
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Original name of the company
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Name of the company after cleaning up
        /// </summary>
        public string CleanedUpName { get; set; }
        /// <summary>
        /// Name by which the companies will be grouped.
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// Info about the games from that company (number of games and names of the first 10)
        /// </summary>
        public string GameInfo { get; set; }
        /// <summary>
        /// Display name of the company. Includes info for cleaned up name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (Name == CleanedUpName)
                {
                    return Name;
                }
                else
                {
                    return $"{Name} → {CleanedUpName}";
                }
            }
        }
        /// <summary>
        /// Specifies if the company will be merged with the others in the group.
        /// </summary>
        public bool Merge
        {
            get
            {
                return merge;
            }
            set
            {
                merge = value;
                OnPropertyChanged("Merge");
            }
        }

        public RelayCommand UseAsNameCommand
        {
            get => new RelayCommand(() =>
            {
                Owner.CompanyName = CleanedUpName;
                Owner.CompanyId = Id;
            });
        }

        /// <summary>
        /// Fetches game infos for the company.
        /// </summary>
        public void PrepareGameInfo()
        {
            List<Game> gameList = API.Instance.Database.Games
                    .Where(g =>
                        (
                            g.DeveloperIds != null &&
                            g.DeveloperIds.Contains(Id)
                        ) ||
                        (
                            g.PublisherIds != null &&
                            g.PublisherIds.Contains(Id)
                        )
                    ).ToList();

            string games = string.Join(", ", gameList
                .OrderByDescending(g => g.Favorite)
                .ThenByDescending(g => g.Playtime)
                .ThenBy(g => g.SortingName)
                .Take(10)
                .Select(g => g.Name)
                .ToList());

            if (gameList.Count == 0)
            {
                GameInfo = $"0 {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGames")}";
            }
            else if (gameList.Count == 1)
            {
                GameInfo = $"1 {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGame")}: {games}";
            }
            else
            {
                GameInfo = $"{gameList.Count} {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGames")}: {games}";
            }
        }
    }
}