using CoolapkLite.Core.Models;
using System;
using System.Collections.Immutable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.ValueConverters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "bool": return (value is bool) ? (bool)value ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
                case "!bool": return (value is bool) ? (bool)value ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;
                case "entity": return (value is ImmutableArray<Entity>) ?!((ImmutableArray<Entity>)value).IsDefaultOrEmpty ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
                case "string": return (value is string) ? !string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed : !string.IsNullOrEmpty((value ?? string.Empty).ToString()) ? Visibility.Visible : Visibility.Collapsed;
                default: return value is bool boolean ? boolean ? Visibility.Visible : Visibility.Collapsed : value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }
}
