using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using System;
using System.Collections.ObjectModel;
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
    public sealed partial class CirclePage : Page
    {
        private static int PivotIndex = 0;

        private bool isLoaded;
        private Func<bool, Task> Refresh;

        public CirclePage() => InitializeComponent();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                Pivot.ItemsSource = GetMainItems();
                Pivot.SelectedIndex = PivotIndex;
                isLoaded = true;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(MenuItem.Tag.ToString().Contains("V") ? $"/page?url={MenuItem.Tag}" : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}", Dispatcher));
                Refresh = reset => (Frame.Content as AdaptivePage)?.Refresh(reset);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame frame && frame.Content is AdaptivePage AdaptivePage)
            {
                Refresh = reset => AdaptivePage.Refresh(reset);
            }
        }

        private void Pivot_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = this.GetXAMLRootSize().Width > 640 ? 0 : 48;

        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("CirclePage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
