using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public sealed class AccountsViewModel : CachedListViewModelBase<AccountsViewModel, Credential>
    {
        public static readonly PasswordVault PasswordVault = new PasswordVault();

        public string Title => ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Accounts");

        private static int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    _ = SetAccountAsync(value);
                    SetProperty(ref selectedIndex, value);
                }
            }
        }

        public AccountsViewModel(CoreDispatcher dispatcher) : base(dispatcher) { }

        static AccountsViewModel() => SettingsHelper.LoginChanged += isLogin => SetSelectedIndex(isLogin && _items.Count > 0 && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account ? _items.FindIndex(x => x.UID == account.UID) : -1);

        #region IList<Credential> Members

        protected override void SetIndex(int index, Credential value)
        {
            Credential old = _items[index];
            if (old.UID == value.UID)
            {
                if (old.Token == value.Token) { return; }
                else { Replace(index, old, value); }
            }
            else
            {
                int oldIndex = _items.FindIndex(x => x.UID == value.UID);
                if (oldIndex >= 0)
                {
                    old = _items[oldIndex];
                    if (value.Token != old.Token)
                    {
                        Replace(oldIndex, old, value);
                    }
                    return;
                }
                else
                {
                    Replace(index, old, value);
                    if (SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account
                        && account.UID == value.UID)
                    {
                        SetSelectedIndex(index);
                    }
                }
            }
        }

        protected override void Replace(int index, Credential old, Credential item)
        {
            PasswordVault.Remove(old);
            PasswordVault.Add(item);
            base.Replace(index, old, item);
        }

        public override ReplaceStatus AddOrReplace(Credential item)
        {
            if (_items.Count >= 20)
            {
                return ReplaceStatus.Maxed;
            }
            int index = _items.FindIndex(x => x.UID == item.UID);
            if (index >= 0)
            {
                Credential old = _items[index];
                if (item.Token != old.Token)
                {
                    Replace(index, old, item);
                    return ReplaceStatus.Replaced;
                }
                return ReplaceStatus.Duplicated;
            }
            else
            {
                PasswordVault.Add(item);
                ReplaceStatus status = base.AddOrReplace(item);
                if (selectedIndex < 0
                    && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account
                    && account.UID == item.UID)
                {
                    SetSelectedIndex(index);
                }
                return status;
            }
        }

        public override void Insert(int index, Credential item)
        {
            int oldIndex = _items.FindIndex(x => x.UID == item.UID);
            if (oldIndex >= 0)
            {
                Credential old = _items[oldIndex];
                if (item.Token != old.Token)
                {
                    Replace(oldIndex, old, item);
                }
            }
            else
            {
                PasswordVault.Add(item);
                base.Insert(index, item);
                if (selectedIndex < 0
                    && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account
                    && account.UID == item.UID)
                {
                    SetSelectedIndex(index);
                }
            }
        }

        protected override void Remove(int index, Credential item)
        {
            PasswordVault.Remove(item);
            base.Remove(index, item);
        }

        #endregion

        public int FindIndex(Predicate<Credential> match) => _items.FindIndex(match);

        public override Task Refresh(bool reset = true)
        {
            try
            {
                IReadOnlyList<PasswordCredential> credentials = PasswordVault.FindAllByResource(Credential.ResourceName);
                Clear(); AddRange(credentials.Select<PasswordCredential, Credential>(x => x));
                SetSelectedIndex(Count > 0 && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account ? FindIndex(x => x.UID == account.UID) : -1);
            }
            catch
            {
                _ = Dispatcher.ShowMessageAsync(ResourceLoader.GetForViewIndependentUse("AccountsPage").GetString("NoAccount"));
            }
            return Task.CompletedTask;
        }

        public async Task ImportAsync()
        {
            try
            {
                FileOpenPicker fileOpenPicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };
                fileOpenPicker.FileTypeFilter.Add(".json");

                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    string content = await FileIO.ReadTextAsync(file);
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("AccountsPage");
                    List<Credential> accounts = JsonConvert.DeserializeObject<List<Credential>>(content, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }).Where(x => !x.IsEmpty).ToList();
                    if (accounts?.Count > 0)
                    {
                        if (_items.Count > 0)
                        {
                            int count = 0;
                            foreach (Credential account in accounts)
                            {
                                ReplaceStatus status = AddOrReplace(account);
                                if (status == ReplaceStatus.Added || status == ReplaceStatus.Replaced)
                                {
                                    count++;
                                }
                            }
                            _ = Dispatcher.ShowMessageAsync(string.Format(loader.GetString("ImportSucceed"), count));
                        }
                        else
                        {
                            AddRange(accounts);
                            accounts.ForEach(x => PasswordVault.Add(x));
                            _ = Dispatcher.ShowMessageAsync(string.Format(loader.GetString("ImportSucceed"), accounts.Count));
                            SetSelectedIndex(Count > 0 && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account _account ? FindIndex(x => x.UID == _account.UID) : -1);
                        }
                    }
                    else
                    {
                        _ = Dispatcher.ShowMessageAsync(loader.GetString("ImportFailed"));
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(AccountsViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public async Task ExportAsync()
        {
            try
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("AccountsPage");
                if (await CheckWindowsHelloAsync())
                {
                    string content = JsonConvert.SerializeObject(_items, _items.GetType(), Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

                    string fileName = Title;
                    int index = fileName.LastIndexOf('.');
                    FileSavePicker fileSavePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        SuggestedFileName = $"Coolapk-Accounts_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                        FileTypeChoices = { { string.Format(ResourceLoader.GetForViewIndependentUse().GetString("FileExtDescription"), "json"), new[] { ".json" } } }
                    };

                    StorageFile file = await fileSavePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        await FileIO.WriteTextAsync(file, content);
                        _ = Dispatcher.ShowMessageAsync(string.Format(loader.GetString("ExportSucceed"), file.Path));
                    }
                }
                else
                {
                    _ = Dispatcher.ShowMessageAsync(loader.GetString("ExportFailed"));
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(AccountsViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        private async Task<bool> CheckWindowsHelloAsync()
        {
            try
            {
                const string name = "AccountManager";

                // Do we have capability to provide credentials from the device
                if (await KeyCredentialManager.IsSupportedAsync())
                {
                    // Get credentials for current user and app
                    KeyCredentialRetrievalResult result = await KeyCredentialManager.OpenAsync(name);

                    if (result.Credential != null)
                    {
                        KeyCredentialOperationResult signResult = await result.Credential.RequestSignAsync(CryptographicBuffer.ConvertStringToBinary("LoginAuth", BinaryStringEncoding.Utf8));
                        if (signResult.Status == KeyCredentialStatus.Success)
                        {
                            return true;
                        }
                    }
                    // No previous saved credentials found
                    else
                    {
                        KeyCredentialRetrievalResult creationResult = await KeyCredentialManager.RequestCreateAsync(name, KeyCredentialCreationOption.ReplaceExisting);
                        if (creationResult.Status == KeyCredentialStatus.Success)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(AccountsViewModel)).Error(ex.ExceptionToMessage(), ex);
            }

            return false;
        }

        private static void SetSelectedIndex(int index)
        {
            selectedIndex = index;
            RaisePropertyChangedEvent(nameof(SelectedIndex));
        }

        private async Task SetAccountAsync(int index)
        {
            if (index < 0 || index >= Count) { return; }
            Credential credential = this[index];
            if (credential.UID != SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount).UID)
            {
                _ = Dispatcher.ShowProgressBarAsync();
                try
                {
                    Account account = await credential.GetAccountAsync().ConfigureAwait(false);
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");
                    if (!account.IsEmpty)
                    {
                        bool result = await SettingsHelper.LoginAsync(account).ConfigureAwait(false);
                        _ = Dispatcher.ShowMessageAsync(loader.GetString(result ? "LoginSuccessfully" : "LoginFailed"));
                    }
                    else
                    {
                        _ = Dispatcher.ShowMessageAsync(loader.GetString("GetUserInfoFailed"));
                    }
                }
                finally
                {
                    _ = Dispatcher.HideProgressBarAsync();
                }
            }
        }

        public bool IsEqual(AccountsViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }

    public sealed class Credential : IEquatable<Credential>
    {
        public static readonly string ResourceName = Package.Current.Id.Name;

        public string UID { get; set; }

        private string token;
        public string Token
        {
            get
            {
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(UID))
                {
                    try { token = AccountsViewModel.PasswordVault.Retrieve(ResourceName, UID).Password; }
                    catch (Exception ex) { SettingsHelper.LogManager.GetLogger(nameof(Credential)).Error(ex.ExceptionToMessage(), ex); }
                }
                return token;
            }
            set => token = value;
        }

        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(UID);

        public Credential() { }

        public Credential(string uid, string password) : this()
        {
            UID = uid;
            Token = password;
        }

        public async Task<Account> GetAccountAsync()
        {
            if (!string.IsNullOrEmpty(UID))
            {
                if (await NetworkHelper.GetUserInfoByNameAsync(UID) is UserInfoModel results)
                {
                    return new Account(results.UID.ToString(), WebUtility.UrlEncode(results.UserName), Token);
                }
            }
            return default;
        }

        public override bool Equals(object obj) => Equals(obj as Credential);

        public override int GetHashCode() => (UID, Token).GetHashCode();

        public bool Equals(Credential other) => (object)this == other || (other is Credential && UID == other.UID && Token == other.Token);

        public static bool operator ==(Credential left, Credential right) => left?.Equals(right) ?? (right is null);

        public static bool operator !=(Credential left, Credential right) => !(left == right);

        public static implicit operator Credential(PasswordCredential credential) => new Credential(credential.UserName, credential.Password);
        public static implicit operator PasswordCredential(Credential credential) => new PasswordCredential(ResourceName, credential.UID, credential.Token);
    }
}
