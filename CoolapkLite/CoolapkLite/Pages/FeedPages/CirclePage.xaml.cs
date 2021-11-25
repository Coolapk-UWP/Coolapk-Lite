using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CirclePage : Page
    {
        public CirclePage() => InitializeComponent();

        private void Pivot_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Pivot.ItemsSource = MenuItem.GetMainItems();
        }
    }

    public class MenuItem
    {
        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem()
                { 
                    Tag = "V9_HOME_TAB_FOLLOW",
                    Header = "V9_HOME_TAB_FOLLOW",
                    Content = new Frame()
                }
            };
            return items;
        }
    }
}
