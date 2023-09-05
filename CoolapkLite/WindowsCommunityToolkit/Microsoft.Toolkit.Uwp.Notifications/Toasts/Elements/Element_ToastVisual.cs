// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    internal sealed class Element_ToastVisual : IHaveXmlName, IHaveXmlNamedProperties, IHaveXmlChildren
    {
        internal const bool DEFAULT_ADD_IMAGE_QUERY = false;

        public bool? AddImageQuery { get; set; }

        public Uri BaseUri { get; set; }

        public string Language { get; set; }

        public int? Version { get; set; }

        public IList<Element_ToastBinding> Bindings { get; private set; } = new List<Element_ToastBinding>();

        /// <inheritdoc/>
        string IHaveXmlName.Name => "visual";

        /// <inheritdoc/>
        IEnumerable<object> IHaveXmlChildren.Children => Bindings;

        /// <inheritdoc/>
        IEnumerable<KeyValuePair<string, object>> IHaveXmlNamedProperties.EnumerateNamedProperties()
        {
            yield return new KeyValuePair<string, object>("addImageQuery", AddImageQuery);
            yield return new KeyValuePair<string, object>("baseUri", BaseUri);
            yield return new KeyValuePair<string, object>("lang", Language);
            yield return new KeyValuePair<string, object>("version", Version);
        }
    }
}