using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Helpers.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        private HashSet<ScrollViewer> ScrollViewers;
        private ScrollProgressProvider Provider;
        private SpinLock SpinLock = new SpinLock();
        private CancellationTokenSource Token;
        private NewDS ItemsSource = new NewDS();

        public BlankPage()
        {
            InitializeComponent();
            ScrollViewers = new HashSet<ScrollViewer>();
            Provider = new ScrollProgressProvider();
            Provider.ProgressChanged += Provider_ProgressChanged;
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem PivotItem = Pivot.ContainerFromItem(Pivot.SelectedItem) as PivotItem;

            if (Token != null) Token.Cancel();
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
            ProgressValue.Text = $"{args}";
            Head.Translation = PivotHeader.Translation = new Vector3(0f, (float)(-Provider.Threshold * Provider.Progress), 0f);
            bool lockTaken = false;
            try
            {
                SpinLock.Enter(ref lockTaken);
                foreach (ScrollViewer sv in ScrollViewers)
                {
                    sv.ChangeView(null, Provider.Progress * Provider.Threshold, null, true);
                }
            }
            finally
            {
                if (lockTaken)
                    SpinLock.Exit();
            }
        }

        private void Pivot_PivotItemLoaded(Pivot sender, PivotItemEventArgs args)
        {
            ScrollViewer ScrollViewer = (args.Item.ContentTemplateRoot as FrameworkElement).FindDescendant<ScrollViewer>();
            FrameworkElement FrameworkElement = ScrollViewer.FindDescendant<ScrollContentPresenter>().FindChild<FrameworkElement>();
            if (FrameworkElement != null)
            {
                FrameworkElement.Margin = new Thickness(0, Head.ActualHeight + PivotHeader.ActualHeight, 0, 0);
            }
            if (ScrollViewer != Provider.ScrollViewer)
            {
                ScrollViewer.ChangeView(null, Provider.Progress * Provider.Threshold, null, true);

                var lockTaken = false;
                try
                {
                    SpinLock.Enter(ref lockTaken);
                    ScrollViewers.Add(ScrollViewer);
                }
                finally
                {
                    if (lockTaken)
                        SpinLock.Exit();
                }
            }
        }

        private void Pivot_PivotItemUnloading(Pivot sender, PivotItemEventArgs args)
        {
            var sv = (args.Item.ContentTemplateRoot as FrameworkElement).FindDescendant<ScrollViewer>();
            if (sv != null)
            {
                var lockTaken = false;
                try
                {
                    SpinLock.Enter(ref lockTaken);
                    ScrollViewers.Remove(sv);
                }
                finally
                {
                    if (lockTaken)
                        SpinLock.Exit();
                }
            }
        }

        private void PivotHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.Threshold = Head.ActualHeight;
        }

        private void Head_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PivotHeader.Margin = new Thickness(0, e.NewSize.Height, 0, 0);
            bool lockTaken = false;
            try
            {
                SpinLock.Enter(ref lockTaken);
                foreach (ScrollViewer ScrollViewer in ScrollViewers)
                {
                    FrameworkElement FrameworkElement = ScrollViewer.FindDescendant<ScrollContentPresenter>().FindChild<FrameworkElement>();
                    if (FrameworkElement != null)
                    {
                        FrameworkElement.Margin = new Thickness(0, Head.ActualHeight + PivotHeader.ActualHeight, 0, 0);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                    SpinLock.Exit();
            }
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

    internal class NewDS : DataSourceBase<string>
    {
        private int _loadnum = 0;
        protected async override Task<IList<string>> LoadItemsAsync(uint count)
        {
            List<string> items = new();
            await Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    items.Add((i + _loadnum).ToString());
                    _loadnum += items.Count;
                }
            });
            return items;
        }

        protected override void AddItems(IList<string> items)
        {
            if (items != null)
            {
                foreach (string item in items)
                {
                    Add(item);
                }
            }
        }
    }
}
