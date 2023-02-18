using System;

namespace CompanyCompanion.Models
{
    /// <summary>
    /// Contains all relevant data to merge similar companies to one.
    /// </summary>
    public class MergeItem
    {
        /// <summary>
        /// String the companies are grouped by
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Name of the company
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Name of the company after cleaning up
        /// </summary>
        public string CleanedUpName { get; set; }
        /// <summary>
        /// Specifies if the company name will be used as the new name for the whole group.
        /// </summary>
        public Boolean UseAsName { get; set; } = false;
        /// <summary>
        /// Specifies if the company will be merged with the others in the group.
        /// </summary>
        public Boolean Merge { get; set; } = true;
    }
}