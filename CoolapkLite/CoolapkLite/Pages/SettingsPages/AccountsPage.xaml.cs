using CoolapkLite.Common;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using CoolapkLite.ViewModels.FeedPages;
using CoolapkLite.ViewModels.SettingsPages;
using System.Linq;
using System.Threading.Tasks;
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
            if (Provider.Accounts == null)
            {
                _ = Refresh(true);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case nameof(AddAccount) when SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account:
                    Provider.Accounts.Add(account);
                    _ = Refresh();
                    break;
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedValue is Account account
                && account != SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount))
            {
                _ = this.ShowProgressBarAsync();
                try
                {
                    bool result = await SettingsHelper.LoginAsync(account);
                    _ = this.ShowMessageAsync(result ? "登录成功" : "登录失败");
                }
                finally
                {
                    _ = this.HideProgressBarAsync();
                }
            }
        }

        public async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account)
            {
                ListView.SelectedValue = Provider.Accounts.FirstOrDefault(x => x == account);
            }
        }

        private void FrameworkElement_RefreshEvent() => _ = Refresh(true);
    }
}
