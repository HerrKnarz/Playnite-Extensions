using System;

namespace KNARZhelper
{
    public static class MiscHelper
    {
        /// <summary>
        /// Converts a Unix Timestamp to DateTime.
        /// </summary>
        /// <param name="unixTimeStamp">The timestamp to convert</param>
        /// <returns>DateTime value of the timestamp</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.StartOfMonth().AddMonths(1).AddDays(-1);
        }
    }
}