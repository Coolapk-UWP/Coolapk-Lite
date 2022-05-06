using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = CoolapkLite.Controls.TwoPaneView;
using TwoPaneViewMode = CoolapkLite.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedShellPage : Page
    {
        private FeedShellViewModel Provider;
        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private Thickness ScrollViewerPadding => UIHelper.ScrollViewerPadding;

        public FeedShellPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedShellViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Provider.Refresh(-2);
                if (Provider.FeedDetail != null)
                {
                    SetLayout();
                }
            }
            await System.Threading.Tasks.Task.Delay(30);
        }

        private void SetLayout()
        {
            //ListControl.ReplyDS = new Controls.ReplyDS(FeedDetailModel);
            TwoPaneView.MinWideModeWidth = Provider.FeedDetail?.IsFeedArticle ?? false ? 876 : 804;
            TwoPaneView.Pane1Length = new GridLength(Provider.FeedDetail?.IsFeedArticle ?? false ? 520 : 420);
            //_ = ListControl.Refresh(-2);
        }

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            _ = (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            _ = (BtnsPanel.Parent as Panel).Children.Remove(BtnsPanel);
            _ = (TitleBar.Parent as Panel).Children.Remove(TitleBar);


            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListControl.HeaderHeight = double.NaN;
                ListControl.HeaderMargin = UIHelper.PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = true;
                ListControl.RefreshButtonVisibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(TitleBar);
                RightGrid.Children.Add(BtnsPanel);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListControl.HeaderMargin = 0d;
                ListControl.HeaderHeight = UIHelper.PageTitleHeight;
                TitleBar.IsRefreshButtonVisible = false;
                ListControl.RefreshButtonVisibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(TitleBar);
                LeftGrid.Children.Add(BtnsPanel);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
