using CoolapkLite.Controls;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        private bool isLoginPage;
        private string uri;

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private const string loginUri = "https://account.coolapk.com/auth/loginByCoolapk";

        public bool IsLoginPage
        {
            get => isLoginPage;
            set
            {
                isLoginPage = value;
                if (value)
                {
                    TryLoginButton.Visibility = Visibility.Visible;
                    OpenInSystemBrowserButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TryLoginButton.Visibility = Visibility.Collapsed;
                    OpenInSystemBrowserButton.Visibility = Visibility.Visible;
                }
            }
        }

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Browser");
            object[] vs = e.Parameter as object[];
            IsLoginPage = (bool)vs[0];
            if (vs[0] is bool && (bool)vs[0])
            {
                IsLoginPage = (bool)vs[0];
                WebView.Navigate(new Uri(loginUri));
            }
            else if (vs[1] is string && !string.IsNullOrEmpty(vs[1] as string))
            {
                uri = vs[1] as string;
                if (!uri.Contains("://")) { uri = $"https://{uri}"; }
                WebView.Navigate(new Uri(uri));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            WebView.NavigationStarting -= WebView_NavigationStarting;
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
            WebView.NavigationStarting -= WebView_NavigationStarting;
            args.Cancel = true;
            LoadUri(args.Uri);
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/")
            {
                CheckLogin();
            }
            else if (args.Uri.AbsoluteUri == loginUri)
            {
                IsLoginPage = true;
            }
            TitleBar.Title = sender.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private async void CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (await SettingsHelper.Login())
            {
                if (Frame.CanGoBack) { Frame.GoBack(); }
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Navigate(new Uri(loginUri));
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(uri));

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => WebView.Refresh();

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => CheckLogin();

        private async void ManualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginDialog Dialog = new LoginDialog();
            ContentDialogResult result = await Dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && Frame.CanGoBack) { Frame.GoBack(); }
        }
    }
}
