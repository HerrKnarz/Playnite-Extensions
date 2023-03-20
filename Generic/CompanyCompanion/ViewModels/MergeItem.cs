using CompanyCompanion.ViewModels;
using Playnite.SDK;
using System;

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
        /// List of all games associated with the company as a developer
        /// </summary>
        public GameList GamesAsDeveloper { get; set; }
        /// <summary>
        /// List of all games associated with the company as a publisher
        /// </summary>
        public GameList GamesAsPublisher { get; set; }
        /// <summary>
        /// Display name of the company. Includes info for cleaned up name.
        /// </summary>
        public string DisplayName
        {
            get => (Name == CleanedUpName) ? Name : $"{Name} → {CleanedUpName}";
        }
        /// <summary>
        /// Specifies if the company will be merged with the others in the group.
        /// </summary>
        public bool Merge
        {
            get => merge;
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
            GamesAsDeveloper = new GameList(Id);
            GamesAsPublisher = new GameList(Id, false);
        }
    }
}