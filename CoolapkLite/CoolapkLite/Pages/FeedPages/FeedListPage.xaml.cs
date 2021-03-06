using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
                DataContext = Provider;
                Provider.TitleUpdate += Provider_TitleUpdate;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(-2);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.TitleUpdate -= Provider_TitleUpdate;
            Provider.OnLoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.OnLoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        private void Provider_TitleUpdate(object sender, EventArgs e) => TitleBar.Title = Provider.Title;

        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(-2);

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
