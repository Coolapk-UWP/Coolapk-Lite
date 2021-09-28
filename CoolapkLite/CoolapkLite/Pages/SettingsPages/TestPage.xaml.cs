using CoolapkLite.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public TestPage()
        {
            InitializeComponent();
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Test");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OpenEdge":
                    _ = Launcher.LaunchUriAsync(new Uri(WebUrl.Text));
                    break;
                case "ShowError":
                    //throw new WFunMessageException(NotifyMessage.Text);
                case "ShowMessage":
                    UIHelper.ShowMessage(NotifyMessage.Text);
                    break;
                case "OpenBrowser":
                    //_ = Frame.Navigate(typeof(BrowserPage), new object[] { WebUrl.Text });
                    break;
                case "ShowAsyncError":
                    //Thread thread = new Thread(() => throw new WFunMessageException(NotifyMessage.Text));
                    //thread.Start();
                    break;
                case "ShowProgressBar":
                    UIHelper.ShowProgressBar();
                    break;
                case "HideProgressBar":
                    UIHelper.HideProgressBar();
                    break;
                case "ErrorProgressBar":
                    UIHelper.ErrorProgressBar();
                    break;
                case "PausedProgressBar":
                    UIHelper.PausedProgressBar();
                    break;
                case "PrograssRingState":
                    if (UIHelper.IsShowingProgressBar)
                    {
                        UIHelper.HideProgressBar();
                    }
                    else
                    {
                        UIHelper.ShowProgressBar();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
