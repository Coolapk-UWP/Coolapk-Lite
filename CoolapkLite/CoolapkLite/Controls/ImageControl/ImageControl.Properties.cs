﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls
{
    public partial class ImageControl
    {
        #region Source

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(ImageModel),
                typeof(ImageControl),
                new PropertyMetadata(null, OnSourcePropertyChanged));

        public ImageModel Source
        {
            get => (ImageModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ImageControl)sender).OnSourcePropertyChanged(args);
        }

        #endregion

        #region Stretch

        /// <summary>
        /// Identifies the <see cref="Stretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                nameof(Stretch),
                typeof(Stretch),
                typeof(ImageControl),
                new PropertyMetadata(Stretch.UniformToFill));

        /// <summary>
        /// Gets or sets the stretch behavior of the image
        /// </summary>
        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        public new CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(UIElementHelper.CornerRadiusProperty);
            set => SetValue(UIElementHelper.CornerRadiusProperty, value);
        }

        #endregion

        #region EnableDrag

        /// <summary>
        /// Identifies the <see cref="EnableDrag"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableDragProperty =
            DependencyProperty.Register(
                nameof(EnableDrag),
                typeof(bool),
                typeof(ImageControl),
                new PropertyMetadata(false, OnEnableDragPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets is drag enable.
        /// </summary>
        public bool EnableDrag
        {
            get => (bool)GetValue(EnableDragProperty);
            set => SetValue(EnableDragProperty, value);
        }

        private static void OnEnableDragPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ImageControl control)
            {
                control.OnEnableDragPropertyChanged((bool)args.NewValue);
            }
        }

        #endregion

        #region EnableLazyLoading

        /// <summary>
        /// Identifies the <see cref="EnableLazyLoading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableLazyLoadingProperty =
            DependencyProperty.Register(
                nameof(EnableLazyLoading),
                typeof(bool),
                typeof(ImageControl),
                new PropertyMetadata(false, OnEnableLazyLoadingPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets is lazy loading enable.
        /// </summary>
        public bool EnableLazyLoading
        {
            get => (bool)GetValue(EnableLazyLoadingProperty);
            set => SetValue(EnableLazyLoadingProperty, value);
        }

        private static void OnEnableLazyLoadingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ImageControl control)
            {
                bool value = (bool)args.NewValue;
                if (value && control.IsEnableLazyLoading)
                {
                    control.LayoutUpdated -= control.ImageControl_LayoutUpdated;
                    control.LayoutUpdated += control.ImageControl_LayoutUpdated;
                    control.InvalidateLazyLoading();
                }
                else
                {
                    control.LayoutUpdated -= control.ImageControl_LayoutUpdated;
                }
            }
        }

        #endregion

        #region LazyLoadingThreshold

        /// <summary>
        /// Identifies the <see cref="LazyLoadingThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LazyLoadingThresholdProperty =
            DependencyProperty.Register(
                nameof(LazyLoadingThreshold),
                typeof(double),
                typeof(ImageControl),
                new PropertyMetadata(3d, OnLazyLoadingThresholdPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating the threshold for triggering lazy loading.
        /// </summary>
        public double LazyLoadingThreshold
        {
            get => (double)GetValue(LazyLoadingThresholdProperty);
            set => SetValue(LazyLoadingThresholdProperty, value);
        }

        private static void OnLazyLoadingThresholdPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ImageControl control && control.EnableLazyLoading)
            {
                control.InvalidateLazyLoading();
            }
        }

        #endregion

        #region TemplateSettings

        /// <summary>
        /// Identifies the <see cref="TemplateSettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(ImageControlTemplateSettings),
                typeof(ImageControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TemplateSettings.
        /// </summary>
        public ImageControlTemplateSettings TemplateSettings
        {
            get => (ImageControlTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsProperty, value);
        }

        #endregion
    }
}
