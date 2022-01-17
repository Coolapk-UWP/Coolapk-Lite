using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkUWP.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl
    {
        public static readonly DependencyProperty FeedDetailProperty = DependencyProperty.Register(
           nameof(FeedDetail),
           typeof(FeedDetailModel),
           typeof(FeedShellDetailControl),
           null);

        public FeedDetailModel FeedDetail
        {
            get => (FeedDetailModel)GetValue(FeedDetailProperty);
            set => SetValue(FeedDetailProperty, value);
        }

        public FeedShellDetailControl() => InitializeComponent();

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
                    //await DataHelper.MakeLikeAsync(
                    //    element.Tag as ICanChangeLikModel,
                    //    element.Dispatcher,
                    //    (SymbolIcon)element.FindName("like1"),
                    //    (SymbolIcon)element.FindName("like2"));
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

        private void Flyout_Opened(object sender, object _)
        {
            Flyout flyout = (Flyout)sender;
            if (flyout.Content == null)
            {
                flyout.Content = new ShowQRCodeControl
                {
                    QRCodeText = (string)flyout.Target.Tag
                };
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
