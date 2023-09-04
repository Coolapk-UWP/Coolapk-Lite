using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public class InfoBadgeTemplateSettings : DependencyObject
    {
        internal InfoBadgeTemplateSettings()
        {
        }

        #region IconElement

        private static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(UIElement),
                typeof(InfoBadgeTemplateSettings),
                null);

        public UIElement IconElement
        {
            get => (UIElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }

        #endregion

        #region InfoBadgeCornerRadius

        private static readonly DependencyProperty InfoBadgeCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(InfoBadgeCornerRadius),
                typeof(CornerRadius),
                typeof(InfoBadgeTemplateSettings),
                null);

        public CornerRadius InfoBadgeCornerRadius
        {
            get => (CornerRadius)GetValue(InfoBadgeCornerRadiusProperty);
            internal set => SetValue(InfoBadgeCornerRadiusProperty, value);
        }

        #endregion
    }
}
