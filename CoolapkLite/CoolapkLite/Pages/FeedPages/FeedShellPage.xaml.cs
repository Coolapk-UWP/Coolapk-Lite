using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            //TwoPaneView.MinWideModeWidth = FeedDetailModel?.IsFeedArticle ?? false ? 876 : 804;
            //TwoPaneView.Pane1Length = new GridLength(FeedDetailModel?.IsFeedArticle ?? false ? 520 : 420);
            //UIHelper.MainPage.SetTitle(FeedDetailModel.IsFeedArticle ? $"{FeedDetailModel.UserName}的图文" : $"{FeedDetailModel.UserName}的动态");
            //_ = ListControl.Refresh(-2);
        }

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            _ = (DetailControl.Parent as Panel).Children.Remove(DetailControl);

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                // Add the details content to Pane1.
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                // Put details content in Pane2.
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
