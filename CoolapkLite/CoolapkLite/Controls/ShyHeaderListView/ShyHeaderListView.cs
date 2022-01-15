using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Providers;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "TopHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ListViewHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    public sealed class ShyHeaderListView : ListView
    {
        private Grid _topHeader;
        private Grid _listViewHeader;
        private ScrollViewer _scrollViewer;

        private CompositionPropertySet _propSet;

        public static readonly DependencyProperty TopHeaderProperty = DependencyProperty.Register(
           "TopHeader",
           typeof(object),
           typeof(ShyHeaderListView),
           null);

        public static readonly DependencyProperty LeftHeaderProperty = DependencyProperty.Register(
           "LeftHeader",
           typeof(object),
           typeof(ShyHeaderListView),
           null);

        public static readonly DependencyProperty RightHeaderProperty = DependencyProperty.Register(
           "RightHeader",
           typeof(object),
           typeof(ShyHeaderListView),
           null);

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
           "HeaderHeight",
           typeof(double),
           typeof(ShyHeaderListView),
           new PropertyMetadata(double.NaN, null));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
           "HeaderBackground",
           typeof(Brush),
           typeof(ShyHeaderListView),
           new PropertyMetadata(UIHelper.ApplicationPageBackgroundThemeElementBrush(), null));

        public static readonly DependencyProperty TopHeaderBackgroundProperty = DependencyProperty.Register(
           "TopHeaderBackground",
           typeof(Brush),
           typeof(ShyHeaderListView),
           null);

        public new object Header
        {
            get => _listViewHeader;
        }

        public object TopHeader
        {
            get => GetValue(TopHeaderProperty);
            set => SetValue(TopHeaderProperty, value);
        }

        public object LeftHeader
        {
            get => GetValue(LeftHeaderProperty);
            set => SetValue(LeftHeaderProperty, value);
        }

        public object RightHeader
        {
            get => GetValue(RightHeaderProperty);
            set => SetValue(RightHeaderProperty, value);
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
            this.DefaultStyleKey = typeof(ShyHeaderListView);

            ScrollViewerExtensions.SetEnableMiddleClickScrolling(this, true);
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
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded += ListViewHeader_Loaded;
            }

            base.OnApplyTemplate();
        }

        private void TopHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid TopHeader = sender as Grid;
            _propSet ??= Window.Current.Compositor.CreatePropertySet();
            _propSet.InsertScalar("height", -(float)TopHeader.ActualHeight);
        }

        private void ListViewHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Grid ListViewHeader = sender as Grid;
            Canvas.SetZIndex(ItemsPanelRoot, -1);
            //原本ListViewBase里Header是在ItemsPanelRoot下方的，使用Canvans.SetZIndex把ItemsPanelRoot设置到下方

            Visual _headerVisual = ElementCompositionPreview.GetElementVisual(ListViewHeader);
            CompositionPropertySet _manipulationPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);
            
            _propSet ??= Window.Current.Compositor.CreatePropertySet();
            _propSet.InsertScalar("height", -(float)_topHeader?.ActualHeight);

            Compositor _compositor = Window.Current.Compositor;
            ExpressionAnimation _headerAnimation = _compositor.CreateExpressionAnimation("_manipulationPropertySet.Translation.Y > _propSet.height ? 0: _propSet.height -_manipulationPropertySet.Translation.Y");
            //_manipulationPropertySet.Translation.Y是ScrollViewer滚动的数值，手指向上移动的时候，也就是可视部分向下移动的时候，Translation.Y是负数。

            _headerAnimation.SetReferenceParameter("_propSet", _propSet);
            _headerAnimation.SetReferenceParameter("_manipulationPropertySet", _manipulationPropertySet);

            _headerVisual.StartAnimation("Offset.Y", _headerAnimation);
        }

        private async Task<T> WaitForLoaded<T>(FrameworkElement element, Func<T> func, Predicate<T> pre, CancellationToken cancellationToken)
        {
            TaskCompletionSource<T> tcs = null;
            try
            {
                tcs = new TaskCompletionSource<T>();
                cancellationToken.ThrowIfCancellationRequested();
                var result = func.Invoke();
                if (pre(result)) return result;


                element.Loaded += Element_Loaded;

                return await tcs.Task;

            }
            catch
            {
                element.Loaded -= Element_Loaded;
                var result = func.Invoke();
                if (pre(result)) return result;
            }

            return default;


            void Element_Loaded(object sender, RoutedEventArgs e)
            {
                if (tcs == null) return;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    element.Loaded -= Element_Loaded;
                    var _result = func.Invoke();
                    if (pre(_result)) tcs.SetResult(_result);
                    else tcs.SetCanceled();
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("canceled");
                }
            }

        }
    }
}
