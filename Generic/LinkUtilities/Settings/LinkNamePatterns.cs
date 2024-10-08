﻿using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LinkUtilities.Settings
{
    /// <summary>
    ///     Types of patterns that can be matched.
    /// </summary>
    public enum PatternTypes
    {
        LinkNamePattern,
        RemovePattern,
        RenamePattern,
        MissingLinkPatterns
    }

    /// <summary>
    ///     Handles the Patterns to find link names for URL/link title combinations
    /// </summary>
    public class LinkNamePatterns : ObservableCollection<LinkNamePattern>
    {
        /// <summary>
        ///     Creates an empty instance
        /// </summary>
        public LinkNamePatterns() => SortByPosition = false;

        /// <summary>
        ///     Creates an empty instance, but sets the SortByPosition property to the desired value.
        /// </summary>
        /// <param name="sortByPosition">If true the list is sorted by the position property.</param>
        public LinkNamePatterns(bool sortByPosition = false) => SortByPosition = sortByPosition;

        public bool SortByPosition { get; set; }

        /// <summary>
        ///     Adds the default patterns to the list and sorts it afterward.
        /// </summary>
        /// <param name="type">
        ///     Type of the pattern to be added
        /// </param>
        public void AddDefaultPatterns(PatternTypes type)
        {
            foreach (var item in GetDefaultLinkNamePatterns(type).Where(item => this.All(x => x.LinkName != item.LinkName)))
            {
                Add(item);
            }

            SortPatterns();
        }

        public bool LinkMatch(ref string linkName, string linkUrl, bool overridePartialMatch = false)
        {
            var tempLinkName = linkName;

            var pattern = this.FirstOrDefault(x => x.LinkMatch(tempLinkName, linkUrl, overridePartialMatch));

            if (pattern == null)
            {
                return false;
            }

            linkName = pattern.LinkName;
            return true;
        }

        public void RemoveEmpty() => this.RemoveAll(x => !(x.NamePattern?.Any() ?? false) && !(x.UrlPattern?.Any() ?? false));

        public void SortPatterns()
        {
            var patterns = this.ToList();
            Clear();

            if (SortByPosition)
            {
                this.AddMissing(patterns.Distinct()
                    .OrderBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)
                    .ToList());
            }
            else
            {
                this.AddMissing(patterns.Distinct()
                    .OrderBy(x => x.Position)
                    .ThenBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(x => x.NamePattern, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(x => x.UrlPattern, StringComparer.CurrentCultureIgnoreCase)
                    .ToList());
            }
        }

        /// <summary>
        ///     Gets a list of default patterns.
        /// </summary>
        /// <param name="type">
        ///     Type of the pattern to be added
        /// </param>
        private static IEnumerable<LinkNamePattern> GetDefaultLinkNamePatterns(PatternTypes type)
        {
            string fileName;

            switch (type)
            {
                case PatternTypes.LinkNamePattern:
                {
                    fileName = "DefaultLinkNamePatterns.json";
                    break;
                }
                case PatternTypes.RemovePattern:
                {
                    fileName = "DefaultRemovePatterns.json";
                    break;
                }
                case PatternTypes.RenamePattern:
                {
                    fileName = "DefaultRenamePatterns.json";
                    break;
                }
                case PatternTypes.MissingLinkPatterns:
                {
                    fileName = "DefaultMissingLinkPatterns.json";
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return Serialization.FromJsonFile<List<LinkNamePattern>>(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Resources",
                fileName));
        }
    }
}