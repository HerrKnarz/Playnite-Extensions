using LinkUtilities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LinkUtilities.Settings
{
    /// <summary>
    /// Handles the Patterns to find link names for URL/link title combinations
    /// </summary>
    public class LinkNamePatterns : ObservableCollection<LinkNamePattern>
    {
        /// <summary>
        /// Creates an instance with the given items
        /// </summary>
        /// <param name="items">items to add to the collection</param>
        public LinkNamePatterns(List<LinkNamePattern> items)
        {
            this.AddMissing(items);
        }

        /// <summary>
        /// Creates an empty instance
        /// </summary>
        public LinkNamePatterns()
        {
        }

        /// <summary>
        /// Gets a list of default patterns.
        /// </summary>
        public static List<LinkNamePattern> GetDefaultLinkNamePatterns()
        {
            string json = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "DefaultLinkNamePatterns.json"));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<LinkNamePattern>>(json);
        }

        /// <summary>
        /// Adds the default patterns to the list and sorts it afterwards.
        /// </summary>
        public void AddDefaultPatterns()
        {
            List<LinkNamePattern> patterns = this.ToList();

            foreach (LinkNamePattern item in GetDefaultLinkNamePatterns())
            {
                if (!patterns.Any(x => x.LinkName == item.LinkName))
                {
                    patterns.Add(item);
                }
            }

            Clear();
            this.AddMissing(patterns.Distinct().OrderBy(x => x.LinkName));
        }

        public bool LinkMatch(ref string linkName, string linkUrl)
        {
            string tempLinkName = linkName;

            LinkNamePattern pattern = this.FirstOrDefault(x => x.LinkMatch(tempLinkName, linkUrl));
            if (pattern != null)
            {
                linkName = pattern.LinkName;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
