// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenu is based on a SplitView control. By default it contains a HamburgerButton and a ListView to display menu items.
    /// </summary>
    public partial class HamburgerMenu
    {
        #region HamburgerWidth

        /// <summary>
        /// Identifies the <see cref="HamburgerWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HamburgerWidthProperty =
            DependencyProperty.Register(
                nameof(HamburgerWidth),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(48.0));

        /// <summary>
        /// Gets or sets main button's width.
        /// </summary>
        public double HamburgerWidth
        {
            get => (double)GetValue(HamburgerWidthProperty);
            set => SetValue(HamburgerWidthProperty, value);
        }

        #endregion

        #region HamburgerHeight

        /// <summary>
        /// Identifies the <see cref="HamburgerHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HamburgerHeightProperty =
            DependencyProperty.Register(
                nameof(HamburgerHeight),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(48.0));

        /// <summary>
        /// Gets or sets main button's height.
        /// </summary>
        public double HamburgerHeight
        {
            get => (double)GetValue(HamburgerHeightProperty);
            set => SetValue(HamburgerHeightProperty, value);
        }

        #endregion

        #region HamburgerMargin

        /// <summary>
        /// Identifies the <see cref="HamburgerMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HamburgerMarginProperty =
            DependencyProperty.Register(
                nameof(HamburgerMargin),
                typeof(Thickness),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets main button's margin.
        /// </summary>
        public Thickness HamburgerMargin
        {
            get => (Thickness)GetValue(HamburgerMarginProperty);
            set => SetValue(HamburgerMarginProperty, value);
        }

        #endregion

        #region HamburgerVisibility

        /// <summary>
        /// Identifies the <see cref="HamburgerVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HamburgerVisibilityProperty =
            DependencyProperty.Register(
                nameof(HamburgerVisibility),
                typeof(Visibility),
                typeof(HamburgerMenu),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets main button's visibility.
        /// </summary>
        public Visibility HamburgerVisibility
        {
            get => (Visibility)GetValue(HamburgerVisibilityProperty);
            set => SetValue(HamburgerVisibilityProperty, value);
        }

        #endregion

        #region HamburgerMenuTemplate

        /// <summary>
        /// Identifies the <see cref="HamburgerMenuTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HamburgerMenuTemplateProperty =
            DependencyProperty.Register(
                nameof(HamburgerMenuTemplate),
                typeof(DataTemplate),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a template for the hamburger icon.
        /// </summary>
        public DataTemplate HamburgerMenuTemplate
        {
            get => (DataTemplate)GetValue(HamburgerMenuTemplateProperty);
            set => SetValue(HamburgerMenuTemplateProperty, value);
        }

        #endregion
    }
}
