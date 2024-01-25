using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = CoolapkLite.Controls.TwoPaneView;
using TwoPaneViewMode = CoolapkLite.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private DateTime dateTime = default;

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(ProfileViewModel),
                typeof(ProfilePage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public ProfileViewModel Provider
        {
            get => (ProfileViewModel)GetValue(ProviderProperty);
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
                typeof(ProfilePage),
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
                typeof(ProfilePage),
                new PropertyMetadata(double.NaN));

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            private set => SetValue(HeaderHeightProperty, value);
        }

        #endregion

        public ProfilePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Provider == null)
            {
                Provider = new ProfileViewModel(Dispatcher);
            }

            Provider.LoadMoreStarted += OnLoadMoreStarted;
            Provider.LoadMoreCompleted += OnLoadMoreCompleted;

            if (!Provider.IsLogin || dateTime == default || DateTime.UtcNow - dateTime == TimeSpan.FromMinutes(1))
            {
                _ = Refresh(true);
            }

            SettingsHelper.LoginChanged += OnLoginChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= OnLoadMoreStarted;
            Provider.LoadMoreCompleted -= OnLoadMoreCompleted;
            SettingsHelper.LoginChanged -= OnLoginChanged;
        }

        private void OnLoadMoreStarted() => _ = this.ShowProgressBarAsync();

        private void OnLoadMoreCompleted() => _ = this.HideProgressBarAsync();

        private void OnLoginChanged(bool isLogin) => _ = Refresh(true);

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset).ConfigureAwait(false);
            dateTime = DateTime.UtcNow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Tag?.ToString())
            {
                case "FeedsButton":
                    _ = this.NavigateAsync(typeof(FeedListPage), FeedListViewModel.GetProvider(FeedListType.UserPageList, Provider.ProfileDetail.EntityID.ToString(), Dispatcher));
                    break;
                case "FollowsButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我", Dispatcher));
                    break;
                case "FansButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我", Dispatcher));
                    break;
                case "LoginButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri, Dispatcher));
                    break;
                case "CreateFeedButton":
                    new CreateFeedControl
                    {
                        FeedType = CreateFeedType.Feed,
                        PopupTransitions = new TransitionCollection
                        {
                            new PopupThemeTransition()
                        }
                    }.Show(this);
                    break;
                case "NotificationButton":
                    _ = this.NavigateAsync(typeof(NotificationsPage), Dispatcher);
                    break;
                default:
                    break;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Tag?.ToString())
            {
                case "FeedsButton":
                    _ = this.NavigateAsync(typeof(FeedListPage), FeedListViewModel.GetProvider(FeedListType.UserPageList, Provider.ProfileDetail.EntityID.ToString(), Dispatcher));
                    break;
                default:
                    _ = this.OpenLinkAsync(element.Tag?.ToString());
                    break;
            }
            if (e != null) { e.Handled = true; }
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();
            Thickness ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            Thickness ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = new Thickness(0, ScrollViewerMargin.Top, 0, Padding.Bottom);
                ScrollViewer.Padding = new Thickness(0, ScrollViewerPadding.Top, 0, -Padding.Bottom);
            }
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            _ = Provider.Refresh(true);
            if (e != null) { e.Handled = true; }
        }

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

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                HeaderHeight = double.NaN;
                HeaderMargin = PageTitleHeight;
                // Add the details content to Pane1.
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                HeaderMargin = 0d;
                HeaderHeight = PageTitleHeight;
                // Put details content in Pane2.
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
