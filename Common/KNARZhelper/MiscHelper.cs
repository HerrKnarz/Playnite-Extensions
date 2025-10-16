using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KNARZhelper
{
    /// <summary>
    /// Miscellaneous helper functions.
    /// </summary>
    public static class MiscHelper
    {
        private static readonly Regex _regexNumbers = new Regex("[^0-9.-]+");

        /// <summary>
        /// Regex to remove company form from company names.
        /// </summary>
        public static readonly Regex CompanyFormRegex =
            new Regex(@",?\s+((co[,.\s]+)?ltd|(l\.)?inc|s\.?l|a[./]?s|limited|l\.?l\.?(c|p)|s\.?a(\.?r\.?l)?|s\.?r\.?o|gmbh|ab|corp|pte|ace|co|pty|pty\sltd|srl)\.?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Adds a TextBlock with IcoFont font to the application resources. Can be used in menus as icons.
        /// </summary>
        /// <param name="key">Key used to identify the resource.</param>
        /// <param name="text">Text to display in the TextBlock.</param>
        public static void AddTextIcoFontResource(string key, string text)
        {
            Application.Current.Resources.Add(key, new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
            });
        }

        /// <summary>
        /// Creates a deep clone of an object via JSON serialization.
        /// </summary>
        /// <typeparam name="T">Type if the object</typeparam>
        /// <param name="self">The instance of the object itself</param>
        /// <returns>Clone of the given object instance with all including objects cloned as well</returns>
        public static T DeepClone<T>(this T self) where T : class => Serialization.GetClone(self);

        /// <summary>
        /// Returns the last day of the month for the given date.
        /// </summary>
        /// <param name="date">Date to get the last day of the month for</param>
        /// <returns>The last day of the month for the given date</returns>
        public static DateTime EndOfMonth(this DateTime date)
                            => date.StartOfMonth().AddMonths(1).AddDays(-1);

        /// <summary>
        /// Checks if the given item is one of the given options.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="options">Options to check against</param>
        /// <returns>True if the item is one of the options, false otherwise</returns>
        public static bool IsOneOf(this object item, params object[] options) => options.Contains(item);

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of the parent to find</typeparam>
        /// <param name="child">Child element to start the search from</param>
        /// <returns>Found parent element of the specified type, or null if not found</returns>
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                //get parent item
                var parentObject = VisualTreeHelper.GetParent(child);

                switch (parentObject)
                {
                    //we've reached the end of the tree
                    case null:
                        return null;
                    //check if the parent matches the type we're looking for
                    case T parent:
                        return parent;

                    default:
                        child = parentObject;
                        break;
                }
            }
        }

        /// <summary>
        /// Checks if the given text contains only numbers (0-9), dots (.) or minus (-) signs.
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if the text contains only numbers, dots or minus signs, false otherwise</returns>
        public static bool IsOnlyNumbers(string text) => !_regexNumbers.IsMatch(text);

        /// <summary>
        /// Removes all items from the ObservableCollection that match the given condition.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <param name="coll">The collection to remove items from</param>
        /// <param name="condition">The condition to match items against</param>
        /// <returns>The number of items removed from the collection</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Sorts the ObservableCollection in place using the given key selector.
        /// </summary>
        /// <typeparam name="TSource">Type of the items in the collection</typeparam>
        /// <typeparam name="TKey">Type of the key to sort by</typeparam>
        /// <param name="source">The collection to sort</param>
        /// <param name="keySelector">The key selector to determine the sort order</param>
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            var sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (var sortedItem in sortedList)
            {
                source.Add(sortedItem);
            }
        }

        /// <summary>
        /// Returns the first day of the month for the given date.
        /// </summary>
        /// <param name="date">Date to get the first day of the month for</param>
        /// <returns>The first day of the month for the given date</returns>
        public static DateTime StartOfMonth(this DateTime date)
                    => new DateTime(date.Year, date.Month, 1);

        /// <summary>
        /// Converts a Unix Timestamp to DateTime.
        /// </summary>
        /// <param name="unixTimeStamp">The timestamp to convert</param>
        /// <returns>DateTime value of the timestamp</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
    }
}