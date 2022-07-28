using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkUWP.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class Feeds : ResourceDictionary
    {
        public Feeds() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement s = sender as FrameworkElement;
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            if ((s.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            UIHelper.OpenLinkAsync(s.Tag as string);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                default:
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs _)
        {
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "MakeReplyButton":
                    DisabledCopy();
                    //ListViewItem item = element.FindAscendant<ListViewItem>();
                    //MakeFeedControl ctrl = item.FindName("makeFeed") as MakeFeedControl;
                    //ctrl.Visibility = ctrl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case "LikeButton":
                    DisabledCopy();
                    await RequestHelper.ChangeLikeAsync(element.Tag as ICanChangeLikeModel, element.Dispatcher);
                    break;

                case "ReportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed&id={element.Tag}" });
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "DeviceButton":
                    DisabledCopy();
                    //FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.DevicePageList, (sender as FrameworkElement).Tag as string);
                    //if (f != null)
                    //{
                    //    UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    //}
                    break;

                case "ChangeButton":
                    DisabledCopy();
                    //UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl UserControl = sender as UserControl;
            StackPanel StackPanel = UserControl.FindChildByName("BtnsPanel") as StackPanel;
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel.SetValue(Grid.RowProperty, width > 600 ? 1 : 5);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
