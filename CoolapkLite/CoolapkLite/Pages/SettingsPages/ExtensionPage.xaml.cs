using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ExtensionPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(ExtensionManager),
                typeof(ExtensionPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public ExtensionManager Provider
        {
            get => (ExtensionManager)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("Extension");

        public ExtensionPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = Provider ?? new ExtensionManager(ExtensionManager.OSSUploader);
            await Refresh(true);
        }

        public async Task Refresh(bool reset = false)
        {
            this.ShowProgressBar();
            if (reset)
            {
                await Provider.Initialize(Dispatcher);
            }
            else
            {
                await Provider.FindAndLoadExtensions();
            }
            this.HideProgressBar();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case "Uninstall":
                    Provider.RemoveExtension(element.Tag as Extension);
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness StackPanelMargin;
            Thickness ScrollViewerMargin;
            Thickness ScrollViewerPadding;

            StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
            ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();

            if (StackPanel != null)
            {
                StackPanel.Margin = StackPanelMargin;
                StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = ScrollViewerMargin;
                ScrollViewer.Padding = ScrollViewerPadding;
            }
        }
    }
}
