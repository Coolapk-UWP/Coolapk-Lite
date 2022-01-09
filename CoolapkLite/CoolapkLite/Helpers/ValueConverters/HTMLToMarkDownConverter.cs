using CoolapkLite.Core.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.ValueConverters
{
    public class HTMLToMarkDownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value.ToString().CSStoMarkDown();

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value.ToString().MarkDowntoCSS();
    }
}
