using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
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
            if (e.Parameter is FavoriteViewModel ViewModel)
            {
                Provider = ViewModel;
                Provider.LoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.LoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(true);
            }
            else
            {
                TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Favorite");
            }
            DataContext = Provider;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.LoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        private async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);
    }
}
