using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    public partial class ImageControl : Control
    {
        private bool _isLoaded = false;

        /// <summary>
        /// VisualStates name in template
        /// </summary>
        protected const string CommonGroup = "CommonStates";

        /// <summary>
        /// Loading state name in template
        /// </summary>
        protected const string LoadingState = "Loading";

        /// <summary>
        /// Loaded state name in template
        /// </summary>
        protected const string LoadedState = "Loaded";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageControl"/> class.
        /// </summary>
        public ImageControl()
        {
            DefaultStyleKey = typeof(ImageControl);
            SetValue(TemplateSettingsProperty, new ImageControlTemplateSettings());
            Loaded += ImageControl_Loaded;
            Unloaded += ImageControl_Unloaded;
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            _ = VisualStateManager.GoToState(this, LoadingState, false);

            if (!EnableLazyLoading)
            {
                SetSource();
            }

            base.OnApplyTemplate();
        }

        private void ImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            if (!EnableLazyLoading)
            {
                SetSource();
            }
        }

        private void ImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
            RemoveSource();
        }

        private void OnSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is ImageModel oldValue)
            {
                oldValue.LoadStarted -= ImageModel_LoadStarted;
                oldValue.LoadCompleted -= ImageModel_LoadCompleted;
                oldValue.NoPicChanged -= ImageModel_NoPicChanged;
            }

            if (args.NewValue is ImageModel newValue)
            {
                newValue.LoadStarted -= ImageModel_LoadStarted;
                newValue.LoadStarted += ImageModel_LoadStarted;
                newValue.LoadCompleted -= ImageModel_LoadCompleted;
                newValue.LoadCompleted += ImageModel_LoadCompleted;
                newValue.NoPicChanged -= ImageModel_NoPicChanged;
                newValue.NoPicChanged += ImageModel_NoPicChanged;

                if (EnableLazyLoading)
                {
                    InvalidateLazyLoading();
                }
                else
                {
                    SetSource();
                }
            }
            else
            {
                _ = VisualStateManager.GoToState(this, LoadingState, true);
            }
        }

        private async void ImageModel_LoadStarted(ImageModel sender, object args)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            _ = VisualStateManager.GoToState(this, LoadingState, true);
        }

        private async void ImageModel_LoadCompleted(ImageModel sender, object args)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            _ = VisualStateManager.GoToState(this, sender.IsNoPic ? LoadingState : LoadedState, true);
        }

        private async void ImageModel_NoPicChanged(ImageModel sender, bool args)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            _ = VisualStateManager.GoToState(this, args ? LoadingState : LoadedState, true);
        }

        private void ImageControl_LayoutUpdated(object sender, object e)
        {
            InvalidateLazyLoading();
        }

        private void InvalidateLazyLoading()
        {
            if (Source == null || !(ApiInfoHelper.IsFrameworkElementIsLoadedSupported ? IsLoaded : _isLoaded))
            {
                return;
            }

            // Find the first ascendant ScrollViewer, if not found, use the root element.
            IEnumerable<FrameworkElement> ascendants = this.FindAscendants().OfType<FrameworkElement>().Reverse();
            FrameworkElement hostElement = ascendants.FirstOrDefault(x => x is ScrollViewer) ?? ascendants.FirstOrDefault();

            if (hostElement == null)
            {
                return;
            }

            Rect controlRect = TransformToVisual(hostElement)
                .TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));
            double lazyLoadingThreshold = LazyLoadingThreshold;
            double halfThreshold = (1 - lazyLoadingThreshold) / 2;
            Rect hostRect = new Rect(
                hostElement.ActualWidth * halfThreshold,
                hostElement.ActualHeight * halfThreshold,
                hostElement.ActualWidth * lazyLoadingThreshold,
                hostElement.ActualHeight * lazyLoadingThreshold);

            if (controlRect.IntersectsWith(hostRect))
            {
                SetSource();
            }
            else
            {
                RemoveSource();
            }
        }

        private void SetSource()
        {
            if (TemplateSettings.ActualSource?.Equals(Source) != true)
            {
                TemplateSettings.ActualSource = Source;
            }

            if (Source?.IsLoaded == true)
            {
                _ = VisualStateManager.GoToState(this, LoadedState, true);
            }
        }

        private void RemoveSource()
        {
            TemplateSettings.ActualSource = null;
            _ = VisualStateManager.GoToState(this, LoadingState, true);
        }
    }
}
