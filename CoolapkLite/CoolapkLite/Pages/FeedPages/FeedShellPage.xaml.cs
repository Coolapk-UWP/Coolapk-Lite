using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        public FeedShellPage()
        {
            InitializeComponent();
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
