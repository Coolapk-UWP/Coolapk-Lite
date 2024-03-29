﻿using CoolapkLite.BackgroundTasks;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class FeedsTemplates : ResourceDictionary
    {
        public FeedsTemplates() => InitializeComponent();

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if ((element.DataContext as ICanCopy)?.IsCopyEnabled == true) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag?.ToString());
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case "NewWindow" when element.Tag is FeedReplyModel replyModel:
                    _ = element.Dispatcher.NavigateOutsideAsync(typeof(AdaptivePage), dispatcher => AdaptiveViewModel.GetReplyListProvider(replyModel.ID.ToString(), $"回复({replyModel.ReplyNum})", dispatcher));
                    break;
                case "SeeAllButton" when element.Tag is FeedReplyModel replyModel:
                    _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetReplyListProvider(replyModel.ID.ToString(), $"回复({replyModel.ReplyNum})", element.Dispatcher));
                    break;
                default:
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag?.ToString());
                    break;
            }
        }

        private void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            if (!(sender is FrameworkElement element)) { return; }
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
                                new PopupThemeTransition()
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
                                new PopupThemeTransition()
                            }
                        }.Show(element);
                    }
                    break;

                case "PinTileButton":
                    DisabledCopy();
                    _ = PinSecondaryTileAsync(element.Tag as FeedModelBase);
                    break;

                case "LikeButton":
                    DisabledCopy();
                    _ = (element.Tag as ICanLike).ChangeLikeAsync();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    _ = element.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag?.ToString(), element.Dispatcher));
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "NewWindow":
                    DisabledCopy();
                    _ = element.Dispatcher.OpenLinkOutsideAsync(element.Tag?.ToString());
                    break;

                default:
                    DisabledCopy();
                    _ = element.OpenLinkAsync((sender as FrameworkElement).Tag?.ToString());
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            _ = sender.ShowProgressBarAsync();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true).ConfigureAwait(false);
            _ = sender.HideProgressBarAsync();
            if (!isSucceed) { return; }

            JObject token = (JObject)result;

            if (token.TryGetValue("id", out JToken id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, id.ToString(), sender.Dispatcher);

                if (provider != null)
                {
                    _ = sender.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }

        private async Task<bool> PinSecondaryTileAsync(FeedModelBase feed)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = feed.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTileAsync(tileId, $"{feed.UserInfo.UserName}的{feed.Info}", feed.Url).ConfigureAwait(false);
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

            _ = Dispatcher.ShowMessageAsync(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag?.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement UserControl = sender as FrameworkElement;
            FrameworkElement StackPanel = UserControl.FindChild("BtnsPanel");
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel?.SetValue(Grid.RowProperty, width > 600 ? 1 : 20);
        }
    }
}
