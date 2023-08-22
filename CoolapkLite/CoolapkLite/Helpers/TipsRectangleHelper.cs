using CoolapkLite.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public class TipsRectangleHelper : DependencyObject
    {
        #region IsEnable

        /// <summary>
        /// Identifies the <see cref="IsEnable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.Register(
                "IsEnable",
                typeof(bool),
                typeof(TipsRectangleHelper),
                new PropertyMetadata(false, OnIsEnablePropertyChanged));

        public static bool GetIsEnable(ItemsControl control)
        {
            return (bool)control.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(ItemsControl control, bool value)
        {
            control.SetValue(IsEnableProperty, value);
        }

        private static void OnIsEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl)
            {
                AnimateSelectionProvider provider = new AnimateSelectionProvider
                {
                    ItemsControls = new ItemsControl[] { itemsControl },
                    IndicatorName = GetIndicatorName(itemsControl),
                    Orientation = GetOrientation(itemsControl)
                };
                SetProvider(itemsControl, provider);
            }
        }

        #endregion

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                "Provider",
                typeof(AnimateSelectionProvider),
                typeof(TipsRectangleHelper),
                null);

        public static AnimateSelectionProvider GetProvider(ItemsControl control)
        {
            return (AnimateSelectionProvider)control.GetValue(ProviderProperty);
        }

        private static void SetProvider(ItemsControl control, AnimateSelectionProvider value)
        {
            control.SetValue(ProviderProperty, value);
        }

        #endregion

        #region IndicatorName

        /// <summary>
        /// Identifies the IndicatorName dependency property.
        /// </summary>
        public static readonly DependencyProperty IndicatorNameProperty =
            DependencyProperty.Register(
                "IndicatorName",
                typeof(string),
                typeof(TipsRectangleHelper),
                new PropertyMetadata(null, OnIndicatorNamePropertyChanged));

        public static string GetIndicatorName(ItemsControl control)
        {
            return (string)control.GetValue(IndicatorNameProperty);
        }

        public static void SetIndicatorName(ItemsControl control, string value)
        {
            control.SetValue(IndicatorNameProperty, value);
        }

        private static void OnIndicatorNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl && GetProvider(itemsControl) is AnimateSelectionProvider provider)
            {
                provider.IndicatorName = GetIndicatorName(itemsControl);
            }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(TipsRectangleHelper),
                new PropertyMetadata(Orientation.Vertical, OnOrientationPropertyChanged));

        public static Orientation GetOrientation(ItemsControl control)
        {
            return (Orientation)control.GetValue(OrientationProperty);
        }

        public static void SetOrientation(ItemsControl control, Orientation value)
        {
            control.SetValue(OrientationProperty, value);
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl && GetProvider(itemsControl) is AnimateSelectionProvider provider)
            {
                provider.Orientation = GetOrientation(itemsControl);
            }
        }

        #endregion
    }
}
