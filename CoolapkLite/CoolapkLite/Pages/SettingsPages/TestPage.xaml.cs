using CoolapkLite.Core.Helpers;
using CoolapkLite.Helpers;
using CoolapkLite.Pages.ToolPages;
using CoolapkLite.ViewModels.ToolPages;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private Thickness ScrollViewerPadding => UIHelper.ScrollViewerPadding;

        internal bool IsExtendsTitleBar
        {
            get => CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            set
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = value;
                RaisePropertyChangedEvent();
            }
        }

        public TestPage()
        {
            InitializeComponent();
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Test");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OutPIP":
                    _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                    break;
                case "EnterPIP":
                    _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    break;
                case "OpenEdge":
                    _ = Launcher.LaunchUriAsync(new Uri(WebUrl.Text));
                    break;
                case "ShowError":
                    //throw new WFunMessageException(NotifyMessage.Text);
                    break;
                case "GetContent":
                    Uri uri = WebUrl.Text.ValidateAndGetUri();
                    (bool isSucceed, string result) = uri == null ? (true, "这不是一个链接") : await Utils.GetStringAsync(uri, false);
                    if (!isSucceed)
                    {
                        result = "网络错误";
                    }
                    ContentDialog GetJsonDialog = new ContentDialog
                    {
                        Title = WebUrl.Text,
                        Content = new ScrollViewer
                        {
                            Content = new TextBlock
                            {
                                IsTextSelectionEnabled = true,
                                Text = result.ConvertJsonString()
                            },
                            VerticalScrollMode = ScrollMode.Enabled,
                            HorizontalScrollMode = ScrollMode.Enabled,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        },
                        CloseButtonText = "好的",
                        DefaultButton = ContentDialogButton.Close
                    };
                    _ = await GetJsonDialog.ShowAsync();
                    break;
                case "ShowMessage":
                    UIHelper.ShowMessage(NotifyMessage.Text);
                    break;
                case "OpenBrowser":
                    _ = Frame.Navigate(typeof(BrowserPage), new object[] { false, WebUrl.Text });
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
                case "GoToFansAnalyzePage":
                    _ = Frame.Navigate(typeof(FansAnalyzePage), new FansAnalyzeViewModel("536381"));
                    break;
                default:
                    break;
            }
        }
    }
}
