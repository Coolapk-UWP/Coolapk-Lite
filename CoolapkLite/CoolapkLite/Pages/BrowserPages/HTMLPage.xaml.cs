using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.BrowserPages;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.BrowserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HTMLPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(HTMLViewModel),
                typeof(HTMLPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public HTMLViewModel Provider
        {
            get => (HTMLViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public HTMLPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is HTMLViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Refresh(true);
            }
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                args.Cancel = true;
                _ = this.OpenLinkAsync(args.Uri.AbsoluteUri);
            }
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshRequested(TitleBar sender, object args) => _ = Refresh(true);
    }
}
