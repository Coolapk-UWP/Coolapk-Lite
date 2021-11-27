using CoolapkLite.Helpers;
using CoolapkLite.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CirclePage : Page
    {
        private Thickness PivotTitleMargin => UIHelper.PivotTitleMargin;

        public CirclePage() => InitializeComponent();

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.ItemsSource = MenuItem.GetMainItems();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(MenuItem.Tag.ToString().Contains("V") ? $"/page?url={MenuItem.Tag}" : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}"));
                RelayCommand RefreshButtonCommand = new RelayCommand((Frame.Content as AdaptivePage).Refresh);
                RefreshButton.Command = RefreshButtonCommand;
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                RelayCommand RefreshButtonCommand = new RelayCommand(AdaptivePage.Refresh);
                RefreshButton.Command = RefreshButtonCommand;
            }
        }

        private void Pivot_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = Window.Current.Bounds.Width > 640 ? 0 : 48;
    }

    public class MenuItem
    {
        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("CirclePage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem() { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem() { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem() { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem() { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem() { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }
    }
}
