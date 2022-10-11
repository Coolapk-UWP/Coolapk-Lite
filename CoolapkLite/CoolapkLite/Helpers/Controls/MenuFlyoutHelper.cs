using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class MenuFlyoutHelper
    {
        #region Icon

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

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(IconElement),
                typeof(MenuFlyoutHelper),
                new PropertyMetadata(null, OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuFlyoutItem item)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon"))
                {
                    item.Icon = IconElement;
                }
            }
            else if (d is MenuFlyoutSubItem subitem)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutSubItem", "Icon"))
                {
                    subitem.Icon = IconElement;
                }
            }
            else if (d is ToggleMenuFlyoutItem toggle)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem", "Icon"))
                {
                    toggle.Icon = IconElement;
                }
            }
        }

        #endregion
    }
}
