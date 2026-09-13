using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.SettingsPages;
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
    public sealed partial class AccountsPage : Page
    {
        public readonly AccountsViewModel Provider;

        public AccountsPage()
        {
            InitializeComponent();
            Provider = AccountsViewModel.Caches.TryGetValue(Dispatcher, out AccountsViewModel provider) ? provider : new AccountsViewModel(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Provider.Count <= 0)
            {
                _ = Refresh(true);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("AccountsPage");
            switch (element.Name)
            {
                case nameof(AddAccount) when SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account:
                    switch (Provider.AddOrReplace(new Credential(account.UID, account.Token)))
                    {
                        case ReplaceStatus.Duplicated:
                            _ = this.ShowMessageAsync(string.Format(loader.GetString("Duplicated"), account.UID));
                            break;
                        case ReplaceStatus.Replaced:
                            _ = this.ShowMessageAsync(string.Format(loader.GetString("Replaced"), account.UID));
                            break;
                        case ReplaceStatus.Added:
                            _ = this.ShowMessageAsync(string.Format(loader.GetString("Added"), account.UID));
                            break;
                    }
                    break;
                case "RemoveAccount" when element.Tag is Credential credential:
                    Provider.Remove(credential);
                    _ = this.ShowMessageAsync(string.Format(loader.GetString("Removed"), credential.UID));
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Tag)
            {
                case "ExportAccount":
                    _ = Provider.ExportAsync();
                    break;
                case "ImportAccount":
                    _ = Provider.ImportAsync();
                    break;
                default:
                    break;
            }
        }

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void FrameworkElement_RefreshEvent() => _ = Refresh(true);
    }
}
