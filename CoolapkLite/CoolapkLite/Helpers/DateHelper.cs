using System;

namespace CoolapkLite.Helpers
{
    public static class DateHelper
    {
        public enum TimeIntervalType
        {
            YearsAgo,
            MonthsAgo,
            DaysAgo,
            HoursAgo,
            MinutesAgo,
            JustNow,
            Soon,
            MinutesLater,
            HoursLater,
            DaysLater,
            MonthsLater,
            YearsLater,
        }

        private static readonly DateTime UnixDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ConvertUnixTimeStampToReadable(this long time) => ConvertUnixTimeStampToReadable(time, DateTime.Now);

        public static string ConvertUnixTimeStampToReadable(this long time, DateTime? baseTime) => ConvertDateTimeToReadable(time.ConvertUnixTimeStampToDateTime(), baseTime);

        public static string ConvertDateTimeToReadable(this DateTime time) => ConvertDateTimeToReadable(time, DateTime.Now);

        public static string ConvertDateTimeToReadable(this DateTime time, DateTime? baseTime)
        {
            object obj;
            TimeIntervalType type;

            if (baseTime == null)
            {
                type = TimeIntervalType.YearsAgo;
                obj = time.ToLocalTime();
            }
            else
            {
                time = time.ToUniversalTime();
                DateTime universalTime = baseTime.Value.ToUniversalTime();
                TimeSpan temp = universalTime.Subtract(time);

                if (temp.Days > 30)
                {
                    type = time.Year == universalTime.Year
                        ? TimeIntervalType.MonthsAgo
                        : TimeIntervalType.YearsAgo;
                    obj = time.ToLocalTime();
                }
                else if (temp.TotalDays > 0)
                {
                    type = temp.Days > 0
                        ? TimeIntervalType.DaysAgo
                        : temp.Hours > 0
                            ? TimeIntervalType.HoursAgo
                            : temp.Minutes > 0
                                ? TimeIntervalType.MinutesAgo
                                : TimeIntervalType.JustNow;
                    obj = temp;
                }
                else if (temp.Days > -30)
                {
                    type = temp.Days < 0
                        ? TimeIntervalType.DaysLater
                        : temp.Hours < 0
                            ? TimeIntervalType.HoursLater
                            : temp.Minutes < 0
                                ? TimeIntervalType.MinutesLater
                                : TimeIntervalType.Soon;
                    obj = temp.Negate();
                }
                else
                {
                    type = time.Year == universalTime.Year
                        ? TimeIntervalType.MonthsLater
                        : TimeIntervalType.YearsLater;
                    obj = time.ToLocalTime();
                }
            }

            switch (type)
            {
                case TimeIntervalType.YearsAgo:
                case TimeIntervalType.YearsLater:
                    return ((DateTime)obj).ToString("D");

                case TimeIntervalType.MonthsAgo:
                case TimeIntervalType.MonthsLater:
                    return ((DateTime)obj).ToString("M");

                case TimeIntervalType.DaysAgo:
                    return $"{((TimeSpan)obj).Days}天前";

                case TimeIntervalType.HoursAgo:
                    return $"{((TimeSpan)obj).Hours}小时前";

                case TimeIntervalType.MinutesAgo:
                    return $"{((TimeSpan)obj).Minutes}分钟前";

                case TimeIntervalType.JustNow:
                    return "刚刚";

                case TimeIntervalType.Soon:
                    return "不久之后";

                case TimeIntervalType.MinutesLater:
                    return $"{((TimeSpan)obj).Minutes}分钟之后";

                case TimeIntervalType.HoursLater:
                    return $"{((TimeSpan)obj).Hours}小时之后";

                case TimeIntervalType.DaysLater:
                    return $"{((TimeSpan)obj).Days}天之后";

                default:
                    return string.Empty;
            }
        }

        public static DateTime ConvertUnixTimeStampToDateTime(this long time) => UnixDateBase.Add(new TimeSpan(time * 1000_0000));

        public static double ConvertDateTimeToUnixTimeStamp(this DateTime time) => Math.Round(time.ToUniversalTime().Subtract(UnixDateBase).TotalSeconds);
    }
}
