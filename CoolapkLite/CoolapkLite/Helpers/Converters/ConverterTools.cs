// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml.Markup;

#if !NETCORE463
using System.Reflection;
#endif

namespace CoolapkLite.Helpers.Converters
{
    /// <summary>
    /// Static class used to provide internal tools
    /// </summary>
    internal static class ConverterTools
    {
        /// <summary>
        /// Helper method to safely cast an object to a boolean
        /// </summary>
        /// <param name="parameter">Parameter to cast to a boolean</param>
        /// <returns>Bool value or false if cast failed</returns>
        internal static bool TryParseBool(object parameter)
        {
            switch (parameter)
            {
                case bool @bool:
                    return @bool;
                case null:
                    return false;
                default:
                    return bool.TryParse(parameter.ToString(), out bool parsed) && parsed;
            }
        }

        /// <summary>
        /// Helper method to convert a value from a source type to a target type.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns>The converted value</returns>
        internal static object Convert(object value, Type targetType) => value == null || targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);

        /// <summary>
        /// Helper method to convert a value from a source type to a target type.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <typeparam name="T">The target type</typeparam>
        /// <returns>The converted value</returns>
        internal static T Convert<T>(object value)
        {
            switch (value)
            {
                case T typedValue:
                    return typedValue;
                case null:
                    return default;
                default:
                    object result = XamlBindingHelper.ConvertValue(typeof(T), value);
                    return (T)result;
            }
        }
    }
}