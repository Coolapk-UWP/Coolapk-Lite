using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.Converters
{
    public class TimeSpanToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time;
            switch (value)
            {
                case TimeSpan timeSpan:
                    time = timeSpan;
                    break;
                case string @string:
                    time = TimeSpan.Parse(@string);
                    break;
                default:
                    time = TimeSpan.FromMinutes(System.Convert.ToDouble(value));
                    break;
            }
            return ConverterTools.Convert(time.ToString(), targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            else
            {
                TimeSpan timeSpan = TimeSpan.Parse(value.ToString());
                return targetType == typeof(TimeSpan) ? timeSpan : ConverterTools.Convert(timeSpan.TotalMinutes, targetType);
            }
        }
    }
}
