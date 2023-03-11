using Playnite.SDK;

namespace WikipediaMetadata.Models
{
    /// <summary>
    /// Search results for Wikipedia searches with added key property.
    /// </summary>
    internal class WikipediaItemOption : GenericItemOption
    {
        public string Key { get; set; }

        /// <summary>
        /// Gets a search item for the search dialog from a JSON page result
        /// </summary>
        /// <param name="page">found page</param>
        /// <returns>Search result for the search dialog.</returns>
        public static WikipediaItemOption FromWikipediaSearchResult(Page page)
        {
            return new WikipediaItemOption
            {
                Key = page.Key,
                Name = page.Title,
                Description = page.Description,
            };
        }
    }
}
