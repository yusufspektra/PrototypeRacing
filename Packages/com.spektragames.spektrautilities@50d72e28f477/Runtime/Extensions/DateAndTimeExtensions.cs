using System;
using System.Globalization;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class DateAndTimeExtensions
    {
        public static DateTime ConvertStringToDateTime(this string value)
        {
            return DateTime.ParseExact(value, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ConvertDateTimeToString(this DateTime value)
        {
            return value.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public static string ConvertDateTimeToStringForSQL(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static int ConvertDateTimeToTimestamp(this DateTime value)
        {
            // Seconds 
            var epoch = value - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)epoch.TotalSeconds;
        }

        public static DateTime ConvertTimestampToDateTime(this int value)
        {
            // Seconds
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
            dtDateTime = dtDateTime.AddSeconds(value);
            return dtDateTime;
        }

        public static long ConvertDateTimeToTimestampMS(this DateTime value)
        {
            // Milliseconds 
            var epoch = value - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (long)epoch.TotalMilliseconds;
        }

        public static DateTime ConvertTimestampToDateTimeMS(this long value)
        {
            // Milliseconds
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
            dtDateTime = dtDateTime.AddMilliseconds(value);
            return dtDateTime;
        }
    }
}