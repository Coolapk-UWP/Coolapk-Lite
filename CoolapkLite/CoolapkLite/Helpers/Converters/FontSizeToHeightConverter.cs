﻿using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.Converters
{
    public class FontSizeToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 4 / 3;
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 3 / 4;
            return ConverterTools.Convert(result, targetType);
        }
    }
}
