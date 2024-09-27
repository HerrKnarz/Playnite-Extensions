using System.Linq;

namespace WikipediaMetadata
{
    internal static class WikipediaHelper
    {
        /// <summary>
        /// Fetches the url of the main image from the page.
        /// </summary>
        /// <param name="key">Key of the page</param>
        /// <returns>Url of the image</returns>
        internal static string GetImageUrl(string key) =>
            WikipediaApiCaller.GetImage(key)?.Query?.Pages.FirstOrDefault()?.Original?.Source;
    }
}