using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Network;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.StartScreen;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class TestViewModel : IViewModel
    {
        public static Dictionary<CoreDispatcher, TestViewModel> Caches { get; } = new Dictionary<CoreDispatcher, TestViewModel>();

        public static bool IsJumpListSupported { get; } = ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported();

        public static string FrameworkDescription { get; } = RuntimeInformation.FrameworkDescription;

        public static string DeviceFamily { get; } = AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string OperatingSystemVersion { get; } = SystemInformation.Instance.OperatingSystemVersion.ToString();

        public static string OSArchitecture { get; } = RuntimeInformation.OSArchitecture.ToString();

        public CoreDispatcher Dispatcher { get; }

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Test");

        public bool IsAppWindowSupported => WindowHelper.IsAppWindowSupported;

        public bool IsSearchPaneSupported => SettingsPaneRegister.IsSearchPaneSupported;

        public bool IsAppExtensionSupported => ExtensionManager.IsOSSUploaderSupported;

        public bool IsSettingsPaneSupported => SettingsPaneRegister.IsSettingsPaneSupported;

        public bool IsCompactOverlaySupported => ApiInfoHelper.IsApplicationViewViewModeSupported;

        public bool IsGetElementVisualSupported => ApiInfoHelper.IsGetElementVisualSupported;

        public bool IsRequestRestartAsyncSupported => ApiInfoHelper.IsRequestRestartAsyncSupported;

        public bool IsXamlCompositionBrushSupported => ApiInfoHelper.IsXamlCompositionBrushBaseSupported;

        public bool IsRequestInfoForAppAsyncSupported => ApiInfoHelper.IsRequestInfoForAppAsyncSupported;

        public ImmutableArray<CultureInfo> SupportCultures => LanguageHelper.SupportCultures;

        public bool IsExtendsTitleBar
        {
            get => CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            set
            {
                if (IsExtendsTitleBar != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsExtendsTitleBar, value);
                    ThemeHelper.UpdateExtendViewIntoTitleBar(value);
                    ThemeHelper.UpdateSystemCaptionButtonColors();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsUseAPI2
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseAPI2);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseAPI2, value);
                UriHelper.IsUseAPI2 = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsFullLoad
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsFullLoad);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsFullLoad, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsCustomUA
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            set
            {
                if (IsCustomUA != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsCustomUA, value);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int APIVersion
        {
            get => (int)SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion) - 4;
            set
            {
                if (APIVersion != value)
                {
                    SettingsHelper.Set(SettingsHelper.APIVersion, value + 4);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsUseTokenV2
        {
            get => SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion) == TokenVersions.TokenV2;
            set
            {
                if (IsUseTokenV2 != value)
                {
                    SettingsHelper.Set(SettingsHelper.TokenVersion, (int)(value ? TokenVersions.TokenV2 : TokenVersions.TokenV1));
                    NetworkHelper.SetRequestHeaders();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsUseLiteHome
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseLiteHome, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseAppWindow
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseAppWindow);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseAppWindow, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseBlurBrush
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseBlurBrush);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseBlurBrush, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseCompositor
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseCompositor, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseVirtualizing
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseVirtualizing);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseVirtualizing, value);
                ItemsPanelSelector.IsVirtualizing = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsChangeBrowserUA
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsChangeBrowserUA);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsChangeBrowserUA, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseNoPicFallback
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseNoPicFallback);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseNoPicFallback, value);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseBackgroundTask
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseBackgroundTask);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseBackgroundTask, value);
                RaisePropertyChangedEvent();
                _ = UpdateBackgroundTask(value);
            }
        }

        public bool IsEnableLazyLoading
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsEnableLazyLoading);
            set
            {
                SettingsHelper.Set(SettingsHelper.IsEnableLazyLoading, value);
                RaisePropertyChangedEvent();
            }
        }

        public double SemaphoreSlimCount
        {
            get => SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount);
            set
            {
                if (SemaphoreSlimCount != value)
                {
                    int result = (int)Math.Floor(value);
                    SettingsHelper.Set(SettingsHelper.SemaphoreSlimCount, result);
                    ImageModel.SetSemaphoreSlim(result);
                    RaisePropertyChangedEvent();
                }
            }
        }

        private static string userAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
        public string UserAgent
        {
            get => userAgent;
            set
            {
                if (userAgent != value)
                {
                    userAgent = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, TestViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
            }
        }

        protected static async void RaisePropertyChangedEvent(params string[] names)
        {
            if (names?.Any() == true)
            {
                foreach (KeyValuePair<CoreDispatcher, TestViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    names.ForEach(name => cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name)));
                }
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

        public TestViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
        }

        public async Task UpdateBackgroundTask(bool isEnable)
        {
            if (!ApiInfoHelper.IsTileActivatedInfoSupported)
            { return; }

            // Check for background access (optional)
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            if (status != BackgroundAccessStatus.Unspecified
                && status != BackgroundAccessStatus.Denied
                && status != (BackgroundAccessStatus)7)
            {
                if (isEnable)
                {
                    RegisterLiveTileTask();
                    RegisterNotificationsTask();
                }
                else
                {
                    UnregisterLiveTileTask();
                    UnregisterNotificationsTask();
                }
            }

            #region LiveTileTask

            const string LiveTileTask = nameof(BackgroundTasks.LiveTileTask);

            void RegisterLiveTileTask()
            {
                uint time = SettingsHelper.Get<uint>(SettingsHelper.TileUpdateTime);
                if (time < 15)
                {
                    UnregisterLiveTileTask();
                    return;
                }

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(LiveTileTask, new TimeTrigger(time, false), true);
            }

            void UnregisterLiveTileTask()
            {
                // If background task is not registered, do nothing
                if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Unregister (Single Process)
                BackgroundTaskHelper.Unregister(LiveTileTask);
            }

            #endregion

            #region NotificationsTask

            const string NotificationsTask = nameof(BackgroundTasks.NotificationsTask);

            void RegisterNotificationsTask()
            {
                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(NotificationsTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _NotificationsTask = BackgroundTaskHelper.Register(NotificationsTask, new TimeTrigger(15, false), true);
            }

            void UnregisterNotificationsTask()
            {
                // If background task is not registered, do nothing
                if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(NotificationsTask)))
                { return; }

                // Unregister (Single Process)
                BackgroundTaskHelper.Unregister(NotificationsTask);
            }

            #endregion
        }

        public static void Refresh(bool reset)
        {
            if (reset)
            {
                RaisePropertyChangedEvent(
                    nameof(IsExtendsTitleBar),
                    nameof(IsUseAPI2),
                    nameof(IsFullLoad),
                    nameof(IsCustomUA),
                    nameof(APIVersion),
                    nameof(IsUseTokenV2),
                    nameof(IsUseLiteHome),
                    nameof(IsUseAppWindow),
                    nameof(IsUseBlurBrush),
                    nameof(IsUseCompositor),
                    nameof(IsUseVirtualizing),
                    nameof(IsChangeBrowserUA),
                    nameof(IsUseBackgroundTask),
                    nameof(IsEnableLazyLoading),
                    nameof(SemaphoreSlimCount));
            }
            userAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
            RaisePropertyChangedEvent(nameof(UserAgent));
        }

        Task IViewModel.Refresh(bool reset) => Task.Run(() => UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString());

        bool IViewModel.IsEqual(IViewModel other) => other is TestViewModel model && IsEqual(model);

        public bool IsEqual(TestViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
