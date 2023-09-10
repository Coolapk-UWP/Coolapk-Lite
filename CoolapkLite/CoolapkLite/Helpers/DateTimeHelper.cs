using System;

namespace CoolapkLite.Helpers
{
    public static class DateTimeHelper
    {
        private enum TimeIntervalType
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

        public static string ConvertUnixTimeStampToReadable(this long time) => ConvertUnixTimeStampToReadable(time, DateTimeOffset.UtcNow);

        public static string ConvertUnixTimeStampToReadable(this long time, DateTimeOffset? baseTime) => ConvertDateTimeOffsetToReadable(time.ConvertUnixTimeStampToDateTimeOffset(), baseTime);

        public static string ConvertDateTimeOffsetToReadable(this DateTimeOffset time) => ConvertDateTimeOffsetToReadable(time, DateTimeOffset.UtcNow);

        public static string ConvertDateTimeOffsetToReadable(this DateTimeOffset time, DateTimeOffset? baseTime)
        {
            object obj;
            TimeIntervalType type;

            if (baseTime == null)
            {
                type = TimeIntervalType.YearsAgo;
                obj = time.LocalDateTime;
            }
            else
            {
                TimeSpan temp = baseTime.Value.Subtract(time);

                if (temp.Days > 30)
                {
                    type = time.Year == baseTime?.Year
                        ? TimeIntervalType.MonthsAgo
                        : TimeIntervalType.YearsAgo;
                    obj = time.LocalDateTime;
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
                    type = time.Year == baseTime?.Year
                        ? TimeIntervalType.MonthsLater
                        : TimeIntervalType.YearsLater;
                    obj = time.LocalDateTime;
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

        public static DateTimeOffset ConvertUnixTimeStampToDateTimeOffset(this long time) =>
            time >= 100000_00000
                ? DateTimeOffset.FromUnixTimeMilliseconds(time)
                : DateTimeOffset.FromUnixTimeSeconds(time);

        public static DateTime ConvertDateTimeOffsetToDateTime(this DateTimeOffset dateTime) =>
            dateTime.Offset.Equals(TimeSpan.Zero)
                ? dateTime.UtcDateTime
                : dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime))
                    ? DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local)
                    : dateTime.DateTime;
    }
}
