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
    }
}