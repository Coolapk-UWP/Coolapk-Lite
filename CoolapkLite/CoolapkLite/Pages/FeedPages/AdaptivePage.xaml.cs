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
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AdaptivePage : Page
    {
        private AdaptiveViewModel Provider;

        public AdaptivePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AdaptiveViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(-2);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.OnLoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.OnLoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        public void Refresh() => _ = Refresh(-2);

        private async Task Refresh(int p = -1)
        {
            await Provider.Refresh(p);
        }

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(-2);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            if (StackPanel != null) { StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch; }
        }
    }
}
