using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(IndexViewModel),
                typeof(IndexPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public IndexViewModel Provider
        {
            get => (IndexViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public IndexPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Provider == null)
            {
                Provider = new IndexViewModel(Dispatcher);
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

        private Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void FrameworkElement_RefreshEvent() => _ = Refresh(true);
    }
}
