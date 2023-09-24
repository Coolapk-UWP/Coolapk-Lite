using CoolapkLite.Common;
using CoolapkLite.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        private long? _token;
        private bool _isLoading;
        private double _topHeight;
        private bool? _isThreshold;
        private ScrollProgressProvider _progressProvider;
        private readonly bool IsUseCompositor = SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor) && ApiInfoHelper.IsGetElementVisualSupported;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShyHeaderListView"/> class.
        /// </summary>
        public ShyHeaderListView()
        {
            DefaultStyleKey = typeof(ShyHeaderListView);
            if (IsUseCompositor)
            {
                _progressProvider = new ScrollProgressProvider();
                _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
            }
            Loaded += ListView_Loaded;
            Unloaded += ListView_Unloaded;
            _ = RegisterPropertyChangedCallback(ItemsPanelProperty, OnItemsPanelPropertyChanged);
        }

        #region TopHeader

        public static readonly DependencyProperty TopHeaderProperty =
            DependencyProperty.Register(
                nameof(TopHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public object TopHeader
        {
            get => GetValue(TopHeaderProperty);
            set => SetValue(TopHeaderProperty, value);
        }

        #endregion

        #region FlyoutHeader

        public static readonly DependencyProperty FlyoutHeaderProperty =
            DependencyProperty.Register(
                nameof(FlyoutHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public object FlyoutHeader
        {
            get => GetValue(FlyoutHeaderProperty);
            set => SetValue(FlyoutHeaderProperty, value);
        }

        #endregion

        #region HeaderMargin

        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(0d));

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        #endregion

        #region HeaderHeight

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(double.NaN));

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        #endregion

        #region HeaderBackground

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        #endregion

        #region TopHeaderBackground

        public static readonly DependencyProperty TopHeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(TopHeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public Brush TopHeaderBackground
        {
            get => (Brush)GetValue(TopHeaderBackgroundProperty);
            set => SetValue(TopHeaderBackgroundProperty, value);
        }

        #endregion

        #region LoadingThreshold

        public static readonly DependencyProperty LoadingThresholdProperty =
            DependencyProperty.Register(
                nameof(LoadingThreshold),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(0.5));

        public double LoadingThreshold
        {
            get => (double)GetValue(LoadingThresholdProperty);
            set => SetValue(LoadingThresholdProperty, value);
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            if (_topHeader != null)
            {
                _topHeader.SizeChanged -= TopHeader_SizeChanged;
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded -= ListView_Loaded;
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

        private void UpdateIncrementalLoading()
        {
            if (ItemsPanelRoot != null)
            {
                if (ItemsPanelRoot is ItemsStackPanel
                    || ItemsPanelRoot is ItemsWrapGrid
                    || ItemsPanelRoot is VirtualizingPanel)
                {
                    IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge;
                    if (_scrollViewer != null)
                    {
                        _scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
                        _scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                    }
                }
                else
                {
                    IncrementalLoadingTrigger = IncrementalLoadingTrigger.None;
                    if (_scrollViewer != null)
                    {
                        _scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
                        _scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                        _scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
                        _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                    }
                }
            }
        }

        private void OnItemsPanelPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateIncrementalLoading();
        }

        private void OnVerticalOffsetPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateVisualState((double)sender.GetValue(dp) >= _topHeight || _topHeight == 0);
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer)) { return; }
            LoadMoreItems(scrollViewer);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer)) { return; }
            LoadMoreItems(scrollViewer);
        }

        public async void LoadMoreItems(ScrollViewer scrollViewer)
        {
            if (!(ItemsSource is ISupportIncrementalLoading source)) { return; }
            check:
            if (Items.Count > 0 && !source.HasMoreItems) { return; }
            if (_isLoading) { return; }
            if (((scrollViewer.ExtentHeight - scrollViewer.VerticalOffset) / scrollViewer.ViewportHeight) - 1.0 <= LoadingThreshold)
            {
                try
                {
                    _isLoading = true;
                    await source.LoadMoreItemsAsync(20);
                    goto check;
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(ShyHeaderListView)).Error(ex.ExceptionToMessage(), ex);
                }
                finally
                {
                    _isLoading = false;
                }
            }
        }

        private void ProgressProvider_ProgressChanged(object sender, double args)
        {
            UpdateVisualState(args == 1 || _progressProvider.Threshold == 0);
        }

        private void TopHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid TopHeader = sender as Grid;
            if (TopHeader.ActualHeight == 0)
            {
                if (_progressProvider != null)
                {
                    _progressProvider.ProgressChanged -= ProgressProvider_ProgressChanged;
                    _progressProvider = null;
                }
                if (_token.HasValue)
                {
                    _scrollViewer.UnregisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, _token.Value);
                }
                UpdateVisualState(true);
            }
            else
            {
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
                else if (!_token.HasValue)
                {
                    _token = _scrollViewer.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, OnVerticalOffsetPropertyChanged);
                }
                UpdateVisualState(_scrollViewer.VerticalOffset >= _topHeight || _topHeight == 0);
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
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
            UpdateIncrementalLoading();
        }

        private void ListView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_progressProvider != null)
            {
                _progressProvider.ProgressChanged -= ProgressProvider_ProgressChanged;
                _progressProvider = null;
            }
        }
    }
}
