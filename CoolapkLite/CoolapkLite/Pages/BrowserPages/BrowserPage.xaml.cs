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
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(BrowserViewModel),
                typeof(BrowserPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public BrowserViewModel Provider
        {
            get => (BrowserViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigating += OnFrameNavigating;
            if (e.Parameter is BrowserViewModel ViewModel)
            {
                Provider = ViewModel;
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
            _ = this.ShowProgressBarAsync();
            if (Provider.IsChangeBrowserUA || args.Uri.Host.Contains("coolapk", StringComparison.OrdinalIgnoreCase))
            {
                WebView.NavigationStarting -= WebView_NavigationStarting;
                args.Cancel = true;
                LoadUri(args.Uri);
            }
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try
            {
                if (Provider.IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/")
                {
                    await CheckLoginAsync();
                }
                else if (args.Uri.AbsoluteUri == UriHelper.LoginUri)
                {
                    Provider.IsLoginPage = true;
                }
                Provider.Title = sender.DocumentTitle;
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back && WebView.CanGoBack)
            {
                WebView.GoBack();
                args.Cancel = true;
            }
        }

        private async Task CheckLoginAsync()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");
            _ = this.ShowMessageAsync(loader.GetString("Logging"));
            if (await SettingsHelper.LoginAsync())
            {
                if (Frame.CanGoBack)
                {
                    Frame.Navigating -= OnFrameNavigating;
                    Frame.GoBack();
                }
                _ = this.ShowMessageAsync(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Navigate(new Uri(UriHelper.LoginUri));
                _ = this.ShowMessageAsync(loader.GetString("CannotGetToken"));
            }
        }

        private async void ManualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _ = this.ShowProgressBarAsync();
            LoginDialog Dialog = new LoginDialog();
            ContentDialogResult result = await Dialog.ShowAsync();
            _ = result == ContentDialogResult.Primary ? CheckLoginAsync() : this.HideProgressBarAsync();
        }

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => _ = CheckLoginAsync();

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(WebView.Source);
    }
}
