using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.Converters
{
    public class DateLineToReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = string.Empty;
            switch (value)
            {
                case long @long:
                    result = ConverterTools.TryParseBool(parameter)
                        ? @long.ConvertUnixTimeStampToReadable(null)
                        : @long.ConvertUnixTimeStampToReadable();
                    break;
                case DateTimeOffset dateTimeOffset:
                    result = ConverterTools.TryParseBool(parameter)
                        ? dateTimeOffset.ConvertDateTimeOffsetToReadable(null)
                        : dateTimeOffset.ConvertDateTimeOffsetToReadable();
                    break;
                case DateTime dateTime:
                    result = ConverterTools.TryParseBool(parameter)
                        ? new DateTimeOffset(dateTime).ConvertDateTimeOffsetToReadable(null)
                        : new DateTimeOffset(dateTime).ConvertDateTimeOffsetToReadable();
                    break;
                case null:
                    break;
                default:
                    result = ConverterTools.TryParseBool(parameter)
                        ? System.Convert.ToInt64(value).ConvertUnixTimeStampToReadable(null)
                        : System.Convert.ToInt64(value).ConvertUnixTimeStampToReadable();
                    break;
            }
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object result = null;
            if (DateTimeOffset.TryParse(value?.ToString(), out DateTimeOffset dateTimeOffset))
            {
                if (targetType == typeof(DateTimeOffset))
                {
                    result = dateTimeOffset;
                }
                else if (targetType == typeof(DateTime))
                {
                    result = dateTimeOffset.ConvertDateTimeOffsetToDateTime();
                }
                else
                {
                    result = dateTimeOffset.ToUnixTimeSeconds();
                }
            }
            return ConverterTools.Convert(result, targetType);
        }
    }
}
