using CoolapkLite.Helpers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        private ViewModels.HistoryPage.ViewModel Provider;

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public HistoryPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("History");
            if (e.Parameter is ViewModels.HistoryPage.ViewModel ViewModel)
            {
                Provider = ViewModel;
                ListView.ItemsSource = Provider;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(-2);
                if (!string.IsNullOrEmpty(Provider.Title))
                {
                    TitleBar.Title = Provider.Title;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.OnLoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.OnLoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);

        private async void ListView_RefreshRequested(object sender, System.EventArgs e) => await Refresh(-2);
    }
}
