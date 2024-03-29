﻿using System;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers.Converters
{
    public class NumAddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double result = System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter);
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double result = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            return ConverterTools.Convert(result, targetType);
        }
    }
}
