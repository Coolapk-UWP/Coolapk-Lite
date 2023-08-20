using CoolapkLite.BackgroundTasks;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using TileSize = Windows.UI.StartScreen.TileSize;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class FeedsTemplates : ResourceDictionary
    {
        public FeedsTemplates() => InitializeComponent();

        private void OnRootTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag.ToString());
        }

        private void OnTopTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "SeeAllButton":
                    _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetReplyListProvider(((FeedReplyModel)element.Tag).ID.ToString(), (FeedReplyModel)element.Tag));
                    break;
                default:
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "ReplyButton":
                    DisabledCopy();
                    if (element.Tag is FeedModelBase feed)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = feed.ID,
                            FeedType = CreateFeedType.Reply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show(element);
                    }
                    else if (element.Tag is FeedReplyModel reply)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = reply.ID,
                            FeedType = CreateFeedType.ReplyReply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show(element);
                    }
                    break;

                case "PinTileButton":
                    DisabledCopy();
                    _ = PinSecondaryTile(element.Tag as FeedModelBase);
                    break;

                case "LikeButton":
                    DisabledCopy();
                    await (element.Tag as ICanLike).ChangeLike();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    _ = element.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "ChangeButton":
                    DisabledCopy();
                    //UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            UIHelper.ShowProgressBar();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true);
            UIHelper.HideProgressBar();
            if (!isSucceed) { return; }

            JObject token = (JObject)result;

            if (token.TryGetValue("id", out JToken id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, id.ToString());

                if (provider != null)
                {
                    _ = sender.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }

        private async Task<bool> PinSecondaryTile(FeedModelBase feed)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = feed.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTile(tileId, $"{feed.UserInfo.UserName}的{feed.Info}", feed.Url);
            if (isPinned)
            {
                try
                {
                    LiveTileTask.UpdateTile(tileId, LiveTileTask.GetFeedTile(feed));
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(FeedShellDetailControl)).Error(ex.ExceptionToMessage(), ex);
                }

                return isPinned;
            }

            UIHelper.ShowMessage(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl UserControl = sender as UserControl;
            FrameworkElement StackPanel = UserControl.FindChild("BtnsPanel");
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel?.SetValue(Grid.RowProperty, width > 600 ? 1 : 20);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
