namespace KNARZhelper.Enum
{
    /// <summary>
    /// Result of a database interaction
    /// </summary>
    public enum DbInteractionResult
    {
        /// <summary>
        /// The database entry was updated
        /// </summary>
        Updated,
        /// <summary>
        /// The database entry was created
        /// </summary>
        Created,
        /// <summary>
        /// The database entry is a duplicate
        /// </summary>
        IsDuplicate,
        /// <summary>
        /// An error occurred during the database interaction
        /// </summary>
        Error
    }
}