using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
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
    public sealed partial class FavoritePage : Page
    {
        private FavoriteViewModel Provider;

        public FavoritePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Favorite");
            if (e.Parameter is FavoriteViewModel ViewModel)
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
}
