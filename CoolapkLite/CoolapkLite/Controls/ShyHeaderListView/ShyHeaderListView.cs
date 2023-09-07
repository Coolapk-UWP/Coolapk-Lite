using CoolapkLite.Common;
using CoolapkLite.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "TopHeader", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "FlyoutHeader", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "ListViewHeader", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "InnerFlyoutHeaderGrid", Type = typeof(Panel))]
    [TemplatePart(Name = "OuterFlyoutHeaderGrid", Type = typeof(Panel))]
    public class ShyHeaderListView : ListView
    {
        private FrameworkElement _topHeader;
        private FrameworkElement _flyoutHeader;
        private FrameworkElement _listViewHeader;
        private ScrollViewer _scrollViewer;
        private Panel _innerFlyoutHeaderGrid;
        private Panel _outerFlyoutHeaderGrid;

        private double _topHeight;
        private bool? _isThreshold;
        private ScrollProgressProvider _progressProvider;
        private readonly bool IsUseCompositor = SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor) && ApiInfoHelper.IsGetElementVisualSupported;

        public static readonly DependencyProperty TopHeaderProperty =
            DependencyProperty.Register(
                nameof(TopHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty FlyoutHeaderProperty =
            DependencyProperty.Register(
                nameof(FlyoutHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty TopHeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(TopHeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public object TopHeader
        {
            get => GetValue(TopHeaderProperty);
            set => SetValue(TopHeaderProperty, value);
        }

        public object FlyoutHeader
        {
            get => GetValue(FlyoutHeaderProperty);
            set => SetValue(FlyoutHeaderProperty, value);
        }

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public Brush TopHeaderBackground
        {
            get => (Brush)GetValue(TopHeaderBackgroundProperty);
            set => SetValue(TopHeaderBackgroundProperty, value);
        }

        public ShyHeaderListView()
        {
            DefaultStyleKey = typeof(ShyHeaderListView);
            if (IsUseCompositor)
            {
                _progressProvider = new ScrollProgressProvider();
                _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
            }
        }

        protected override void OnApplyTemplate()
        {
            if (_topHeader != null)
            {
                _topHeader.SizeChanged -= TopHeader_SizeChanged;
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded -= ListViewHeader_Loaded;
            }

            _topHeader = (FrameworkElement)GetTemplateChild("TopHeader");
            _flyoutHeader = (FrameworkElement)GetTemplateChild("FlyoutHeader");
            _listViewHeader = (FrameworkElement)GetTemplateChild("ListViewHeader");
            _scrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");
            _innerFlyoutHeaderGrid = (Panel)GetTemplateChild("InnerFlyoutHeaderGrid");
            _outerFlyoutHeaderGrid = (Panel)GetTemplateChild("OuterFlyoutHeaderGrid");

            if (_topHeader != null)
            {
                _topHeader.SizeChanged += TopHeader_SizeChanged;
            }
            if (_scrollViewer != null)
            {
                if (_progressProvider != null)
                {
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
                else
                {
                    _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                }
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded += ListViewHeader_Loaded;
                _listViewHeader.Unloaded += ListViewHeader_Unloaded;
            }
            base.OnApplyTemplate();
        }

        private void UpdateVisualState(bool isThreshold)
        {
            _ = isThreshold ? VisualStateManager.GoToState(this, "OnThreshold", true)
                : VisualStateManager.GoToState(this, "BeforeThreshold", true);

            if (_isThreshold != isThreshold)
            {
                _isThreshold = isThreshold;

                if (_flyoutHeader.Parent != null)
                {
                    (_flyoutHeader.Parent as Panel).Children.Remove(_flyoutHeader);
                }
                else
                {
                    _innerFlyoutHeaderGrid.Children.Remove(_flyoutHeader);
                    _outerFlyoutHeaderGrid.Children.Remove(_flyoutHeader);
                }

                if (isThreshold)
                {
                    _outerFlyoutHeaderGrid.Children.Add(_flyoutHeader);
                }
                else
                {
                    _innerFlyoutHeaderGrid.Children.Add(_flyoutHeader);
                }
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateVisualState(_scrollViewer.VerticalOffset >= _topHeight || _topHeight == 0);
        }

        private void ProgressProvider_ProgressChanged(object sender, double args)
        {
            UpdateVisualState(args == 1 || _progressProvider.Threshold == 0);
        }

        private void TopHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid TopHeader = sender as Grid;
            _topHeight = Math.Max(0, TopHeader.ActualHeight - HeaderMargin);
            if (IsUseCompositor)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
                _progressProvider.Threshold = _topHeight;
            }
            UpdateVisualState(_scrollViewer.VerticalOffset >= _topHeight || _topHeight == 0);
        }

        private void ListViewHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Grid ListViewHeader = sender as Grid;
            Canvas.SetZIndex(ItemsPanelRoot, -1);

            if (IsUseCompositor)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
            }
            else
            {
                _topHeight = Math.Max(0, _topHeader.ActualHeight - HeaderMargin);
            }
        }

        private void ListViewHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_progressProvider != null)
            {
                _progressProvider.ProgressChanged -= ProgressProvider_ProgressChanged;
                _progressProvider = null;
            }
        }
    }
}
