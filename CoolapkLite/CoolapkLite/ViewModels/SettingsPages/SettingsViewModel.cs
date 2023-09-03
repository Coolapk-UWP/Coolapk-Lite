using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class SettingsViewModel : IViewModel
    {
        public static Dictionary<CoreDispatcher, SettingsViewModel> Caches { get; } = new Dictionary<CoreDispatcher, SettingsViewModel>();

        public CoreDispatcher Dispatcher { get; }

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Setting");

        public bool IsLogin
        {
            get => !string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid));
            set => RaisePropertyChangedEvent();
        }

        public bool IsNoPicsMode
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
            set
            {
                if (IsNoPicsMode != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsNoPicsMode, value);
                    RaisePropertyChangedEvent();
                    ThemeHelper.UISettingChanged?.Invoke(UISettingChangedType.NoPicChanged);
                }
            }
        }

        public bool? ShowOtherException
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException);
            set
            {
                if (ShowOtherException != value)
                {
                    SettingsHelper.Set(SettingsHelper.ShowOtherException, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsDisplayOriginPicture
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture);
            set
            {
                if (IsDisplayOriginPicture != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsDisplayOriginPicture, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsUseMultiWindow
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseMultiWindow);
            set
            {
                if (IsUseMultiWindow != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsUseMultiWindow, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool? CheckUpdateWhenLaunching
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLaunching);
            set
            {
                if (CheckUpdateWhenLaunching != value)
                {
                    SettingsHelper.Set(SettingsHelper.CheckUpdateWhenLaunching, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public uint TileUpdateTime
        {
            get => SettingsHelper.Get<uint>(SettingsHelper.TileUpdateTime);
            set
            {
                if (TileUpdateTime != value)
                {
                    SettingsHelper.Set(SettingsHelper.TileUpdateTime, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        private static bool isCleanCacheButtonEnabled = true;
        public bool IsCleanCacheButtonEnabled
        {
            get => isCleanCacheButtonEnabled;
            set => SetProperty(ref isCleanCacheButtonEnabled, value);
        }

        private static bool isCheckUpdateButtonEnabled = true;
        public bool IsCheckUpdateButtonEnabled
        {
            get => isCheckUpdateButtonEnabled;
            set => SetProperty(ref isCheckUpdateButtonEnabled, value);
        }

        private static string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set => SetProperty(ref _aboutTextBlockText, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, SettingsViewModel> cache in Caches)
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

        public string VersionTextBlockText
        {
            get
            {
                string name = ResourceLoader.GetForViewIndependentUse()?.GetString("AppName") ?? Package.Current.DisplayName;
                string ver = Package.Current.Id.Version.ToFormattedString(3);
                _ = GetAboutTextBlockTextAsync();
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
            SettingsHelper.LoginChanged += (sender, args) => IsLogin = args;
        }

        private async Task GetAboutTextBlockTextAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string langCode = LanguageHelper.GetPrimaryLanguage();
            Uri dataUri = new Uri($"ms-appx:///Assets/About/About.{langCode}.md");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            if (file != null)
            {
                string markdown = await FileIO.ReadTextAsync(file);
                AboutTextBlockText = markdown;
            }
        }

        public async Task CleanCacheAsync()
        {
            IsCleanCacheButtonEnabled = false;
            try
            {
                await ImageCacheHelper.CleanCacheAsync().ContinueWith((x) => IsCleanCacheButtonEnabled = true);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Error(ex.ExceptionToMessage(), ex);
                IsCleanCacheButtonEnabled = true;
            }
        }

        public async Task CheckUpdateAsync()
        {
            IsCheckUpdateButtonEnabled = false;
            try
            {
                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");
#if CANARY
                UpdateInfo results = await UpdateHelper.CheckUpdateAsync("wherewhere", "Coolapk-UWP", 5).ConfigureAwait(false);
#else
                UpdateInfo results = await UpdateHelper.CheckUpdateAsync("Coolapk-UWP", "Coolapk-Lite").ConfigureAwait(false);
#endif
                if (results != null)
                {
                    if (results.IsExistNewVersion)
                    {
                        Dispatcher.ShowMessage($"{_loader.GetString("FindUpdate")} {VersionTextBlockText} -> {results.Version.ToString(3)}");
                    }
                    else
                    {
                        Dispatcher.ShowMessage(_loader.GetString("UpToDate"));
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Error(ex.ExceptionToMessage(), ex);
                Dispatcher.ShowMessage(ex.Message);
            }
            IsCheckUpdateButtonEnabled = true;
        }

        public Task Refresh(bool reset) => GetAboutTextBlockTextAsync();

        bool IViewModel.IsEqual(IViewModel other) => other is SettingsViewModel model && IsEqual(model);

        public bool IsEqual(SettingsViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
