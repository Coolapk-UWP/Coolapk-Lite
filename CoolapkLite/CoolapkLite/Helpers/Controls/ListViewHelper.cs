using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Message;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Helpers
{
    public static class ListViewHelper
    {
        #region Padding

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.RegisterAttached(
                "Padding",
                typeof(Thickness),
                typeof(ListViewHelper),
                new PropertyMetadata(null, OnPaddingChanged));

        public static Thickness GetPadding(ListViewBase element)
        {
            return (Thickness)element.GetValue(PaddingProperty);
        }

        public static void SetPadding(ListViewBase element, Thickness value)
        {
            element.SetValue(PaddingProperty, value);
        }

        private static async void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListViewBase element)) { return; }
            await element.ResumeOnLoadedAsync(() => element.FindDescendant<ScrollViewer>() != null);
            UpdatePadding(element, (Thickness)e.NewValue);
            element.RegisterPropertyChangedCallback(ItemsControl.ItemsPanelProperty, OnItemsPanelChanged);
        }

        private static async void OnItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (!(sender is ListViewBase element)) { return; }

            ScrollViewer scrollViewer = null;
            await element.ResumeOnLoadedAsync(() => (scrollViewer = element.FindDescendant<ScrollViewer>()) != null);

            if ((scrollViewer != null
                || (scrollViewer = element.FindDescendant<ScrollViewer>()) != null)
                && scrollViewer.FindDescendant<ItemsPresenter>() is ItemsPresenter itemsPresenter)
            {
                using (CancellationTokenSource tokenSource = new CancellationTokenSource(3000))
                {
                    while (await itemsPresenter.Dispatcher.AwaitableRunAsync(() => itemsPresenter.FindDescendant<Panel>() == null))
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                        if (tokenSource.IsCancellationRequested)
                        {
                            await element.Dispatcher.ResumeForegroundAsync();
                            await element.ResumeOnLoadedAsync(() => false);
                            goto end;
                        }
                    }
                }

                await element.Dispatcher.ResumeForegroundAsync();
            end:
                UpdatePadding(element, GetPadding(element));
            }
        }

        public static void UpdatePadding(this ListViewBase element, Thickness padding)
        {
            if (element?.FindDescendant<ScrollViewer>() is ScrollViewer scrollViewer
                && scrollViewer.FindDescendant<ItemsPresenter>()?.FindDescendant<Panel>() is Panel panel)
            {
                Thickness margin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);

                panel.Margin = padding;

                scrollViewer.Margin = padding;
                scrollViewer.Padding = margin;
            }
        }

        #endregion

        #region EnableIncrementalLoading

        public static readonly DependencyProperty EnableIncrementalLoadingProperty =
            DependencyProperty.RegisterAttached(
                "EnableIncrementalLoading",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(false, OnEnableIncrementalLoadingChanged));

        public static bool GetEnableIncrementalLoading(ItemsControl element)
        {
            return (bool)element.GetValue(EnableIncrementalLoadingProperty);
        }

        public static void SetEnableIncrementalLoading(ItemsControl element, bool value)
        {
            element.SetValue(EnableIncrementalLoadingProperty, value);
        }

        private static async void OnEnableIncrementalLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl element && e.NewValue is true)
            {
                ScrollViewer scrollViewer = null;
                await element.ResumeOnLoadedAsync(() => (scrollViewer = element.FindDescendant<ScrollViewer>()) != null);

                if (scrollViewer == null && (scrollViewer = element.FindDescendant<ScrollViewer>()) == null)
                {
                    return;
                }

                scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
                scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

                void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs args)
                {
                    LoadMoreItems(element, scrollViewer);
                }

                void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
                {
                    LoadMoreItems(element, scrollViewer);
                }

                async void LoadMoreItems(ItemsControl listViewBase, ScrollViewer _scrollViewer)
                {
                    const double loadingThreshold = 0.5;
                    if (!(listViewBase.ItemsSource is ISupportIncrementalLoading source)) { return; }
                check:
                    if (listViewBase.Items.Count > 0 && !source.HasMoreItems) { return; }
                    if (GetIsIncrementallyLoading(listViewBase)) { return; }
                    if (((_scrollViewer.ExtentHeight - _scrollViewer.VerticalOffset) / _scrollViewer.ViewportHeight) - 1.0 <= loadingThreshold)
                    {
                        try
                        {
                            SetIsIncrementallyLoading(listViewBase, true);
                            await source.LoadMoreItemsAsync(20);
                            goto check;
                        }
                        catch (Exception ex)
                        {
                            SettingsHelper.LogManager.GetLogger(nameof(ShyHeaderListView)).Error(ex.ExceptionToMessage(), ex);
                        }
                        finally
                        {
                            SetIsIncrementallyLoading(listViewBase, false);
                        }
                    }
                }
            }
        }

        #endregion

        #region IsIncrementallyLoading

        private static readonly DependencyProperty IsIncrementallyLoadingProperty =
            DependencyProperty.RegisterAttached(
                "IsIncrementallyLoading",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(false));

        private static bool GetIsIncrementallyLoading(ItemsControl element)
        {
            return (bool)element.GetValue(IsIncrementallyLoadingProperty);
        }

        private static void SetIsIncrementallyLoading(ItemsControl element, bool value)
        {
            element.SetValue(IsIncrementallyLoadingProperty, value);
        }

        #endregion

        #region IsSelectionEnabled

        public static readonly DependencyProperty IsSelectionEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsSelectionEnabled",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(true, OnIsSelectionEnabledChanged));

        public static bool GetIsSelectionEnabled(ListViewBase element)
        {
            return (bool)element.GetValue(IsSelectionEnabledProperty);
        }

        public static void SetIsSelectionEnabled(ListViewBase element, bool value)
        {
            element.SetValue(IsSelectionEnabledProperty, value);
        }

        private static void OnIsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewBase element)
            {
                if (e.NewValue is false)
                {
                    element.SelectionChanged -= ListViewBase_SelectionChanged;
                    element.SelectionChanged += ListViewBase_SelectionChanged;
                }
                else
                {
                    element.SelectionChanged -= ListViewBase_SelectionChanged;
                }
            }
        }

        private static void ListViewBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListViewBase element)) { return; }
            element.SelectedIndex = -1;
        }

        #endregion

        #region IsItemClickEnabled

        public static readonly DependencyProperty IsItemClickEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsItemClickEnabled",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(false, OnIsItemClickEnabledChanged));

        public static bool GetIsItemClickEnabled(ListViewBase element)
        {
            return (bool)element.GetValue(IsItemClickEnabledProperty);
        }

        public static void SetIsItemClickEnabled(ListViewBase element, bool value)
        {
            element.SetValue(IsItemClickEnabledProperty, value);
        }

        private static void OnIsItemClickEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewBase element)
            {
                bool isEnabled = (bool)e.NewValue;
                element.IsItemClickEnabled = isEnabled;
                if (isEnabled)
                {
                    element.ItemClick -= ListViewBase_ItemClick;
                    element.ItemClick += ListViewBase_ItemClick;
                }
                else
                {
                    element.ItemClick -= ListViewBase_ItemClick;
                }
            }
        }

        private static void ListViewBase_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(sender is DependencyObject element)) { return; }
            switch (e.ClickedItem)
            {
                case SourceFeedModel SourceFeedModel:
                    FeedShellViewModel provider = SourceFeedModel.IsQuestionFeed
                        ? new QuestionViewModel(SourceFeedModel.ID.ToString(), element.Dispatcher)
                        : SourceFeedModel.IsVoteFeed
                            ? new VoteViewModel(SourceFeedModel.ID.ToString(), element.Dispatcher)
                            : (FeedShellViewModel)new FeedDetailViewModel(SourceFeedModel.ID.ToString(), element.Dispatcher);
                    _ = element.NavigateAsync(typeof(FeedShellPage), provider);
                    break;
                case ImageModel ImageModel:
                    _ = element.ShowImageAsync(ImageModel);
                    break;
                case MessageNotificationModel MessageNotification:
                    _ = element.NavigateAsync(typeof(ChatPage), new ChatViewModel(MessageNotification.UKey, $"{MessageNotification.UserName}的私信", element.Dispatcher));
                    break;
                case MessageModel MessageModel:
                    if (MessageModel.MessageCard is IHasUrl _IHasUrl)
                    {
                        _ = element.OpenLinkAsync(_IHasUrl.Url);
                    }
                    else if (MessageModel.MessagePic is ImageModel _ImageModel)
                    {
                        _ = element.ShowImageAsync(_ImageModel);
                    }
                    break;
                case IHasUrl IHasUrl:
                    _ = element.OpenLinkAsync(IHasUrl.Url);
                    break;
                case string String:
                    _ = element.OpenLinkAsync(String);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
