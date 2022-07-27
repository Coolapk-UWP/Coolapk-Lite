using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.ValueConverters
{
    public class HTMLToMarkDownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value.ToString().HTMLtoMarkDown();

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value.ToString().MarkDowntoHTML();
    }
}
