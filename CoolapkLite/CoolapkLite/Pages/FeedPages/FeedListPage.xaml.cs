using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.ToolsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using CoolapkLite.ViewModels.ToolsPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = CoolapkLite.Controls.TwoPaneView;
using TwoPaneViewMode = CoolapkLite.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    public enum FeedListType
    {
        TagPageList,
        DyhPageList,
        AppPageList,
        UserPageList,
        DevicePageList,
        ProductPageList,
        CollectionPageList,
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page, INotifyPropertyChanged
    {
        private FeedListViewModel Provider;

        private double headerMargin;
        internal double HeaderMargin
        {
            get => headerMargin;
            private set
            {
                headerMargin = value;
                RaisePropertyChangedEvent();
            }
        }

        private double headerHeight;
        internal double HeaderHeight
        {
            get => headerHeight;
            private set
            {
                headerHeight = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.DataTemplateSelector = DetailTemplateSelector;
                await Refresh(true);
            }
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (ListView.ItemsSource is EntityItemSource entities)
            {
                _ = entities.Refresh(true);
            }
        }

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).MaxHeight = e.NewSize.Width / 2;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                if ((sender as FrameworkElement).Parent is FrameworkElement parent)
                { parent.Visibility = Visibility.Collapsed; }
            }
            else
            {
                FlipView view = sender as FlipView;
                view.MaxHeight = view.ActualWidth / 3;
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(20)
                };
                timer.Tick += (o, a) =>
                {
                    if (view.SelectedIndex != -1)
                    {
                        if (view.SelectedIndex + 1 >= view.Items.Count)
                        {
                            while (view.SelectedIndex > 0)
                            {
                                view.SelectedIndex -= 1;
                            }
                        }
                        else
                        {
                            view.SelectedIndex += 1;
                        }
                    }
                };
                view.Unloaded += (_, __) => timer.Stop();
                timer.Start();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "LikeButton":
                    _ = (element.Tag as ICanLike).ChangeLike();
                    break;
                case "FansButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, false, Provider.Title));
                    break;
                case "ReportButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;
                case "FollowButton":
                    _ = (element.Tag as ICanFollow).ChangeFollow();
                    break;
                case "PinTileButton":
                    _ = Provider.PinSecondaryTile(element.Tag as Entity);
                    break;
                case "FollowsButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, true, Provider.Title));
                    break;
                case "AnalyzeButton":
                    _ = this.NavigateAsync(typeof(FansAnalyzePage), new FansAnalyzeViewModel(element.Tag.ToString(), Dispatcher));
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ImageModel image = (sender as FrameworkElement).Tag as ImageModel;
            switch ((sender as FrameworkElement).Name)
            {
                case "CopyButton":
                    Provider.CopyPic(image);
                    break;
                case "SaveButton":
                    Provider.SavePic(image);
                    break;
                case "ShareButton":
                    Provider.SharePic(image);
                    break;
                case "RefreshButton":
                    _ = image.Refresh();
                    break;
                case "ShowImageButton":
                    _ = this.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type = ImageType.OriginImage;
                    break;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            Thickness ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();

            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = new Thickness(0, ScrollViewerMargin.Top, 0, Padding.Bottom);
                ScrollViewer.Padding = new Thickness(0, ScrollViewerPadding.Top, 0, -Padding.Bottom);
            }
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await Provider.GetImageDataPackage(args.Data, (sender as FrameworkElement).Tag as ImageModel, "拖拽图片");
        }

        private void On_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object Tag = (sender as FrameworkElement).Tag;
            if (Tag is ImageModel image)
            {
                _ = this.ShowImageAsync(image);
            }
            else if (Tag is string url)
            {
                _ = this.OpenLinkAsync(url);
            }
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            double PageTitleHeight = (double)Application.Current.Resources["PageTitleHeight"];

            // Remove details content from it's parent panel.
            if (DetailControl.Parent != null)
            {
                (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            }
            else
            {
                Pane1Grid.Children.Remove(DetailControl);
                Pane2Grid.Children.Remove(DetailControl);
            }

            if (TitleBar.Parent != null)
            {
                (TitleBar.Parent as Panel).Children.Remove(TitleBar);
            }
            else
            {
                LeftGrid.Children.Remove(TitleBar);
                RightGrid.Children.Remove(TitleBar);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                HeaderHeight = double.NaN;
                HeaderMargin = PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = true;
                RefreshButton.Visibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(TitleBar);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                HeaderMargin = 0d;
                HeaderHeight = PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = false;
                RefreshButton.Visibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(TitleBar);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }

    internal class DetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate DyhDetail { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        public DataTemplate ProductDetail { get; set; }
        public DataTemplate CollectionDetail { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is Models.Pages.DyhDetail
                ? DyhDetail
                : item is Models.Pages.UserDetail
                    ? UserDetail
                    : item is Models.Pages.TopicDetail
                        ? TopicDetail
                        : item is Models.Pages.ProductDetail
                            ? ProductDetail
                            : item is Models.Pages.CollectionDetail
                                ? CollectionDetail
                                : Others;
        }
    }
}
