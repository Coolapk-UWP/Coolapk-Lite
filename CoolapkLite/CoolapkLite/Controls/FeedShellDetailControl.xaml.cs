using CoolapkLite.BackgroundTasks;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedShellDetailControl"/> class.
        /// </summary>
        public FeedShellDetailControl() => InitializeComponent();

        #region FeedDetail

        public static readonly DependencyProperty FeedDetailProperty =
            DependencyProperty.Register(
                nameof(FeedDetail),
                typeof(FeedDetailModel),
                typeof(FeedShellListControl),
                null);

        public FeedDetailModel FeedDetail
        {
            get => (FeedDetailModel)GetValue(FeedDetailProperty);
            set => SetValue(FeedDetailProperty, value);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case nameof(PinTileButton) when element.Tag is FeedDetailModel feedDetail:
                    _ = PinSecondaryTileAsync(feedDetail);
                    break;
                case nameof(ReportButton):
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag?.ToString(), Dispatcher));
                    break;
                case nameof(FollowButton) when element.Tag is ICanFollow follow:
                    _ = follow?.ChangeFollowAsync();
                    break;
                default:
                    _ = this.OpenLinkAsync(element.Tag?.ToString());
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
                case "OriginButton":
                    image.Type &= (ImageType)0xFE;
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            _ = this.ShowProgressBarAsync();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true).ConfigureAwait(false);
            _ = this.HideProgressBarAsync();
            if (!isSucceed) { return; }

            JObject token = (JObject)result;

            if (token.TryGetValue("id", out JToken id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, id.ToString(), sender.Dispatcher);

                if (provider != null)
                {
                    _ = this.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }

        private async Task<bool> PinSecondaryTileAsync(FeedDetailModel feed)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = feed.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTileAsync(tileId, feed.Title, feed.Url).ConfigureAwait(false);
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

            _ = this.ShowMessageAsync(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag?.ToString());
            Clipboard.SetContent(dp);
        }

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if ((element.DataContext as ICanCopy)?.IsCopyEnabled == true) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.Tag is ImageModel image ? element.ShowImageAsync(image) : this.OpenLinkAsync(element.Tag?.ToString());
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

        private void UrlButton_Click(object sender, RoutedEventArgs e) => _ = this.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).Height = e.NewSize.Width / 2;
    }
}
