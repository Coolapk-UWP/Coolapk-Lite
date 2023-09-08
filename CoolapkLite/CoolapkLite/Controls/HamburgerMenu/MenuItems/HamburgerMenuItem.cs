// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenuItem provides an abstract implementation for HamburgerMenu entries.
    /// </summary>
    public abstract class HamburgerMenuItem : DependencyObject
    {
        #region Label

        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(HamburgerMenuItem),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that specifies label to display.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region TargetPageType

        /// <summary>
        /// Identifies the <see cref="TargetPageType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetPageTypeProperty =
            DependencyProperty.Register(
                nameof(TargetPageType),
                typeof(Type),
                typeof(HamburgerMenuItem),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that specifies the page to navigate to (if you use the HamburgerMenu with a Frame content)
        /// </summary>
        public Type TargetPageType
        {
            get => (Type)GetValue(TargetPageTypeProperty);
            set => SetValue(TargetPageTypeProperty, value);
        }

        #endregion

        #region Tag

        /// <summary>
        /// Identifies the <see cref="Tag"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register(
                nameof(Tag),
                typeof(object),
                typeof(HamburgerMenuItem),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that specifies an user specific value.
        /// </summary>
        public object Tag
        {
            get => GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        #endregion
    }
}
