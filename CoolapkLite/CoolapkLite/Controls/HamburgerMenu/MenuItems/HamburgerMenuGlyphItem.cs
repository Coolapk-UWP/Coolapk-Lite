// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenuGlyphItem provides a glyph based implementation for HamburgerMenu entries.
    /// </summary>
    public class HamburgerMenuGlyphItem : HamburgerMenuItem
    {
        #region Glyph

        /// <summary>
        /// Identifies the <see cref="Glyph"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(
                nameof(Glyph),
                typeof(string),
                typeof(HamburgerMenuItem),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that specifies the glyph to use from Segoe MDL2 Assets font.
        /// </summary>
        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        #endregion
    }
}
