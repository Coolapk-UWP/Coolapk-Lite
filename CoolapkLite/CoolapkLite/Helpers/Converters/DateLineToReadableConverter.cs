using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.Converters
{
    public class DateLineToReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = ConverterTools.TryParseBool(parameter)
                ? System.Convert.ToInt64(value).ConvertUnixTimeStampToReadable()
                : System.Convert.ToInt64(value).ConvertUnixTimeStampToReadable(null);
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value, targetType);
    }
}
