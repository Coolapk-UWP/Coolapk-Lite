using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        public CoreDispatcher Dispatcher { get; }

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Test");

        public bool IsJumpListSupported { get; } = ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported();

        public bool IsAppWindowSupported { get; } = WindowHelper.IsAppWindowSupported;

        public bool IsSearchPaneSupported { get; } = SettingsPaneRegister.IsSearchPaneSupported;

        public bool IsSettingsPaneSupported { get; } = SettingsPaneRegister.IsSettingsPaneSupported;

        public bool IsAppExtensionSupported { get; } = ExtensionManager.IsOSSUploaderSupported;

        public bool IsCompactOverlaySupported { get; } = ApiInfoHelper.IsApplicationViewViewModeSupported;

        public bool IsGetElementVisualSupported { get; } = ApiInfoHelper.IsGetElementVisualSupported;

        public bool IsAppDiagnosticInfoSupported { get; } = ApiInfoHelper.IsAppDiagnosticInfoSupported;

        public bool IsXamlCompositionBrushSupported { get; } = ApiInfoHelper.IsXamlCompositionBrushBaseSupported;

        public ImmutableArray<CultureInfo> SupportCultures => LanguageHelper.SupportCultures;

        public string FrameworkDescription => RuntimeInformation.FrameworkDescription;

        public string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public string OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion.ToString();

        public string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();

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

        public double SemaphoreSlimCount
        {
            get => SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount);
            set
            {
                if (SemaphoreSlimCount != value)
                {
                    int result = (int)Math.Floor(value);
                    SettingsHelper.Set(SettingsHelper.SemaphoreSlimCount, result);
                    NetworkHelper.SetSemaphoreSlim(result);
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
                    if (cache.Key?.HasThreadAccess == false)
                    {
                        await cache.Key.ResumeForegroundAsync();
                    }
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
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

        public Task Refresh(bool reset) => Task.Run(() => UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString());

        bool IViewModel.IsEqual(IViewModel other) => other is TestViewModel model && IsEqual(model);

        public bool IsEqual(TestViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
