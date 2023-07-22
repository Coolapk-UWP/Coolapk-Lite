using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// Represents a control for indicating notifications, alerts, new content, or to attract focus to an area within an app.
    /// </summary>
    public class InfoBadge : Control
    {
        public InfoBadge()
        {
            DefaultStyleKey = typeof(InfoBadge);
            SetValue(TemplateSettingsProperty, new InfoBadgeTemplateSettings());
            SizeChanged += OnSizeChanged;
        }

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

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnDisplayKindPropertiesChanged();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var defaultDesiredSize = base.MeasureOverride(availableSize);
            if (defaultDesiredSize.Width < defaultDesiredSize.Height)
            {
                return new Size(defaultDesiredSize.Height, defaultDesiredSize.Height);
            }
            return defaultDesiredSize;
        }

        void OnDisplayKindPropertiesChanged()
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

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CornerRadius value()
            {
                var cornerRadiusValue = ActualHeight / 2;
                if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "CornerRadiusProperty"))
                {
                    if (ReadLocalValue(CornerRadiusProperty) == DependencyProperty.UnsetValue)
                    {
                        return new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue);
                    }
                    else
                    {
                        return CornerRadius;
                    }
                }
                return new CornerRadius(cornerRadiusValue, cornerRadiusValue, cornerRadiusValue, cornerRadiusValue);
            };

            TemplateSettings.InfoBadgeCornerRadius = value();
        }
    }
}
