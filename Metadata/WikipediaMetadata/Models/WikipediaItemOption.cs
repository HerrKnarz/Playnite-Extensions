using Playnite.SDK;

namespace WikipediaMetadata.Models
{
    internal class WikipediaItemOption : GenericItemOption
    {
        public string Key { get; set; }

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
