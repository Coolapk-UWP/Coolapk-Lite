using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Helpers.Converters
{
    /// <summary>
    /// Gets the approximated hex name for the color.
    /// </summary>
    public class ColorToHexNameConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color;

            if (value is Color valueColor)
            {
                color = valueColor;
            }
            else if (value is SolidColorBrush valueBrush)
            {
                color = valueBrush.Color;
            }
            else
            {
                // Invalid color value provided
                return DependencyProperty.UnsetValue;
            }

            return ConverterTools.Convert(color.ToHex(), targetType);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return targetType == typeof(Color)
                ? value.ToString().ToColor()
                : targetType == typeof(SolidColorBrush)
                    ? new SolidColorBrush(value.ToString().ToColor())
                    : DependencyProperty.UnsetValue;
        }
    }
}