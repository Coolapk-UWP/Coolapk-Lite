using CoolapkLite.Common;
using CoolapkLite.Models;
using CoolapkLite.Models.Network;
using MetroLog;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using IObjectSerializer = Microsoft.Toolkit.Helpers.IObjectSerializer;

namespace CoolapkLite.Helpers
{
    public static partial class SettingsHelper
    {
        public const string Uid = nameof(Uid);
        public const string Token = nameof(Token);
        public const string TileUrl = nameof(TileUrl);
        public const string UserName = nameof(UserName);
        public const string CustomUA = nameof(CustomUA);
        public const string Bookmark = nameof(Bookmark);
        public const string IsUseAPI2 = nameof(IsUseAPI2);
        public const string CustomAPI = nameof(CustomAPI);
        public const string IsFullLoad = nameof(IsFullLoad);
        public const string IsFirstRun = nameof(IsFirstRun);
        public const string IsCustomUA = nameof(IsCustomUA);
        public const string APIVersion = nameof(APIVersion);
        public const string DeviceInfo = nameof(DeviceInfo);
        public const string IsNoPicsMode = nameof(IsNoPicsMode);
        public const string TokenVersion = nameof(TokenVersion);
        public const string IsUseLiteHome = nameof(IsUseLiteHome);
        public const string IsUseAppWindow = nameof(IsUseAppWindow);
        public const string TileUpdateTime = nameof(TileUpdateTime);
        public const string IsUseCompositor = nameof(IsUseCompositor);
        public const string CurrentLanguage = nameof(CurrentLanguage);
        public const string IsUseMultiWindow = nameof(IsUseMultiWindow);
        public const string SelectedAppTheme = nameof(SelectedAppTheme);
        public const string SelectedBackdrop = nameof(SelectedBackdrop);
        public const string IsUseOldEmojiMode = nameof(IsUseOldEmojiMode);
        public const string IsUseVirtualizing = nameof(IsUseVirtualizing);
        public const string IsChangeBrowserUA = nameof(IsChangeBrowserUA);
        public const string IsExtendsTitleBar = nameof(IsExtendsTitleBar);
        public const string IsUseNoPicFallback = nameof(IsUseNoPicFallback);
        public const string ShowOtherException = nameof(ShowOtherException);
        public const string SemaphoreSlimCount = nameof(SemaphoreSlimCount);
        public const string IsUseBackgroundTask = nameof(IsUseBackgroundTask);
        public const string IsEnableLazyLoading = nameof(IsEnableLazyLoading);
        public const string IsDisplayOriginPicture = nameof(IsDisplayOriginPicture);
        public const string CheckUpdateWhenLaunching = nameof(CheckUpdateWhenLaunching);

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);
        public static Task<Type> GetAsync<Type>(string key) => LocalObject.ReadFileAsync<Type>($"Settings/{key}");
        public static Task SetAsync<Type>(string key, Type value) => LocalObject.CreateFileAsync($"Settings/{key}", value);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(Uid))
            {
                LocalObject.Save(Uid, string.Empty);
            }
            if (!LocalObject.KeyExists(Token))
            {
                LocalObject.Save(Token, string.Empty);
            }
            if (!LocalObject.KeyExists(TileUrl))
            {
                LocalObject.Save(TileUrl, "https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle");
            }
            if (!LocalObject.KeyExists(UserName))
            {
                LocalObject.Save(UserName, string.Empty);
            }
            if (!LocalObject.KeyExists(CustomUA))
            {
                LocalObject.Save(CustomUA, UserAgent.Default);
            }
            if (!LocalObject.KeyExists(IsUseAPI2))
            {
                LocalObject.Save(IsUseAPI2, true);
            }
            if (!LocalObject.KeyExists(CustomAPI))
            {
                LocalObject.Save(CustomAPI, new APIVersion("9.2.2", "1905301"));
            }
            if (!LocalObject.KeyExists(IsFullLoad))
            {
                LocalObject.Save(IsFullLoad, true);
            }
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
            }
            if (!LocalObject.KeyExists(IsCustomUA))
            {
                LocalObject.Save(IsCustomUA, false);
            }
            if (!LocalObject.KeyExists(APIVersion))
            {
                LocalObject.Save(APIVersion, APIVersions.V13);
            }
            if (!LocalObject.KeyExists(DeviceInfo))
            {
                LocalObject.Save(DeviceInfo, Models.Network.DeviceInfo.Default);
            }
            if (!LocalObject.KeyExists(IsNoPicsMode))
            {
                LocalObject.Save(IsNoPicsMode, false);
            }
            if (!LocalObject.KeyExists(TokenVersion))
            {
                LocalObject.Save(TokenVersion, TokenVersions.TokenV2);
            }
            if (!LocalObject.KeyExists(IsUseLiteHome))
            {
                LocalObject.Save(IsUseLiteHome, false);
            }
            if (!LocalObject.KeyExists(IsUseAppWindow))
            {
                LocalObject.Save(IsUseAppWindow, false);
            }
            if (!LocalObject.KeyExists(TileUpdateTime))
            {
                LocalObject.Save(TileUpdateTime, SystemInformation.Instance.OperatingSystemVersion.Build < 21996 ? 15u : 0u);
            }
            if (!LocalObject.KeyExists(IsUseCompositor))
            {
                LocalObject.Save(IsUseCompositor, ApiInfoHelper.IsGetElementVisualSupported);
            }
            if (!LocalObject.KeyExists(CurrentLanguage))
            {
                LocalObject.Save(CurrentLanguage, LanguageHelper.AutoLanguageCode);
            }
            if (!LocalObject.KeyExists(IsUseMultiWindow))
            {
                LocalObject.Save(IsUseMultiWindow, true);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
            }
            if (!LocalObject.KeyExists(SelectedBackdrop))
            {
                LocalObject.Save(SelectedBackdrop, BackdropType.Default);
            }
            if (!LocalObject.KeyExists(IsUseOldEmojiMode))
            {
                LocalObject.Save(IsUseOldEmojiMode, false);
            }
            if (!LocalObject.KeyExists(IsUseVirtualizing))
            {
                LocalObject.Save(IsUseVirtualizing, true);
            }
            if (!LocalObject.KeyExists(IsChangeBrowserUA))
            {
                LocalObject.Save(IsChangeBrowserUA, false);
            }
            if (!LocalObject.KeyExists(IsExtendsTitleBar))
            {
                LocalObject.Save(IsExtendsTitleBar, SystemInformation.Instance.OperatingSystemVersion.Build >= 10586);
            }
            if (!LocalObject.KeyExists(IsUseNoPicFallback))
            {
                LocalObject.Save(IsUseNoPicFallback, false);
            }
            if (!LocalObject.KeyExists(ShowOtherException))
            {
                LocalObject.Save(ShowOtherException, true);
            }
            if (!LocalObject.KeyExists(SemaphoreSlimCount))
            {
                LocalObject.Save(SemaphoreSlimCount, Environment.ProcessorCount);
            }
            if (!LocalObject.KeyExists(IsUseBackgroundTask))
            {
                LocalObject.Save(IsUseBackgroundTask, true);
            }
            if (!LocalObject.KeyExists(IsEnableLazyLoading))
            {
                LocalObject.Save(IsEnableLazyLoading, false);
            }
            if (!LocalObject.KeyExists(IsDisplayOriginPicture))
            {
                LocalObject.Save(IsDisplayOriginPicture, false);
            }
            if (!LocalObject.KeyExists(CheckUpdateWhenLaunching))
            {
                LocalObject.Save(CheckUpdateWhenLaunching, false);
            }
            _ = SetDefaultSettingsAsync();
        }

        public static async Task SetDefaultSettingsAsync()
        {
            StorageFolder folder = LocalObject.Folder;
            StorageFolder settings = await folder.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
            if (await settings.TryGetItemAsync(Bookmark) == null)
            {
                await SetAsync(Bookmark, Models.Bookmark.GetDefaultBookmarks()).ConfigureAwait(false);
            }
        }
    }

    public static partial class SettingsHelper
    {
        public static ILogManager LogManager { get; } = LogManagerFactory.CreateLogManager();
        public static ApplicationDataStorageHelper LocalObject { get; } = ApplicationDataStorageHelper.GetCurrent(new NewtonsoftJsonObjectSerializer());

        private static ImmutableDictionary<int, string> userRemarks;
        public static ImmutableDictionary<int, string> UserRemarks
        {
            get => userRemarks;
            set
            {
                if (userRemarks != value)
                {
                    userRemarks = value;
                }
                InvokeUserRemarksChanged(value);
            }
        }

        #region LoginChanged

        private static readonly WeakEvent<bool> actions = new WeakEvent<bool>();

        public static event Action<bool> LoginChanged
        {
            add => actions.Add(value);
            remove => actions.Remove(value);
        }

        public static void InvokeLoginChanged(bool args) => actions?.Invoke(args);

        #endregion

        #region UserRemarksChanged

        private static readonly WeakEvent<ImmutableDictionary<int, string>> remarks = new WeakEvent<ImmutableDictionary<int, string>>();

        public static event Action<ImmutableDictionary<int, string>> UserRemarksChanged
        {
            add => remarks.Add(value);
            remove => remarks.Remove(value);
        }

        public static void InvokeUserRemarksChanged(ImmutableDictionary<int, string> args) => remarks?.Invoke(args);

        #endregion

        static SettingsHelper() => SetDefaultSettings();

        public static async Task<bool> LoginAsync()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.CoolapkUri))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName) || !await RequestHelper.CheckLoginAsync().ConfigureAwait(false))
                {
                    Logout();
                    return false;
                }
                else
                {
                    Set(Uid, uid);
                    Set(Token, token);
                    Set(UserName, userName);
                    InvokeLoginChanged(true);
                    _ = RemarkModel.GetRemarkDictionary(uid).ContinueWith(x => UserRemarks = x.Result);
                    return true;
                }
            }
        }

        public static async Task<bool> LoginAsync(string Uid, string UserName, string Token)
        {
            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = Uid;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
                if (await RequestHelper.CheckLoginAsync().ConfigureAwait(false))
                {
                    Set(SettingsHelper.Uid, Uid);
                    Set(SettingsHelper.Token, Token);
                    Set(SettingsHelper.UserName, UserName);
                    InvokeLoginChanged(true);
                    _ = RemarkModel.GetRemarkDictionary(Uid).ContinueWith(x => UserRemarks = x.Result);
                    return true;
                }
                else
                {
                    Logout();
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> CheckLoginAsync()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.CoolapkUri))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                bool value = !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName) && await RequestHelper.CheckLoginAsync().ConfigureAwait(false);
                if (value && UserRemarks == null)
                {
                    _ = RemarkModel.GetRemarkDictionary(uid).ContinueWith(x => UserRemarks = x.Result);
                }
                return value;
            }
        }

        public static void Logout()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.Base2Uri))
                {
                    cookieManager.DeleteCookie(item);
                }
            }
            Set(Uid, string.Empty);
            Set(Token, string.Empty);
            Set(UserName, string.Empty);
            InvokeLoginChanged(false);
            UserRemarks = null;
        }
    }

    public class NewtonsoftJsonObjectSerializer : IObjectSerializer
    {
        // Specify your serialization settings
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore };

        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value, typeof(T), Formatting.Indented, settings);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, settings);
    }
}
