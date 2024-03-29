﻿using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    internal static class WikipediaHelper
    {
        /// <summary>
        /// Fetches the url of the main image from the page.
        /// </summary>
        /// <param name="key">Key of the page</param>
        /// <returns>Url of the image</returns>
        internal static string GetImageUrl(string key)
        {
            WikipediaImage imageData = WikipediaApiCaller.GetImage(key);

            ImagePage page = imageData?.Query?.Pages.FirstOrDefault();

            return page?.Original?.Source;
        }
    }
}