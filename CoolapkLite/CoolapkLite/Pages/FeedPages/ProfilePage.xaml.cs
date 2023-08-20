using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
    public sealed partial class ProfilePage : Page, INotifyPropertyChanged
    {
        private ProfileViewModel Provider;
        private DateTime dateTime = default;

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

        public ProfilePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ProfileViewModel ViewModel && Provider == null)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.LoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.LoadMoreCompleted += UIHelper.HideProgressBar;
            }

            if (!Provider.IsLogin || dateTime == default || DateTime.UtcNow - dateTime == TimeSpan.FromMinutes(1))
            {
                await Refresh(true);
                dateTime = DateTime.UtcNow;
            }
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (ListView.ItemsSource is EntityItemSource entities)
            {
                _ = entities.Refresh(true);
            }
            dateTime = DateTime.UtcNow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "FeedsButton":
                    _ = this.NavigateAsync(typeof(FeedListPage), FeedListViewModel.GetProvider(FeedListType.UserPageList, Provider.ProfileDetail.EntityID.ToString()));
                    break;
                case "FollowsButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
                    break;
                case "FansButton":
                    _ = this.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
                    break;
                case "LoginButton":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                case "CreateFeedButton":
                    new CreateFeedControl
                    {
                        FeedType = CreateFeedType.Feed,
                        PopupTransitions = new TransitionCollection
                        {
                            new EdgeUIThemeTransition
                            {
                                Edge = EdgeTransitionLocation.Bottom
                            }
                        }
                    }.Show(this);
                    break;
                case "NotificationButton":
                    _ = this.NavigateAsync(typeof(NotificationsPage));
                    break;
                default:
                    break;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Tag.ToString())
            {
                case "FeedsButton":
                    _ = this.NavigateAsync(typeof(FeedListPage), FeedListViewModel.GetProvider(FeedListType.UserPageList, Provider.ProfileDetail.EntityID.ToString()));
                    break;
                default:
                    _ = this.OpenLinkAsync(element.Tag.ToString());
                    break;
            }
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

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => _ = Provider.Refresh(true);

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
