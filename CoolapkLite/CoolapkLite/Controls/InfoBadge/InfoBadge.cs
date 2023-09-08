using CoolapkLite.Helpers;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// Represents a control for indicating notifications, alerts, new content, or to attract focus to an area within an app.
    /// </summary>
    public class InfoBadge : Control
    {
        #region IconSource

        public UIElement IconSource
        {
            get => (UIElement)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(UIElement),
                typeof(InfoBadge),
                new PropertyMetadata(null, OnIconSourcePropertyChanged));

        private static void OnIconSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBadge)sender).OnDisplayKindPropertiesChanged();
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(InfoBadgeTemplateSettings),
                typeof(InfoBadge),
                null);

        public InfoBadgeTemplateSettings TemplateSettings
        {
            get => (InfoBadgeTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        #region Value

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(InfoBadge),
                new PropertyMetadata(-1, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((InfoBadge)sender).OnDisplayKindPropertiesChanged();
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoBadge"/> class.
        /// </summary>
        public InfoBadge()
        {
            DefaultStyleKey = typeof(InfoBadge);
            SetValue(TemplateSettingsProperty, new InfoBadgeTemplateSettings());
            SizeChanged += OnSizeChanged;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnDisplayKindPropertiesChanged();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size defaultDesiredSize = base.MeasureOverride(availableSize);
            return defaultDesiredSize.Width < defaultDesiredSize.Height
                ? new Size(defaultDesiredSize.Height, defaultDesiredSize.Height)
                : defaultDesiredSize;
        }

        private void OnDisplayKindPropertiesChanged()
        {
            Control thisAsControl = this;
            if (Value >= 0)
            {
                VisualStateManager.GoToState(thisAsControl, "Value", true);
            }
            else
            {
                UIElement iconSource = IconSource;
                if (iconSource != null)
                {
                    TemplateSettings.IconElement = iconSource;
                    if (iconSource is FontIcon)
                    {
                        VisualStateManager.GoToState(thisAsControl, "FontIcon", true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(thisAsControl, "Icon", true);
                    }
                }
                else
                {
                    VisualStateManager.GoToState(thisAsControl, "Dot", true);
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CornerRadius value()
            {
                double cornerRadiusValue = ActualHeight / 2;
                return ApiInfoHelper.IsCornerRadiusSupported
                    ? ReadLocalValue(CornerRadiusProperty) == DependencyProperty.UnsetValue
                        ? new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue)
                        : CornerRadius
                    : new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue);
            };

            TemplateSettings.InfoBadgeCornerRadius = value();
        }
    }
}
