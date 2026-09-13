using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public sealed class AccountsViewModel : IList<Credential>, IList, INotifyCollectionChanged, IViewModel
    {
        private const string IndexerName = "Item[]";
        private static readonly AsyncLock locker = new AsyncLock();
        private static readonly PasswordVault vault = new PasswordVault();
        private static readonly List<Credential> _accounts = new List<Credential>();
        public static Dictionary<CoreDispatcher, AccountsViewModel> Caches { get; } = new Dictionary<CoreDispatcher, AccountsViewModel>();

        public string Title => "切换账号";
        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, AccountsViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
            }
        }

        private void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private async void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
        {
            foreach (KeyValuePair<CoreDispatcher, AccountsViewModel> cache in Caches)
            {
                await cache.Key.ResumeForegroundAsync();
                cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(IndexerName));
                cache.Value.CollectionChanged?.Invoke(cache.Value, e);
            }
        }

        #endregion

        public AccountsViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
        }

        #region IList<Credential> Members

        public int Count => _accounts.Count;

        bool ICollection<Credential>.IsReadOnly => ((ICollection<Credential>)_accounts).IsReadOnly;

        public Credential this[int index]
        {
            get => _accounts[index];
            set
            {
                Credential old = _accounts[index];
                if (old.UID == value.UID)
                {
                    if (old.Token == value.Token) { return; }
                    else { Replace(index, old, value); }
                }
                else
                {
                    int oldIndex = _accounts.FindIndex(x => x.UID == value.UID);
                    if (oldIndex >= 0)
                    {
                        old = _accounts[oldIndex];
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
        }

        private void Replace(int index, Credential old, Credential item)
        {
            _accounts[index] = item;
            vault.Remove(old);
            vault.Add(item);
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    item,
                    old,
                    index));
        }

        public ReplaceStatus AddOrReplace(Credential item)
        {
            int index = _accounts.FindIndex(x => x.UID == item.UID);
            if (index >= 0)
            {
                Credential old = _accounts[index];
                if (item.Token != old.Token)
                {
                    Replace(index, old, item);
                    return ReplaceStatus.Replaced;
                }
                return ReplaceStatus.Duplicated;
            }
            else
            {
                index = _accounts.Count;
                _accounts.Add(item);
                vault.Add(item);
                RaisePropertyChangedEvent(nameof(Count));
                RaiseCollectionChangedEvent(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        item,
                        index));
                if (selectedIndex < 0
                    && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account
                    && account.UID == item.UID)
                {
                    SetSelectedIndex(index);
                }
                return ReplaceStatus.Added;
            }
        }

        void ICollection<Credential>.Add(Credential item) => _ = AddOrReplace(item);

        private void AddRange(IEnumerable<Credential> collection)
        {
            int index = _accounts.Count;
            _accounts.AddRange(collection);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    collection is IList list ? list : _accounts.GetRange(index, _accounts.Count - index),
                    index));
        }

        public void Insert(int index, Credential item)
        {
            int oldIndex = _accounts.FindIndex(x => x.UID == item.UID);
            if (oldIndex >= 0)
            {
                Credential old = _accounts[oldIndex];
                if (item.Token != old.Token)
                {
                    Replace(oldIndex, old, item);
                }
            }
            else
            {
                _accounts.Insert(index, item);
                vault.Add(item);
                RaisePropertyChangedEvent(nameof(Count));
                RaiseCollectionChangedEvent(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        item,
                        index));
                if (selectedIndex < 0
                    && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account
                    && account.UID == item.UID)
                {
                    SetSelectedIndex(index);
                }
            }
        }

        public void CopyTo(Credential[] array, int arrayIndex) => _accounts.CopyTo(array, arrayIndex);

        private void Remove(int index, Credential item)
        {
            _accounts.RemoveAt(index);
            vault.Remove(item);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    item,
                    index));
        }

        public bool Remove(Credential item)
        {
            int index = _accounts.IndexOf(item);
            if (index >= 0)
            {
                Remove(index, _accounts[index]);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            Credential old = _accounts[index];
            Remove(index, old);
        }

        private void Clear()
        {
            _accounts.Clear();
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        void ICollection<Credential>.Clear() => Clear();

        public bool Contains(Credential item) => _accounts.Contains(item);

        public int IndexOf(Credential item) => _accounts.IndexOf(item);

        public IEnumerator<Credential> GetEnumerator() => _accounts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_accounts).GetEnumerator();

        #endregion

        #region IList Members

        bool IList.IsFixedSize => ((IList)_accounts).IsFixedSize;

        bool IList.IsReadOnly => ((IList)_accounts).IsReadOnly;

        bool ICollection.IsSynchronized => ((ICollection)_accounts).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_accounts).SyncRoot;

        object IList.this[int index]
        {
            get => ((IList)_accounts)[index];
            set
            {
                if (value is Credential item)
                {
                    this[index] = item;
                }
                else
                {
                    ThrowWrongValueTypeArgumentException(value, typeof(Credential));
                }
            }
        }

        private static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType) =>
            throw new ArgumentException($"The value \"{value}\" is not of type \"{targetType}\" and cannot be used in this collection.", nameof(value));

        int IList.Add(object value)
        {
            if (value is Credential item)
            {
                _ = AddOrReplace(item);
            }
            else
            {
                ThrowWrongValueTypeArgumentException(value, typeof(Credential));
            }
            return Count - 1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is Credential item)
            {
                Insert(index, item);
            }
            else
            {
                ThrowWrongValueTypeArgumentException(value, typeof(Credential));
            }
        }

        void ICollection.CopyTo(Array array, int index) => ((ICollection)_accounts).CopyTo(array, index);

        void IList.Remove(object value)
        {
            if (value is Credential item)
            {
                Remove(item);
            }
        }

        void IList.Clear() => Clear();

        bool IList.Contains(object value) => value is Credential item && Contains(item);

        int IList.IndexOf(object value) => value is Credential item ? IndexOf(item) : -1;

        #endregion

        public int FindIndex(Predicate<Credential> match) => _accounts.FindIndex(match);

        public Task Refresh(bool reset)
        {
            try
            {
                IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(Credential.ResourceName);
                Clear(); AddRange(credentials.Select<PasswordCredential, Credential>(x => vault.Retrieve(Credential.ResourceName, x.UserName)));
                SetSelectedIndex(Count > 0 && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account account ? FindIndex(x => x.UID == account.UID) : -1);
            }
            catch
            {
                _ = Dispatcher.ShowMessageAsync("当前没有已保存的账号");
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
                    List<Credential> accounts = JsonConvert.DeserializeObject<List<Credential>>(content, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }).Where(x => !x.IsEmpty).ToList();
                    if (accounts?.Count > 0)
                    {
                        if (_accounts.Count > 0)
                        {
                            int count = 0;
                            foreach (Credential account in accounts)
                            {
                                if (AddOrReplace(account) != ReplaceStatus.Duplicated)
                                {
                                    count++;
                                }
                            }
                            _ = Dispatcher.ShowMessageAsync($"成功导入 {count} 个账号");
                        }
                        else
                        {
                            AddRange(accounts);
                            accounts.ForEach(x => vault.Add(x));
                            _ = Dispatcher.ShowMessageAsync($"成功导入 {accounts.Count} 个账号");
                            SetSelectedIndex(Count > 0 && SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount) is Account _account ? FindIndex(x => x.UID == _account.UID) : -1);
                        }
                    }
                    else
                    {
                        _ = Dispatcher.ShowMessageAsync("导入的文件中没有有效的账号信息");
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
                if (await CheckWindowsHelloAsync())
                {
                    string content = JsonConvert.SerializeObject(_accounts, _accounts.GetType(), Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

                    string fileName = Title;
                    int index = fileName.LastIndexOf('.');
                    FileSavePicker fileSavePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        SuggestedFileName = $"Coolapk-Accounts_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                        FileTypeChoices = { { "json 文件", new[] { ".json" } } }
                    };

                    StorageFile file = await fileSavePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        await FileIO.WriteTextAsync(file, content);
                        _ = Dispatcher.ShowMessageAsync($"账号列表已导出到 {file.Path}");
                    }
                }
                else
                {
                    _ = Dispatcher.ShowMessageAsync("Windows Hello 验证失败，取消导出账号列表");
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

        private void SetSelectedIndex(int index)
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
                    if (!account.IsEmpty)
                    {
                        bool result = await SettingsHelper.LoginAsync(account).ConfigureAwait(false);
                        _ = Dispatcher.ShowMessageAsync(result ? "登录成功" : "登录失败");
                    }
                    else
                    {
                        _ = Dispatcher.ShowMessageAsync("获取账户信息失败");
                    }
                }
                finally
                {
                    _ = Dispatcher.HideProgressBarAsync();
                }
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AccountsViewModel model && IsEqual(model);

        public bool IsEqual(AccountsViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }

    public sealed class Credential : IEquatable<Credential>
    {
        public const string ResourceName = "CoolapkLite";

        public string UID { get; set; }
        public string Token { get; set; }
        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(UID) || string.IsNullOrEmpty(Token);

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

        public bool Equals(Credential other) => other is Credential && UID == other.UID && Token == other.Token;

        public static bool operator ==(Credential left, Credential right) => EqualityComparer<Credential>.Default.Equals(left, right);

        public static bool operator !=(Credential left, Credential right) => !(left == right);

        public static implicit operator Credential(PasswordCredential credential) => new Credential(credential.UserName, credential.Password);
        public static implicit operator PasswordCredential(Credential credential) => new PasswordCredential(ResourceName, credential.UID, credential.Token);
    }

    public enum ReplaceStatus
    {
        Duplicated = -1,
        Replaced,
        Added
    }
}
