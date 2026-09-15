using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class AppBarButtonHelper
    {
        #region Icon

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(object),
                typeof(AppBarButtonHelper),
                new PropertyMetadata(null, OnIconChanged));

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>The Icon.</returns>
        public static object GetIcon(AppBarButton control)
        {
            return control.GetValue(IconProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetIcon(AppBarButton control, object value)
        {
            control.SetValue(IconProperty, value);
        }

        private static async void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is AppBarButton element)) { return; }
            ContentPresenter presenter = null;
            await element.ResumeOnLoadedAsync(() => (presenter = element.FindDescendant("Content") as ContentPresenter) != null);
            if (presenter == null)
            {
                if (element.FindDescendant("Content") is ContentPresenter contentPresenter)
                { presenter = contentPresenter; }
                else { return; }
            }
            object content = e.NewValue;
            element.Icon = content == null ? null : new FontIcon();
            presenter.Content = content;
        }

        #endregion
    }
}
