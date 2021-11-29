using CoolapkLite.Helpers;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Pages;
using CoolapkLite.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    internal enum FeedListType
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
    public sealed partial class FeedListPage : Page
    {
        private FeedListViewModel Provider;
        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListViewModel ViewModel)
            {
                Provider = ViewModel;
                ListView.ItemsSource = Provider;
                Provider.TitleUpdate += Provider_TitleUpdate;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(-2);
                if (!string.IsNullOrEmpty(Provider.Title))
                {
                    TitleBar.Title = Provider.Title;
                }
            }
        }

        private void Provider_TitleUpdate(object sender, EventArgs e) => TitleBar.Title = Provider.Title;

        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);

        private async void ListView_RefreshRequested(object sender, System.EventArgs e) => await Refresh(-2);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();
            if (StackPanel != null)
            {
                StackPanel.Margin = UIHelper.StackPanelMargin;
            }
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = UIHelper.ScrollViewerMargin;
                ScrollViewer.Padding = UIHelper.ScrollViewerPadding;
            }
        }
    }

    internal class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate UserDetail { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item.GetType().Name)
            {
                case "FeedModel": return Feed;
                case "UserDetail": return UserDetail;
                default: return Others;
            }
        }
    }
}
