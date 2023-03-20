using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
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
    public sealed partial class IndexPage : Page
    {
        private IndexViewModel Provider;

        public IndexPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IndexViewModel ViewModel
                && (Provider == null || Provider.Uri != ViewModel.Uri))
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.LoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.LoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(true);
            }
            else
            {
                TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Home");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.LoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        private async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
            Thickness ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            Thickness ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();

            if (StackPanel != null)
            {
                StackPanel.Margin = StackPanelMargin;
            }
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = ScrollViewerMargin;
                ScrollViewer.Padding = ScrollViewerPadding;
            }
        }
    }
}
