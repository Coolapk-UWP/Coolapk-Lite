using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using System;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "TopHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ListViewHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    public class ShyHeaderListView : ListView
    {
        private Grid _topHeader;
        private Grid _listViewHeader;
        private ScrollViewer _scrollViewer;

        private double _topHeight;
        private CompositionPropertySet _propSet;
        private ScrollProgressProvider _progressProvider;
        private readonly bool HasGetElementVisual = SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor) && ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementVisual");

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
            if (HasGetElementVisual)
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

            _topHeader = (Grid)GetTemplateChild("TopHeader");
            _listViewHeader = (Grid)GetTemplateChild("ListViewHeader");
            _scrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");

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
                    RegisterScrollViewer();
                }
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded += ListViewHeader_Loaded;
                _listViewHeader.Unloaded += ListViewHeader_Unloaded;
            }
            base.OnApplyTemplate();
        }

        private void RegisterScrollViewer()
        {
            TranslateTransform transform = new TranslateTransform();
            BindingOperations.SetBinding(transform, TranslateTransform.YProperty, new Binding
            {
                Source = _scrollViewer,
                Mode = BindingMode.OneWay,
                Converter = new VerticalOffsetConverter(this),
                Path = new PropertyPath(nameof(_scrollViewer.VerticalOffset))
            });
            _listViewHeader.RenderTransform = transform;
        }

        private void ProgressProvider_ProgressChanged(object sender, double args)
        {
            _ = args == 1 || _progressProvider.Threshold == 0
                ? VisualStateManager.GoToState(this, "OnThreshold", true)
                : VisualStateManager.GoToState(this, "BeforeThreshold", true);
        }

        private void TopHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid TopHeader = sender as Grid;
            _topHeight = Math.Max(0, TopHeader.ActualHeight - HeaderMargin);
            if (HasGetElementVisual)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
                _progressProvider.Threshold = _topHeight;
                _propSet = _propSet ?? Window.Current.Compositor.CreatePropertySet();
                _propSet.InsertScalar("height", (float)_topHeight);
            }
            _ = _scrollViewer.VerticalOffset >= _topHeight || _topHeight == 0
                ? VisualStateManager.GoToState(this, "OnThreshold", true)
                : VisualStateManager.GoToState(this, "BeforeThreshold", true);
        }

        private void ListViewHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Grid ListViewHeader = sender as Grid;
            Canvas.SetZIndex(ItemsPanelRoot, -1);

            if (HasGetElementVisual)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }

                Visual _headerVisual = ElementCompositionPreview.GetElementVisual(ListViewHeader);
                CompositionPropertySet _manipulationPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);

                _propSet = _propSet ?? Window.Current.Compositor.CreatePropertySet();
                _propSet.InsertScalar("height", (float)Math.Max(0, _topHeader.ActualHeight - HeaderMargin));

                Compositor _compositor = Window.Current.Compositor;
                ExpressionAnimation _headerAnimation = _compositor.CreateExpressionAnimation("_manipulationPropertySet.Translation.Y > -_propSet.height ? 0: -_propSet.height -_manipulationPropertySet.Translation.Y");

                _headerAnimation.SetReferenceParameter("_propSet", _propSet);
                _headerAnimation.SetReferenceParameter("_manipulationPropertySet", _manipulationPropertySet);

                _headerVisual.StartAnimation("Offset.Y", _headerAnimation);
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

        public class VerticalOffsetConverter : IValueConverter
        {
            public ShyHeaderListView ShyHeaderListView { get; private set; }

            public VerticalOffsetConverter(ShyHeaderListView shyHeaderListView) => ShyHeaderListView = shyHeaderListView;

            public object Convert(object value, Type targetType, object parameter, string language)
            {
                double offset = System.Convert.ToDouble(value);
                UpdateVisualState(offset);
                double result = offset < ShyHeaderListView._topHeight ? 0 : -ShyHeaderListView._topHeight + offset;
                return ConverterTools.Convert(result, targetType);
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                double offset = System.Convert.ToDouble(value);
                double result = offset + ShyHeaderListView._topHeight;
                return ConverterTools.Convert(result, targetType);
            }

            private void UpdateVisualState(double offset)
            {
                _ = offset >= ShyHeaderListView._topHeight || ShyHeaderListView._topHeight == 0
                    ? VisualStateManager.GoToState(ShyHeaderListView, "OnThreshold", true)
                    : VisualStateManager.GoToState(ShyHeaderListView, "BeforeThreshold", true);
            }
        }
    }
}
