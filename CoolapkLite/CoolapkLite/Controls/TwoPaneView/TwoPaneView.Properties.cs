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
        public static readonly DependencyProperty MinTallModeHeightProperty = DependencyProperty.Register(nameof(MinTallModeHeight), typeof(double), typeof(TwoPaneView), new PropertyMetadata(641, OnMinTallModeHeightPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="MinWideModeWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinWideModeWidthProperty = DependencyProperty.Register(nameof(MinWideModeWidth), typeof(double), typeof(TwoPaneView), new PropertyMetadata(641, OnMinWideModeWidthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(TwoPaneViewMode), typeof(TwoPaneView), new PropertyMetadata(TwoPaneViewMode.Wide));

        /// <summary>
        /// Identifies the <see cref="Pane1"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane1Property = DependencyProperty.Register(nameof(Pane1), typeof(UIElement), typeof(TwoPaneView), new PropertyMetadata(null, OnPane1PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane1Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane1LengthProperty = DependencyProperty.Register(nameof(Pane1Length), typeof(GridLength), typeof(TwoPaneView), new PropertyMetadata(new GridLength(1, GridUnitType.Auto), OnPane1LengthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane2"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane2Property = DependencyProperty.Register(nameof(Pane2), typeof(UIElement), typeof(TwoPaneView), new PropertyMetadata(null, OnPane2PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Pane2Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Pane2LengthProperty = DependencyProperty.Register(nameof(Pane2Length), typeof(GridLength), typeof(TwoPaneView), new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnPane2LengthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="PanePriority"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PanePriorityProperty = DependencyProperty.Register(nameof(PanePriority), typeof(TwoPaneViewPriority), typeof(TwoPaneView), new PropertyMetadata(TwoPaneViewPriority.Pane1, OnPanePriorityPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="TallModeConfiguration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TallModeConfigurationProperty = DependencyProperty.Register(nameof(TallModeConfiguration), typeof(TwoPaneViewTallModeConfiguration), typeof(TwoPaneView), new PropertyMetadata(TwoPaneViewTallModeConfiguration.TopBottom, OnTallModeConfigurationPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="WideModeConfiguration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WideModeConfigurationProperty = DependencyProperty.Register(nameof(WideModeConfiguration), typeof(TwoPaneViewWideModeConfiguration), typeof(TwoPaneView), new PropertyMetadata(TwoPaneViewWideModeConfiguration.LeftRight, OnWideModeConfigurationPropertyChanged));

        public double MinTallModeHeight
        {
            get { return (double)GetValue(MinTallModeHeightProperty); }
            set { SetValue(MinTallModeHeightProperty, value); }
        }

        public double MinWideModeWidth
        {
            get { return (double)GetValue(MinWideModeWidthProperty); }
            set { SetValue(MinWideModeWidthProperty, value); }
        }

        public TwoPaneViewMode Mode
        {
            get { return (TwoPaneViewMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public UIElement Pane1
        {
            get { return (UIElement)GetValue(Pane1Property); }
            set { SetValue(Pane1Property, value); }
        }

        public GridLength Pane1Length
        {
            get { return (GridLength)GetValue(Pane1LengthProperty); }
            set { SetValue(Pane1LengthProperty, value); }
        }

        public UIElement Pane2
        {
            get { return (UIElement)GetValue(Pane2Property); }
            set { SetValue(Pane2Property, value); }
        }

        public GridLength Pane2Length
        {
            get { return (GridLength)GetValue(Pane2LengthProperty); }
            set { SetValue(Pane2LengthProperty, value); }
        }

        public TwoPaneViewPriority PanePriority
        {
            get { return (TwoPaneViewPriority)GetValue(PanePriorityProperty); }
            set { SetValue(PanePriorityProperty, value); }
        }

        public TwoPaneViewTallModeConfiguration TallModeConfiguration
        {
            get { return (TwoPaneViewTallModeConfiguration)GetValue(TallModeConfigurationProperty); }
            set { SetValue(TallModeConfigurationProperty, value); }
        }

        public TwoPaneViewWideModeConfiguration WideModeConfiguration
        {
            get { return (TwoPaneViewWideModeConfiguration)GetValue(WideModeConfigurationProperty); }
            set { SetValue(WideModeConfigurationProperty, value); }
        }

        private static void OnMinTallModeHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DependencyProperty property = e.Property;
            double value = (double)e.NewValue;
            double clampedValue = Math.Max(0.0, value);
            if (clampedValue != value)
            {
                (d as TwoPaneView).SetValue(property, clampedValue);
                return;
            }
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnMinWideModeWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DependencyProperty property = e.Property;
            double value = (double)e.NewValue;
            double clampedValue = Math.Max(0.0, value);
            if (clampedValue != value)
            {
                (d as TwoPaneView).SetValue(property, clampedValue);
                return;
            }
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnPane1PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnPane1LengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnPane2PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnPane2LengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnPanePriorityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnTallModeConfigurationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        private static void OnWideModeConfigurationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TwoPaneView).UpdateMode();
        }

        public event TypedEventHandler<TwoPaneView, object> ModeChanged;
    }
}
