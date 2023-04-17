using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
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
            if (e.Parameter is AdaptiveViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.LoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.LoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.LoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Page page = this.FindAscendant<Page>();
            Provider.IsShowTitle = page is MainPage;

            Thickness StackPanelMargin;
            Thickness ScrollViewerMargin;
            Thickness ScrollViewerPadding;

            if (Provider.IsShowTitle)
            {
                StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
                ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
                ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];
            }
            else
            {
                StackPanelMargin = ScrollViewerMargin = ScrollViewerPadding = new Thickness(0);
            }

            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();

            if (StackPanel != null)
            {
                StackPanel.Margin = StackPanelMargin;
                StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = ScrollViewerMargin;
                ScrollViewer.Padding = ScrollViewerPadding;
            }
        }
    }
}
