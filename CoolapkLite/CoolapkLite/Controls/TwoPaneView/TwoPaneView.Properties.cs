using System;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class TwoPaneView
    {
        /// <summary>
        /// Identifies the <see cref="MinTallModeHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinTallModeHeightProperty =
            DependencyProperty.Register(
                nameof(MinTallModeHeight),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(641, OnMinLengthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="MinWideModeWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinWideModeWidthProperty =
            DependencyProperty.Register(
                nameof(MinWideModeWidth),
                typeof(double),
                typeof(TwoPaneView),
                new PropertyMetadata(641, OnMinLengthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(TwoPaneViewMode),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewMode.Wide));

        /// <summary>
        /// Identifies the <see cref="Pane1"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane1Property =
            DependencyProperty.Register(
                nameof(Pane1),
                typeof(UIElement),
                typeof(TwoPaneView),
                new PropertyMetadata(null, OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane1Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane1LengthProperty =
            DependencyProperty.Register(
                nameof(Pane1Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(new GridLength(1, GridUnitType.Auto), OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane2"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane2Property =
            DependencyProperty.Register(
                nameof(Pane2),
                typeof(UIElement),
                typeof(TwoPaneView),
                new PropertyMetadata(null, OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane2Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane2LengthProperty =
            DependencyProperty.Register(
                nameof(Pane2Length),
                typeof(GridLength),
                typeof(TwoPaneView),
                new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="PanePriority"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PanePriorityProperty =
            DependencyProperty.Register(
                nameof(PanePriority),
                typeof(TwoPaneViewPriority),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewPriority.Pane1, OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="TallModeConfiguration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TallModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(TallModeConfiguration),
                typeof(TwoPaneViewTallModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewTallModeConfiguration.TopBottom, OnModePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="WideModeConfiguration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WideModeConfigurationProperty =
            DependencyProperty.Register(
                nameof(WideModeConfiguration),
                typeof(TwoPaneViewWideModeConfiguration),
                typeof(TwoPaneView),
                new PropertyMetadata(TwoPaneViewWideModeConfiguration.LeftRight, OnModePropertyChanged));

        public double MinTallModeHeight
        {
            get => (double)GetValue(MinTallModeHeightProperty);
            set => SetValue(MinTallModeHeightProperty, value);
        }

        public double MinWideModeWidth
        {
            get => (double)GetValue(MinWideModeWidthProperty);
            set => SetValue(MinWideModeWidthProperty, value);
        }

        public TwoPaneViewMode Mode
        {
            get => (TwoPaneViewMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public UIElement Pane1
        {
            get => (UIElement)GetValue(Pane1Property);
            set => SetValue(Pane1Property, value);
        }

        public GridLength Pane1Length
        {
            get => (GridLength)GetValue(Pane1LengthProperty);
            set => SetValue(Pane1LengthProperty, value);
        }

        public UIElement Pane2
        {
            get => (UIElement)GetValue(Pane2Property);
            set => SetValue(Pane2Property, value);
        }

        public GridLength Pane2Length
        {
            get => (GridLength)GetValue(Pane2LengthProperty);
            set => SetValue(Pane2LengthProperty, value);
        }

        public TwoPaneViewPriority PanePriority
        {
            get => (TwoPaneViewPriority)GetValue(PanePriorityProperty);
            set => SetValue(PanePriorityProperty, value);
        }

        public TwoPaneViewTallModeConfiguration TallModeConfiguration
        {
            get => (TwoPaneViewTallModeConfiguration)GetValue(TallModeConfigurationProperty);
            set => SetValue(TallModeConfigurationProperty, value);
        }

        public TwoPaneViewWideModeConfiguration WideModeConfiguration
        {
            get => (TwoPaneViewWideModeConfiguration)GetValue(WideModeConfigurationProperty);
            set => SetValue(WideModeConfigurationProperty, value);
        }

        private static void OnMinLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DependencyProperty property = e.Property;
            double value = (double)e.NewValue;
            double clampedValue = Math.Max(0.0, value);
            if (clampedValue != value && !double.IsNaN(clampedValue))
            {
                (d as TwoPaneView).SetValue(property, clampedValue);
                return;
            }
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        public event TypedEventHandler<TwoPaneView, object> ModeChanged;
    }
}
