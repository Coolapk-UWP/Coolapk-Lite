using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CoolapkLite.Helpers
{
    public static class FlyoutBaseHelper
    {
        #region Icon

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(IconElement),
                typeof(FlyoutBaseHelper),
                new PropertyMetadata(null, OnIconChanged));

        /// <summary>
        /// Gets the graphic content of the menu flyout item.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>The graphic content of the menu flyout item.</returns>
        public static IconElement GetIcon(MenuFlyoutItemBase control)
        {
            return (IconElement)control.GetValue(IconProperty);
        }

        /// <summary>
        /// Sets the graphic content of the menu flyout item.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetIcon(MenuFlyoutItemBase control, IconElement value)
        {
            control.SetValue(IconProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuFlyoutItem item)
            {
                if (e.NewValue is IconElement IconElement && ApiInfoHelper.IsMenuFlyoutItemIconSupported)
                {
                    item.Icon = IconElement;
                }
            }
            else if (d is MenuFlyoutSubItem subitem)
            {
                if (e.NewValue is IconElement IconElement && ApiInfoHelper.IsMenuFlyoutSubItemIconSupported)
                {
                    subitem.Icon = IconElement;
                }
            }
            else if (d is ToggleMenuFlyoutItem toggle)
            {
                if (e.NewValue is IconElement IconElement && ApiInfoHelper.IsToggleMenuFlyoutItemIconSupported)
                {
                    toggle.Icon = IconElement;
                }
            }
        }

        #endregion

        #region ShouldConstrainToRootBounds

        /// <summary>
        /// Identifies the ShouldConstrainToRootBounds dependency property.
        /// </summary>
        public static readonly DependencyProperty ShouldConstrainToRootBoundsProperty =
            DependencyProperty.RegisterAttached(
                "ShouldConstrainToRootBounds",
                typeof(bool),
                typeof(FlyoutBaseHelper),
                new PropertyMetadata(true, OnShouldConstrainToRootBoundsChanged));

        /// <summary>
        /// Gets a value that indicates whether the flyout should be shown within the bounds of the XAML root.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>A value that indicates whether the flyout should be shown within the bounds of the XAML root.</returns>
        public static bool GetShouldConstrainToRootBounds(FlyoutBase control)
        {
            return (bool)control.GetValue(ShouldConstrainToRootBoundsProperty);
        }

        /// <summary>
        /// Sets a value that indicates whether the flyout should be shown within the bounds of the XAML root.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetShouldConstrainToRootBounds(FlyoutBase control, bool value)
        {
            control.SetValue(ShouldConstrainToRootBoundsProperty, value);
        }

        private static void OnShouldConstrainToRootBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlyoutBase flyout)
            {
                if (e.NewValue is bool ShouldConstrainToRootBounds && ApiInfoHelper.IsShouldConstrainToRootBoundsSupported)
                {
                    flyout.ShouldConstrainToRootBounds = ShouldConstrainToRootBounds;
                }
            }
        }

        #endregion
    }
}
