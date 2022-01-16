using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.ValueConverters
{
    internal class FontSizeToLineHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double zoom = 4 / 3;
            if (parameter is double num)
            {
                zoom = num;
            }
            return value is double size ? size * zoom : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double zoom = 3 / 4;
            if (parameter is double num)
            {
                zoom = 1 / num;
            }
            return value is double size ? size * zoom : value;
        }
    }
}
