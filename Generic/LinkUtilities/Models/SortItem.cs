namespace LinkUtilities.Models
{
    /// <summary>
    /// Defines a position of a link name in a sort order.
    /// </summary>
    public class SortItem
    {
        /// <summary>
        /// Name of the link
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// Position the name will be sorted to
        /// </summary>
        public int Position { get; set; }
    }
}