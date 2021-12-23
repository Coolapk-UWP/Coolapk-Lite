using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Media
{
    /// <summary>
    /// The <see cref="BackdropBlurBrush"/> is a <see cref="Brush"/> that blurs whatever is behind it in the application.
    /// </summary>
    public class BackdropBlurBrush : XamlCompositionBrushBase
    {
        /// <summary>
        /// Identifies the <see cref="Amount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
            nameof(Amount),
            typeof(double),
            typeof(BackdropBlurBrush),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnAmountChanged)));

        /// <summary>
        /// Gets or sets the amount of gaussian blur to apply to the background.
        /// </summary>
        public double Amount
        {
            get { return (double)GetValue(AmountProperty); }
            set { SetValue(AmountProperty, value); }
        }

        private static void OnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            // Unbox and set a new blur amount if the CompositionBrush exists.
            brush.CompositionBrush?.Properties.InsertScalar("Blur.BlurAmount", (float)(double)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the tint for the effect
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TintColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register(
            nameof(TintColor),
            typeof(Color),
            typeof(BackdropBlurBrush),
            new PropertyMetadata(default(Color), OnTintColorPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintColor"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintColorProperty"/></param>
        private static void OnTintColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.CompositionBrush?.Properties.InsertColor("TintColor.Color", (Color)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the background source mode for the effect (the default is <see cref="BlurBackgroundSource.Backdrop"/>).
        /// </summary>
        public BlurBackgroundSource BackgroundSource
        {
            get => (BlurBackgroundSource)GetValue(BackgroundSourceProperty);
            set => SetValue(BackgroundSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="BackgroundSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundSourceProperty = DependencyProperty.Register(
            nameof(BackgroundSource),
            typeof(BlurBackgroundSource),
            typeof(BackdropBlurBrush),
            new PropertyMetadata(BlurBackgroundSource.Backdrop, OnSourcePropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="BackgroundSource"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="BackgroundSourceProperty"/></param>
        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.OnDisconnected();
            brush.OnConnected();
        }

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.5, must be in the [0, 1] range)
        /// </summary>
        public double TintOpacity
        {
            get => (double)GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register(
            nameof(TintOpacity),
            typeof(double),
            typeof(BackdropBlurBrush),
            new PropertyMetadata(0.5, OnTintOpacityPropertyChanged));

        /// <summary>
        /// Updates the UI when <see cref="TintOpacity"/> changes
        /// </summary>
        /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TintOpacityProperty"/></param>
        private static void OnTintOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackdropBlurBrush brush = (BackdropBlurBrush)d;

            brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source1Amount", (float)(1 - (double)e.NewValue));
            brush.CompositionBrush?.Properties.InsertScalar("Arithmetic.Source2Amount", (float)(double)e.NewValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackdropBlurBrush"/> class.
        /// </summary>
        public BackdropBlurBrush()
        {
        }

        /// <summary>
        /// Initializes the Composition Brush.
        /// </summary>
        protected override void OnConnected()
        {
            // Delay creating composition resources until they're required.
            if (CompositionBrush == null)
            {
                // Abort if effects aren't supported.
                if (!CompositionCapabilities.GetForCurrentView().AreEffectsSupported())
                {
                    return;
                }

                CompositionBackdropBrush backdrop;

                switch (BackgroundSource)
                {
                    case BlurBackgroundSource.Backdrop:
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
                    case BlurBackgroundSource.HostBackdrop:
                        backdrop = Window.Current.Compositor.CreateHostBackdropBrush();
                        break;
                    default:
                        backdrop = Window.Current.Compositor.CreateBackdropBrush();
                        break;
                }

                // Use a Win2D blur affect applied to a CompositionBackdropBrush.
                GaussianBlurEffect graphicsEffect = new GaussianBlurEffect
                {
                    Name = "Blur",
                    BlurAmount = (float)Amount,
                    Source = new ArithmeticCompositeEffect
                    {
                        Name = "Arithmetic",
                        MultiplyAmount = 0,
                        Source1Amount = (float)(1 - TintOpacity),
                        Source2Amount = (float)TintOpacity,
                        Source1 = new CompositionEffectSourceParameter("backdrop"),
                        Source2 = new ColorSourceEffect
                        {
                            Name = "TintColor",
                            Color = TintColor
                        }
                    }
                };

                CompositionEffectFactory effectFactory = Window.Current.Compositor.CreateEffectFactory(graphicsEffect, new[] { "Blur.BlurAmount", "Arithmetic.Source1Amount", "Arithmetic.Source2Amount", "TintColor.Color" });
                CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

                effectBrush.SetSourceParameter("backdrop", backdrop);

                CompositionBrush = effectBrush;
            }
        }

        /// <summary>
        /// Deconstructs the Composition Brush.
        /// </summary>
        protected override void OnDisconnected()
        {
            // Dispose of composition resources when no longer in use.
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }
        }
    }

    public enum BlurBackgroundSource
    {
        //
        // 摘要:
        //     画笔从应用窗口后面的内容采样。
        HostBackdrop = 0,
        //
        // 摘要:
        //     画笔从应用内容采样。
        Backdrop = 1
    }
}
