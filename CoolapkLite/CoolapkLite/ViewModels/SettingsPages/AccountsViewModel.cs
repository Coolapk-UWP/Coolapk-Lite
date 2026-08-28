using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using CoolapkLite.ViewModels.FeedPages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class AccountsViewModel : IViewModel
    {
        public static Dictionary<CoreDispatcher, AccountsViewModel> Caches { get; } = new Dictionary<CoreDispatcher, AccountsViewModel>();

        public string Title => "切换账号";

        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public AccountsViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
        }

        public async Task Refresh(bool reset)
        {
            if (_accounts != null)
            {
                await SettingsHelper.SetAsync(SettingsHelper.Accounts, _accounts.ToArray()).ConfigureAwait(false);
            }
            if (reset)
            {
                await ResetAsync().ConfigureAwait(false);
            }
            RefreshOthers();
        }

        private async Task ResetAsync() => Accounts = await SettingsHelper.GetAsync<Account[]>(SettingsHelper.Accounts).ContinueWith(x => new ObservableCollection<Account>(x.Result)).ConfigureAwait(false);

        private void RefreshOthers()
        {
            foreach (KeyValuePair<CoreDispatcher, AccountsViewModel> cache in Caches)
            {
                if (cache.Key != Dispatcher)
                {
                    _ = cache.Value.ResetAsync();
                }
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AccountsViewModel model && IsEqual(model);

        public bool IsEqual(AccountsViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
