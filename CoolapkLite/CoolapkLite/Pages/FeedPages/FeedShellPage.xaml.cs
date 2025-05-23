﻿using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserActivities;
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
    public sealed partial class FeedShellPage : Page
    {
        private static UserActivitySession _currentActivity;

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(FeedShellViewModel),
                typeof(FeedShellPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public FeedShellViewModel Provider
        {
            get => (FeedShellViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public FeedShellPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedShellViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                _ = Provider.Refresh(true)
                    .ContinueWith(async x =>
                    {
                        await Dispatcher.ResumeForegroundAsync();
                        if (Provider.FeedDetail != null)
                        {
                            SetLayout();
                            _ = GenerateActivityAsync();
                        }
                    });
            }
        }

        private void SetLayout()
        {
            if (Provider.FeedDetail?.IsFeedArticle == true)
            {
                TwoPaneView.MinWideModeWidth = 876;
                TwoPaneView.Pane1Length = new GridLength(520);
            }
            else
            {
                TwoPaneView.MinWideModeWidth = 804;
                TwoPaneView.Pane1Length = new GridLength(420);
            }
        }

        private async Task GenerateActivityAsync()
        {
            if (!ApiInfoHelper.IsUserActivityChannelSupported)
            { return; }

            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync(Provider.FeedDetail.Url.GetMD5());

            // Populate required properties
            if (!string.IsNullOrWhiteSpace(Provider.Title))
            {
                userActivity.VisualElements.DisplayText = Provider.Title;
                userActivity.VisualElements.AttributionDisplayText = Provider.Title;
            }
            userActivity.VisualElements.Description = Provider.FeedDetail.Message.HtmlToString();
            userActivity.ActivationUri = new Uri($"coolapk://{(Provider.FeedDetail.Url[0] == '/' ? Provider.FeedDetail.Url.Substring(1) : Provider.FeedDetail.Url)}");

            //Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();
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
                case nameof(ReplyButton):
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
                        }.Show(this);
                    }
                    break;

                case nameof(LikeButton):
                    DisabledCopy();
                    _ = (element.Tag as ICanLike).ChangeLikeAsync();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString(), Dispatcher));
                    break;

                case nameof(ShareButton):
                    DisabledCopy();
                    break;

                case nameof(StarButton):
                    DisabledCopy();
                    break;

                default:
                    DisabledCopy();
                    _ = this.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private void TapArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (e != null) { e.Handled = ListControl.ShyHeaderPivotListView.BringToLists(); }
        }

        private void TitleBar_DoubleTappedRequested(TitleBar sender, DoubleTappedRoutedEventArgs args)
        {
            if (args != null)
            {
                args.Handled = TwoPaneView.Mode == TwoPaneViewMode.SinglePane
                    ? ListControl.ShyHeaderPivotListView.BackToTop()
                    : DetailScrollViewer.ChangeView(0, 0, null);
            }
        }

        private void TitleBar_RefreshRequested(TitleBar sender, object args) => _ = Provider.Refresh(true);

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            if ((DetailControl.MediaPlayerElementEx.MediaElement is MediaElement mediaElement
                && mediaElement.IsFullWindow)
                    || (MediaPlayerElementEx.IsMediaPlayerElementSupported
                    && DetailControl.MediaPlayerElementEx.MediaElement is MediaPlayerElement mediaPlayerElement
                    && mediaPlayerElement.IsFullWindow))
            { return; }

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

            if (BtnsPanel.Parent != null)
            {
                (BtnsPanel.Parent as Panel).Children.Remove(BtnsPanel);
            }
            else
            {
                LeftGrid.Children.Remove(BtnsPanel);
                RightGrid.Children.Remove(BtnsPanel);
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
                ListControl.HeaderHeight = 40;
                ListControl.HeaderMargin = PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = true;
                ListControl.RefreshButtonVisibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(TitleBar);
                RightGrid.Children.Add(BtnsPanel);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListControl.HeaderMargin = 0d;
                ListControl.HeaderHeight = PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = false;
                ListControl.RefreshButtonVisibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(TitleBar);
                LeftGrid.Children.Add(BtnsPanel);
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
