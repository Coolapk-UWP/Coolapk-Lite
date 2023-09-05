// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    internal static class XmlWriterHelper
    {
        public static void Write(XmlWriter writer, object element)
        {
            // If it isn't an XML element, don't write anything
            if (!(element is IHaveXmlName xmlElement))
            {
                return;
            }

            writer.WriteStartElement(xmlElement.Name);

            // Write all named properties
            foreach (var property in (element as IHaveXmlNamedProperties)?.EnumerateNamedProperties() ?? Enumerable.Empty<KeyValuePair<string, object>>())
            {
                if (!(property.Value is null))
                {
                    writer.WriteAttributeString(property.Key, PropertyValueToString(property.Value));
                }
            }

            // Write all additional properties
            foreach (var property in (element as IHaveXmlAdditionalProperties)?.AdditionalProperties ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                writer.WriteAttributeString(property.Key, property.Value);
            }

            // Write the inner text, if any
            if ((element as IHaveXmlText)?.Text is string text && text.Length > 0)
            {
                writer.WriteString(text);
            }

            // Write all children, if any
            foreach (var child in (element as IHaveXmlChildren)?.Children ?? Enumerable.Empty<object>())
            {
                Write(writer, child);
            }

            writer.WriteEndElement();
        }

        private static string PropertyValueToString(object propertyValue)
        {
            switch (propertyValue)
            {
                case true: return "true";
                case false: return "false";
                default:
                    return propertyValue is DateTimeOffset dateTime
                        ? XmlConvert.ToString(dateTime) // ISO 8601 format
                        : (propertyValue?.ToString());
            }
        }

        /// <summary>
        /// Gets the provided binding value, if it exists. Otherwise, falls back to the absolute value.
        /// </summary>
        /// <typeparam name="T">The type of the enum of the class properties.</typeparam>
        /// <param name="bindings">The collection of data-bound values.</param>
        /// <param name="bindableProperty">The property to obtain.</param>
        /// <param name="absoluteValue">The absolute value, if any.</param>
        /// <returns>The provided binding value, if it exists. Otherwise, falls back to the absolute value.</returns>
        internal static string GetBindingOrAbsoluteXmlValue<T>(IDictionary<T, string> bindings, T bindableProperty, string absoluteValue)
        {
            // If a binding is provided, use the binding value
            if (bindings.TryGetValue(bindableProperty, out string bindingValue))
            {
                return "{" + bindingValue + "}";
            }

            // Otherwise fallback to the absolute value
            return absoluteValue;
        }
    }
}