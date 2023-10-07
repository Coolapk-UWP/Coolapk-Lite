using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    public partial class ImageControl : Control
    {
        private bool _isLoaded = false;
        private bool _isImageLoaded = false;

        private readonly bool IsUseNoPicFallback = SettingsHelper.Get<bool>(SettingsHelper.IsUseNoPicFallback);
        private readonly bool IsEnableLazyLoading = SettingsHelper.Get<bool>(SettingsHelper.IsEnableLazyLoading);

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
            if (IsUseNoPicFallback)
            {
                _ = VisualStateManager.GoToState(this, LoadedState, false);
                _isLoaded = true;
            }
            else
            {
                _ = VisualStateManager.GoToState(this, LoadingState, false);
                _isLoaded = false;
            }

            if (!(IsEnableLazyLoading && EnableLazyLoading))
            {
                SetSource();
            }

            base.OnApplyTemplate();
        }

        private void ImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            if (!(IsEnableLazyLoading && EnableLazyLoading))
            {
                SetSource();
            }
        }

        private void ImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
            if (IsEnableLazyLoading && EnableLazyLoading)
            {
                RemoveSource();
            }
        }

        private void OnSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (IsUseNoPicFallback)
            {
                if (IsEnableLazyLoading && EnableLazyLoading)
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
                if (args.OldValue is ImageModel oldValue)
                {
                    oldValue.LoadStarted -= ImageModel_LoadStarted;
                    oldValue.LoadCompleted -= ImageModel_LoadCompleted;
                    oldValue.NoPicChanged -= ImageModel_NoPicChanged;
                }

                if (args.NewValue is ImageModel newValue && !newValue.IsEmpty)
                {
                    newValue.LoadStarted -= ImageModel_LoadStarted;
                    newValue.LoadStarted += ImageModel_LoadStarted;
                    newValue.LoadCompleted -= ImageModel_LoadCompleted;
                    newValue.LoadCompleted += ImageModel_LoadCompleted;
                    newValue.NoPicChanged -= ImageModel_NoPicChanged;
                    newValue.NoPicChanged += ImageModel_NoPicChanged;

                    if (IsEnableLazyLoading && EnableLazyLoading)
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
                    _ = UpdateState(false);
                }
            }
        }

        private void ImageModel_LoadStarted(ImageModel sender, object args) => _ = UpdateState(false);

        private void ImageModel_LoadCompleted(ImageModel sender, object args) => _ = UpdateState(!sender.IsNoPic);

        private void ImageModel_NoPicChanged(ImageModel sender, bool args) => _ = UpdateState(!args);

        private void ImageControl_LayoutUpdated(object sender, object e) => InvalidateLazyLoading();

        private void InvalidateLazyLoading()
        {
            if (Source == null || Source.IsEmpty || !(ApiInfoHelper.IsFrameworkElementIsLoadedSupported ? IsLoaded : _isLoaded))
            {
                return;
            }

            // Find the first ascendant ScrollViewer, if not found, use the root element.
            IEnumerable<FrameworkElement> ascendants =
                this.FindAscendants()
                    .OfType<FrameworkElement>()
                    .Reverse();

            FrameworkElement hostElement =
                ascendants.Skip(1)
                          .OfType<ScrollViewer>(x => x.VerticalScrollMode != ScrollMode.Disabled || x.HorizontalScrollMode != ScrollMode.Disabled)
                          .FirstOrDefault() ?? ascendants.FirstOrDefault();

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
            if (Source == null || Source.IsEmpty)
            {
                RemoveSource();
                return;
            }

            if (TemplateSettings.ActualSource?.Uri != Source.Uri)
            {
                TemplateSettings.ActualSource = Source;
            }

            if (Source.IsLoaded)
            {
                _ = UpdateState(true);
            }
            else if (_isLoaded && !Source.IsLoading)
            {
                _ = Source.Refresh();
            }
        }

        private void RemoveSource()
        {
            TemplateSettings.ActualSource = null;
            _ = UpdateState(false);
        }

        private async Task UpdateState(bool isLoaded, bool useTransitions = true)
        {
            if (IsUseNoPicFallback) { return; }
            if (_isImageLoaded != isLoaded)
            {
                _isImageLoaded = isLoaded;
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                _ = VisualStateManager.GoToState(this, isLoaded ? LoadedState : LoadingState, useTransitions);
            }
        }
    }
}
