﻿using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private Thickness ScrollViewerPadding => UIHelper.ScrollViewerPadding;

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

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.DataTemplateSelector = DetailTemplateSelector;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (ListView.ItemsSource is EntityItemSourse entities)
            {
                _ = entities.Refresh(true);
            }
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = new Thickness(0, UIHelper.ScrollViewerMargin.Top, 0, Padding.Bottom);
                ScrollViewer.Padding = new Thickness(0, UIHelper.ScrollViewerPadding.Top, 0, -Padding.Bottom);
            }
        }

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
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
                HeaderMargin = UIHelper.PageTitleHeight;
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
                HeaderHeight = UIHelper.PageTitleHeight;
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
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item.GetType().Name)
            {
                case "UserDetail": return UserDetail;
                case "TopicDetail": return TopicDetail;
                default: return Others;
            }
        }
    }
}
