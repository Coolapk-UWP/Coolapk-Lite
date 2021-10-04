using CoolapkLite.Core.Helpers;
using CoolapkLite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            using (Windows.Web.Http.HttpRequestMessage httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString());
                WebView.NavigateWithHttpRequestMessage(httpRequestMessage);
                WebView.NavigationStarting += WebView_NavigationStarting;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _ = SettingsHelper.CheckLoginInfo();
            base.OnNavigatingFrom(e);
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

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (SettingsHelper.CheckLoginInfo())
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

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
        }

        private void RefreshPage() => WebView.Refresh();

        private void TryLoginButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLogin();
        }
    }
}
