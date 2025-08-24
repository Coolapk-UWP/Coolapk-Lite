using CoolapkLite.Common;
using CoolapkLite.Helpers;
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
        private readonly ExtensionManager Provider = new ExtensionManager(ExtensionManager.OSSUploader);

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Extension");

        public ExtensionPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!Provider.IsInitialized)
            {
                _ = Refresh(true);
            }
        }

        public async Task Refresh(bool reset = false)
        {
            _ = this.ShowProgressBarAsync();
            try
            {
                await (reset
                    ? Provider.InitializeAsync(Dispatcher).ConfigureAwait(false)
                    : Provider.FindAndLoadExtensionsAsync().ConfigureAwait(false));
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
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

        private void FrameworkElement_RefreshEvent() => _ = Refresh(true);
    }
}
