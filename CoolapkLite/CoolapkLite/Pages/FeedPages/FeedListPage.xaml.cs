﻿using CoolapkLite.Controls;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.ToolsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using CoolapkLite.ViewModels.ToolsPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = CoolapkLite.Controls.TwoPaneView;
using TwoPaneViewMode = CoolapkLite.Controls.TwoPaneViewMode;
using TwoPaneViewPriority = CoolapkLite.Controls.TwoPaneViewPriority;

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
        CollectionPageList
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(FeedListViewModel),
                typeof(FeedListPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public FeedListViewModel Provider
        {
            get => (FeedListViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        #region HeaderMargin

        /// <summary>
        /// Identifies the <see cref="HeaderMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(FeedListPage),
                new PropertyMetadata((double)Application.Current.Resources["PageTitleHeight"]));

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            private set => SetValue(HeaderMarginProperty, value);
        }

        #endregion

        #region HeaderHeight

        /// <summary>
        /// Identifies the <see cref="HeaderHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(FeedListPage),
                new PropertyMetadata(40d));

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            private set => SetValue(HeaderHeightProperty, value);
        }

        #endregion

        public FeedListPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigating += OnFrameNavigating;
            if (e.Parameter is FeedListViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                TwoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
                Provider.DataTemplateSelector = DetailTemplateSelector;
                _ = Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Frame.Navigating -= OnFrameNavigating;
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back
                && TwoPaneView.Mode == TwoPaneViewMode.SinglePane
                && TwoPaneView.PanePriority == TwoPaneViewPriority.Pane1)
            {
                TwoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
                args.Cancel = true;
            }
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (FeedsListView.ItemsSource is EntityItemSource entities)
            {
                await entities.Refresh(true).ConfigureAwait(false);
            }
        }

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).Height = e.NewSize.Width / 2;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            FlipView view = sender as FlipView;
            view.Height = view.ActualWidth / 3;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case "LikeButton" when element.Tag is ICanLike like:
                    _ = like.ChangeLikeAsync();
                    break;
                case "FansButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, false, Provider.Title, Dispatcher));
                    break;
                case "ReportButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag?.ToString(), Dispatcher));
                    break;
                case "RemarkButton":
                    _ = new RemarkDialog(element.Tag?.ToString()).ShowAsync();
                    break;
                case "FollowButton" when element.Tag is ICanFollow follow:
                    _ = follow.ChangeFollowAsync();
                    break;
                case "PinTileButton" when element.Tag is Entity entity:
                    _ = Provider.PinSecondaryTileAsync(entity);
                    break;
                case "MessageButton" when SettingsHelper.Get<string>(SettingsHelper.Uid) is string uid && !string.IsNullOrEmpty(uid):
                    _ = this.NavigateAsync(typeof(ChatPage), new ChatViewModel(string.Join("_", uid, element.Tag), $"{Provider.Title}的私信", Dispatcher));
                    break;
                case "FollowsButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, true, Provider.Title, Dispatcher));
                    break;
                case "AnalyzeButton":
                    _ = this.NavigateAsync(typeof(FansAnalyzePage), new FansAnalyzeViewModel(element.Tag?.ToString(), Dispatcher));
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element && element.Tag is ImageModel image)) { return; }
            switch (element.Name)
            {
                case "CopyButton":
                    image.CopyPic();
                    break;
                case "SaveButton":
                    image.SavePic();
                    break;
                case "ShareButton":
                    image.SharePic();
                    break;
                case nameof(RefreshButton):
                    _ = image.Refresh(element.Dispatcher);
                    break;
                case "ShowImageButton":
                    _ = this.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type &= (ImageType)0xFE;
                    break;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement ListView = sender as FrameworkElement;

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
            DragOperationDeferral deferral = args.GetDeferral();
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await ((sender as FrameworkElement)?.Tag as ImageModel)?.GetImageDataPackageAsync(args.Data, "拖拽图片");
            deferral.Complete();
        }

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if ((element.DataContext as ICanCopy)?.IsCopyEnabled == true) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.Tag is ImageModel image ? this.ShowImageAsync(image) : this.OpenLinkAsync(element.Tag?.ToString());
        }

        public void FrameworkElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Space:
                    FrameworkElement_Tapped(sender, null);
                    e.Handled = true;
                    break;
            }
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                _ = Provider.SearchQuerySubmittedAsync(sender.Text);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TwoPaneView.Mode == TwoPaneViewMode.SinglePane)
            {
                TwoPaneView.PanePriority = TwoPaneView.PanePriority == Controls.TwoPaneViewPriority.Pane1 ? Controls.TwoPaneViewPriority.Pane2 : Controls.TwoPaneViewPriority.Pane1;
            }
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        private void SearchRefreshButton_Click(object sender, RoutedEventArgs e) => _ = Provider.SearchRefresh(true);

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = this.GetXAMLRootSize().Width > 640 ? 0 : 48;

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

            if (SearchBox.Parent != null)
            {
                (SearchBox.Parent as Panel).Children.Remove(SearchBox);
            }
            else
            {
                SearchBoxGrid.Children.Remove(SearchBox);
                Pane1Grid.Children.Remove(SearchBox);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                HeaderHeight = 40;
                HeaderMargin = PageTitleHeight;
                DetailListView.HeaderHeight = 0;
                TitleBar.IsRefreshButtonVisible = true;
                RefreshButton.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
                SearchBoxGrid.Visibility = Visibility.Visible;
                // Add the details content to Pane1.
                RightGrid.Children.Add(TitleBar);
                Pane2Grid.Children.Add(DetailControl);
                SearchBoxGrid.Children.Add(SearchBox);

                Thickness StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
                ItemsStackPanel StackPanel = DetailListView.FindDescendant<ItemsStackPanel>();
                if (StackPanel != null)
                {
                    StackPanel.Margin = StackPanelMargin;
                    StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
            // Dual pane.
            else
            {
                HeaderMargin = 0d;
                HeaderHeight = PageTitleHeight;
                DetailListView.HeaderHeight = 40;
                TitleBar.IsRefreshButtonVisible = false;
                RefreshButton.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Collapsed;
                SearchBoxGrid.Visibility = Visibility.Collapsed;
                // Put details content in Pane2.
                LeftGrid.Children.Add(TitleBar);
                Pane1Grid.Children.Add(DetailControl);
                DetailFlyoutHeader.Children.Add(SearchBox);

                Thickness StackPanelMargin = new Thickness();
                ItemsStackPanel StackPanel = DetailListView.FindDescendant<ItemsStackPanel>();
                if (StackPanel != null)
                {
                    StackPanel.Margin = StackPanelMargin;
                    StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e) => TwoPaneView_ModeChanged(sender as TwoPaneView, null);

        #endregion 界面模式切换
    }

    public class DetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate AppDetail { get; set; }
        public DataTemplate DyhDetail { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        public DataTemplate ProductDetail { get; set; }
        public DataTemplate CollectionDetail { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case AppDetail _:
                    return AppDetail;
                case DyhDetail _:
                    return DyhDetail;
                case UserDetail _:
                    return UserDetail;
                case TopicDetail _:
                    return TopicDetail;
                case ProductDetail _:
                    return ProductDetail;
                case CollectionDetail _:
                    return CollectionDetail;
                default:
                    return Others;
            }
        }
    }
}
