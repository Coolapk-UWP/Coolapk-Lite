// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Notifications.Adaptive.Elements;
using System;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    internal static class BaseImageHelper
    {
        internal static void SetSource(ref string destination, string value)
        {
            destination = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal static Element_AdaptiveImage CreateBaseElement(IBaseImage current)
        {
            if (current.Source == null)
            {
                throw new NullReferenceException("Source property is required.");
            }

            return new Element_AdaptiveImage()
            {
                Src = current.Source,
                Alt = current.AlternateText,
                AddImageQuery = current.AddImageQuery
            };
        }
    }
}