using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    public static class FocusVisualHelper
    {
        #region FocusVisualMargin

        /// <summary>
        /// Gets the outer margin of the focus visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// Provides margin values for the focus visual. The default value is a default Thickness
        /// with all properties (dimensions) equal to 0.
        /// </returns>
        public static Thickness GetFocusVisualMargin(FrameworkElement element)
        {
            return (Thickness)element.GetValue(FocusVisualMarginProperty);
        }

        /// <summary>
        /// Sets the outer margin of the focus visual for a FrameworkElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetFocusVisualMargin(FrameworkElement element, Thickness value)
        {
            element.SetValue(FocusVisualMarginProperty, value);
        }

        /// <summary>
        /// Identifies the FocusVisualMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualMarginProperty =
            DependencyProperty.RegisterAttached(
                "FocusVisualMargin",
                typeof(Thickness),
                typeof(FocusVisualHelper),
                new PropertyMetadata(null, OnFocusVisualMarginChanged));

        private static void OnFocusVisualMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (ApiInfoHelper.IsFocusVisualMarginSupported)
            {
                ((FrameworkElement)d).FocusVisualMargin = GetFocusVisualMargin((FrameworkElement)d);
            }
        }

        #endregion
    }
}
