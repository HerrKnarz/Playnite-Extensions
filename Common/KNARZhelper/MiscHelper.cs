using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Playnite.SDK.Data;

namespace KNARZhelper
{
    public static class MiscHelper
    {
        private static readonly Regex _regexNumbers = new Regex("[^0-9.-]+");

        public static void AddTextIcoFontResource(string key, string text)
        {
            Application.Current.Resources.Add(key, new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
            });
        }

        public static T DeepClone<T>(this T self) where T : class => Serialization.GetClone(self);

        public static DateTime EndOfMonth(this DateTime date)
                            => date.StartOfMonth().AddMonths(1).AddDays(-1);

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                //get parent item
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);

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

        public static bool IsOnlyNumbers(string text) => !_regexNumbers.IsMatch(text);

        public static int RemoveAll<T>(
                            this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            System.Collections.Generic.List<T> itemsToRemove = coll.Where(condition).ToList();

            foreach (T itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            List<TSource> sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (TSource sortedItem in sortedList)
            {
                source.Add(sortedItem);
            }
        }

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