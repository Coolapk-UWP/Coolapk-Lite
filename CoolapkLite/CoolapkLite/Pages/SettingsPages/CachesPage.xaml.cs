using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.SettingsPages;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CachesPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(CachesViewModel),
                typeof(CachesPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public CachesViewModel Provider
        {
            get => (CachesViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public CachesPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Provider == null)
            {
                Provider = new CachesViewModel(Dispatcher);
            }

            Provider.LoadMoreStarted += OnLoadMoreStarted;
            Provider.LoadMoreCompleted += OnLoadMoreCompleted;

            if (!Provider.Any)
            {
                _ = Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= OnLoadMoreStarted;
            Provider.LoadMoreCompleted -= OnLoadMoreCompleted;
        }

        private void OnLoadMoreStarted() => _ = this.ShowProgressBarAsync();

        private void OnLoadMoreCompleted() => _ = this.HideProgressBarAsync();

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void ListView_RefreshRequested(object sender, EventArgs e) => _ = Refresh(true);

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case "Delete":
                    Provider?.RemoveImageAsync(element.Tag as StorageFile);
                    break;
                default:
                    break;
            }
        }
    }
}
