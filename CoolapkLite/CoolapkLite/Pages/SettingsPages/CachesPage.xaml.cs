using CoolapkLite.Controls;
using CoolapkLite.ViewModels.SettingsPages;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CachesPage : Page
    {
        internal CachesViewModel Provider;

        public CachesPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = Provider ?? new CachesViewModel(Dispatcher);
            DataContext = Provider;
            await Refresh(true);
        }

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Delete":
                    Provider?.RemoveImage(element.Tag as StorageFile);
                    break;
                default:
                    break;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness StackPanelMargin;
            Thickness ScrollViewerMargin;
            Thickness ScrollViewerPadding;

            StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
            ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            StaggeredPanel StackPanel = ListView.FindDescendant<StaggeredPanel>();
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
