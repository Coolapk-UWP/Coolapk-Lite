using CoolapkLite.Controls;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.BrowserPages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.BrowserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        private BrowserViewModel Provider;

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigating += OnFrameNavigating;
            if (e.Parameter is BrowserViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                if (Provider.Uri != null)
                {
                    WebView.Navigate(Provider.Uri);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Frame.Navigating -= OnFrameNavigating;
        }

        private void LoadUri(Uri uri)
        {
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString());
                WebView.NavigateWithHttpRequestMessage(httpRequestMessage);
                WebView.NavigationStarting += WebView_NavigationStarting;
            }
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            UIHelper.ShowProgressBar();
            if (args.Uri.Host.Contains("coolapk"))
            {
                WebView.NavigationStarting -= WebView_NavigationStarting;
                args.Cancel = true;
                LoadUri(args.Uri);
            }
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (Provider.IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/")
            {
                await CheckLogin();
            }
            else if (args.Uri.AbsoluteUri == UriHelper.LoginUri)
            {
                Provider.IsLoginPage = true;
            }
            Provider.Title = sender.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back && WebView.CanGoBack)
            {
                WebView.GoBack();
                args.Cancel = true;
            }
        }

        private async Task CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            UIHelper.ShowMessage(loader.GetString("Logging"));
            if (await SettingsHelper.Login())
            {
                if (Frame.CanGoBack)
                {
                    Frame.Navigating -= OnFrameNavigating;
                    Frame.GoBack();
                }
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Navigate(new Uri(UriHelper.LoginUri));
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        private async void ManualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginDialog Dialog = new LoginDialog();
            ContentDialogResult result = await Dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && Frame.CanGoBack)
            {
                Frame.Navigating -= OnFrameNavigating;
                Frame.GoBack();
            }
        }

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(WebView.Source);

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => _ = CheckLogin();

        private void TitleBar_RefreshRequested(TitleBar sender, object args) => WebView.Refresh();
    }
}
