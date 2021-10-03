using CoolapkLite.Helpers;
using CoolapkLite.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        private FeedListPageViewModelBase Provider;
        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListPageViewModelBase ViewModel)
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
    }
}
