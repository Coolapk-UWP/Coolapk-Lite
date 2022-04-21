using CoolapkLite.Core.Helpers.Providers;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "Header", Type = typeof(Grid))]
    [TemplatePart(Name = "PivotHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ContentPresenter", Type = typeof(ContentPresenter))]
    public sealed class ShyHeaderPivot : ContentControl
    {
        private Grid _header;
        private Grid _pivotHeader;
        private ContentPresenter _contentPresenter;

        private CancellationTokenSource Token;
        private readonly ScrollProgressProvider Provider;
        private SpinLock SpinLock = new SpinLock();
        private readonly HashSet<ScrollViewer> ScrollViewers;

        public static readonly DependencyProperty PivotProperty = DependencyProperty.Register(
           "Pivot",
           typeof(Pivot),
           typeof(ShyHeaderPivot),
           new PropertyMetadata(null, OnPivotPropertyChanged));

        public static readonly DependencyProperty TopPanelProperty = DependencyProperty.Register(
           "TopPanel",
           typeof(object),
           typeof(ShyHeaderPivot),
           null);

        public static readonly DependencyProperty LeftHeaderProperty = DependencyProperty.Register(
           "LeftHeader",
           typeof(object),
           typeof(ShyHeaderPivot),
           null);

        public static readonly DependencyProperty RightHeaderProperty = DependencyProperty.Register(
           "RightHeader",
           typeof(object),
           typeof(ShyHeaderPivot),
           null);

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
           "HeaderHeight",
           typeof(double),
           typeof(ShyHeaderPivot),
           new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
           "HeaderBackground",
           typeof(Brush),
           typeof(ShyHeaderPivot),
           new PropertyMetadata(UIHelper.ApplicationPageBackgroundThemeElementBrush()));

        public static readonly DependencyProperty TopPanelBackgroundProperty = DependencyProperty.Register(
           "TopPanelBackground",
           typeof(Brush),
           typeof(ShyHeaderPivot),
           null);

        public Pivot Pivot
        {
            get => (Pivot)GetValue(PivotProperty);
            set => SetValue(PivotProperty, value);
        }

        public object TopPanel
        {
            get => GetValue(TopPanelProperty);
            set => SetValue(TopPanelProperty, value);
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

        public Brush TopPanelBackground
        {
            get => (Brush)GetValue(TopPanelBackgroundProperty);
            set => SetValue(TopPanelBackgroundProperty, value);
        }

        private static void OnPivotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShyHeaderPivot).UpdatePivot();
        }

        public event TypedEventHandler<object, double> ProgressChanged;

        public ShyHeaderPivot()
        {
            DefaultStyleKey = typeof(ShyHeaderPivot);

            Provider = new ScrollProgressProvider();
            ScrollViewers = new HashSet<ScrollViewer>();
            Provider.ProgressChanged += Provider_ProgressChanged;
            VisualStateManager.GoToState(this, "BeforeThreshold", false);
        }

        protected override void OnApplyTemplate()
        {
            if (_header != null)
            {
                _header.SizeChanged -= Header_SizeChanged;
            }
            if (_pivotHeader != null)
            {
                _pivotHeader.Loaded -= PivotHeader_Loaded;
                _pivotHeader.SizeChanged -= Header_SizeChanged;
            }

            _header = (Grid)GetTemplateChild("Header");
            _pivotHeader = (Grid)GetTemplateChild("PivotHeader");
            _contentPresenter = (ContentPresenter)GetTemplateChild("ContentPresenter");

            if (_header != null)
            {
                _header.SizeChanged += Header_SizeChanged;
            }
            if (_pivotHeader != null)
            {
                _pivotHeader.Loaded += PivotHeader_Loaded;
                _pivotHeader.SizeChanged += Header_SizeChanged;
            }
            if (TopPanelBackground == null)
            {
                TopPanelBackground = Background;
            }

            UpdatePivot();

            base.OnApplyTemplate();
        }

        private async void UpdatePivot()
        {
            if (Pivot == null)
            {
                if (Token != null)
                {
                    Token.Cancel();
                }

                Token = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                Pivot = await WaitForLoaded(_contentPresenter, () => _contentPresenter?.FindDescendant<Pivot>(), c => c != null, Token.Token);
            }

            if (Pivot != null)
            {
                Pivot.PivotItemLoaded -= Pivot_PivotItemLoaded;
                Pivot.PivotItemLoaded += Pivot_PivotItemLoaded;
                Pivot.SelectionChanged -= Pivot_SelectionChanged;
                Pivot.SelectionChanged += Pivot_SelectionChanged;
                Pivot.PivotItemUnloading -= Pivot_PivotItemUnloading;
                Pivot.PivotItemUnloading += Pivot_PivotItemUnloading;

                Pivot_SelectionChanged(Pivot, null);
                await Task.Delay(1);
                Pivot_PivotItemLoaded(Pivot, new PivotItemEventArgs() { Item = (PivotItem)Pivot.SelectedItem });
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem PivotItem = Pivot.ContainerFromItem(Pivot.SelectedItem) as PivotItem;

            if (Token != null)
            {
                Token.Cancel();
            }

            Token = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            FrameworkElement ContentTemplateRoot = await WaitForLoaded(PivotItem, () => PivotItem.ContentTemplateRoot as FrameworkElement, c => c != null, Token.Token);

            Provider.ScrollViewer = ContentTemplateRoot.FindDescendant<ScrollViewer>();

            bool lockTaken = false;
            try
            {
                SpinLock.Enter(ref lockTaken);
                ScrollViewers.Remove(Provider.ScrollViewer);
            }
            finally
            {
                if (lockTaken)
                { SpinLock.Exit(); }
            }
        }

        private void Provider_ProgressChanged(object sender, double args)
        {
            _header.Translation = _pivotHeader.Translation = new Vector3(0f, (float)(-Provider.Threshold * Provider.Progress), 0f);
            bool lockTaken = false;
            try
            {
                SpinLock.Enter(ref lockTaken);
                foreach (ScrollViewer ScrollViewer in ScrollViewers)
                {
                    ScrollViewer.ChangeView(null, Provider.Progress * Provider.Threshold, null, true);
                }
                if (Provider.Progress == 1)
                {
                    VisualStateManager.GoToState(this, "OnThreshold", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "BeforeThreshold", true);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    SpinLock.Exit();
                }
            }
            ProgressChanged?.Invoke(sender, args);
        }

        private void Pivot_PivotItemLoaded(Pivot sender, PivotItemEventArgs args)
        {
            ScrollViewer ScrollViewer = (args.Item.ContentTemplateRoot as FrameworkElement).FindDescendant<ScrollViewer>();
            FrameworkElement FrameworkElement = ScrollViewer.FindDescendant<ScrollContentPresenter>().FindChild<FrameworkElement>();
            if (FrameworkElement != null)
            {
                FrameworkElement.Margin = new Thickness(0, _header.ActualHeight + _pivotHeader.ActualHeight, 0, 0);
            }

            if (ScrollViewer != Provider.ScrollViewer)
            {
                ScrollViewer.ChangeView(null, Provider.Progress * Provider.Threshold, null, true);

                bool lockTaken = false;
                try
                {
                    SpinLock.Enter(ref lockTaken);
                    ScrollViewers.Add(ScrollViewer);
                }
                finally
                {
                    if (lockTaken)
                    {
                        SpinLock.Exit();
                    }
                }
            }
        }

        private void Pivot_PivotItemUnloading(Pivot sender, PivotItemEventArgs args)
        {
            ScrollViewer ScrollViewer = (args.Item.ContentTemplateRoot as FrameworkElement).FindDescendant<ScrollViewer>();

            if (ScrollViewer != null)
            {
                bool lockTaken = false;
                try
                {
                    SpinLock.Enter(ref lockTaken);
                    ScrollViewers.Remove(ScrollViewer);
                }
                finally
                {
                    if (lockTaken)
                    {
                        SpinLock.Exit();
                    }
                }
            }
        }

        private void PivotHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.Threshold = _header.ActualHeight;
        }

        private void Header_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _pivotHeader.Margin = new Thickness(0, _header.ActualHeight, 0, 0);
            bool lockTaken = false;
            try
            {
                SpinLock.Enter(ref lockTaken);
                foreach (ScrollViewer ScrollViewer in ScrollViewers)
                {
                    FrameworkElement FrameworkElement = ScrollViewer.FindDescendant<ScrollContentPresenter>().FindChild<FrameworkElement>();
                    if (FrameworkElement != null)
                    {
                        FrameworkElement.Margin = new Thickness(0, _header.ActualHeight + _pivotHeader.ActualHeight, 0, 0);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    SpinLock.Exit();
                }
            }
        }

        private async Task<T> WaitForLoaded<T>(FrameworkElement element, Func<T> func, Predicate<T> pre, CancellationToken cancellationToken)
        {
            TaskCompletionSource<T> tcs = null;
            try
            {
                tcs = new TaskCompletionSource<T>();
                cancellationToken.ThrowIfCancellationRequested();
                T result = func.Invoke();
                if (pre(result))
                {
                    return result;
                }

                element.Loaded += Element_Loaded;

                return await tcs.Task;

            }
            catch
            {
                element.Loaded -= Element_Loaded;
                T result = func.Invoke();
                if (pre(result))
                {
                    return result;
                }
            }

            return default;


            void Element_Loaded(object sender, RoutedEventArgs e)
            {
                if (tcs == null)
                {
                    return;
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    element.Loaded -= Element_Loaded;
                    T _result = func.Invoke();
                    if (pre(_result))
                    {
                        tcs.SetResult(_result);
                    }
                    else
                    {
                        tcs.SetCanceled();
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("canceled");
                }
            }

        }
    }
}
