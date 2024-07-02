using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KNARZhelper
{
    public static class MiscHelper
    {
        public static void AddTextIcoFontResource(string key, string text)
        {
            Application.Current.Resources.Add(key, new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
            });
        }

        public static DateTime EndOfMonth(this DateTime date)
                    => date.StartOfMonth().AddMonths(1).AddDays(-1);

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