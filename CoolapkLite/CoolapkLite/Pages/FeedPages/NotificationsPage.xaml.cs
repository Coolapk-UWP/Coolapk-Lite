using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Pages;
using CoolapkLite.ViewModels.FeedPages;
using CoolapkLite.ViewModels.Providers;
using CoolapkLite.ViewModels.SettingsPages;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NotificationsPage : Page
    {
        private static int PivotIndex = 0;

        private bool isLoaded;
        private Func<bool, Task> RefreshTask;

        #region NotificationsModel

        /// <summary>
        /// Identifies the <see cref="NotificationsModel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NotificationsModelProperty =
            DependencyProperty.Register(
                nameof(NotificationsModel),
                typeof(NotificationsModel),
                typeof(NotificationsPage),
                null);

        public NotificationsModel NotificationsModel
        {
            get => (NotificationsModel)GetValue(NotificationsModelProperty);
            private set => SetValue(NotificationsModelProperty, value);
        }

        #endregion

        public NotificationsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NotificationsModel = NotificationsModel ?? (NotificationsModel.Caches.TryGetValue(Dispatcher, out NotificationsModel model) ? model : new NotificationsModel());
            _ = NotificationsModel.Update();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                Pivot.SelectedIndex = PivotIndex;
                isLoaded = true;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                switch ((Pivot.SelectedItem as PivotItem).Tag.ToString())
                {
                    case "CommentMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "list",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new SimpleNotificationModel(o) },
                                    "id")));
                        break;
                    case "AtMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "atMeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new FeedModel(o) },
                                    "id")));
                        break;
                    case "AtCommentMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "atCommentMeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new AtCommentMeNotificationModel(o) },
                                    "id")));
                        break;
                    case "FeedLike":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "feedLikeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new LikeNotificationModel(o) },
                                    "id")));
                        break;
                    case "Follow":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "contactsFollowList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new SimpleNotificationModel(o) },
                                    "id")));
                        break;
                    case "Message":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetChats,
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new MessageNotificationModel(o) },
                                    "id")));
                        break;
                    default:
                        break;
                }
                RefreshTask = (reset) => (Frame.Content as AdaptivePage).Refresh(reset);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame frame && frame.Content is AdaptivePage AdaptivePage)
            {
                RefreshTask = (reset) => AdaptivePage.Refresh(reset);
            }
        }

        private async Task Refresh(bool reset = false)
        {
            await NotificationsModel.Update();
            await RefreshTask(reset);
        }

        private void Pivot_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = this.GetXAMLRootSize().Width > 640 ? 0 : 48;

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
