using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Update;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class SettingsViewModel : IViewModel
    {
        public static SettingsViewModel Caches { get; set; }

        public CoreDispatcher Dispatcher { get; }

        public string Title => ResourceLoader.GetForCurrentView("MainPage").GetString("Setting");

        public static string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string ToolkitVersion => Assembly.Load(new AssemblyName("Microsoft.Toolkit.Uwp")).GetName().Version.ToString();

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

        public bool? CheckUpdateWhenLuanching
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching);
            set
            {
                if (CheckUpdateWhenLuanching != value)
                {
                    SettingsHelper.Set(SettingsHelper.CheckUpdateWhenLuanching, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCleanCacheButtonEnabled = true;
        public bool IsCleanCacheButtonEnabled
        {
            get => isCleanCacheButtonEnabled;
            set
            {
                if (isCleanCacheButtonEnabled != value)
                {
                    isCleanCacheButtonEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCheckUpdateButtonEnabled = true;
        public bool IsCheckUpdateButtonEnabled
        {
            get => isCheckUpdateButtonEnabled;
            set
            {
                if (isCheckUpdateButtonEnabled != value)
                {
                    isCheckUpdateButtonEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set
            {
                if (_aboutTextBlockText != value)
                {
                    _aboutTextBlockText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                string name = loader?.GetString("AppName") ?? "酷安 Lite";
                _ = GetAboutTextBlockText();
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel(CoreDispatcher dispatcher)
        {
            Caches = this;
            Dispatcher = dispatcher;
            SettingsHelper.LoginChanged += (sender, args) => IsLogin = args;
        }

        private async Task GetAboutTextBlockText()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string langcode = LanguageHelper.GetPrimaryLanguage();
            Uri dataUri = new Uri($"ms-appx:///Assets/About/About.{langcode}.md");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            if (file != null)
            {
                string markdown = await FileIO.ReadTextAsync(file);
                AboutTextBlockText = markdown;
            }
        }

        public async void CleanCache()
        {
            IsCleanCacheButtonEnabled = true;
            await ImageCacheHelper.CleanCacheAsync();
            IsCleanCacheButtonEnabled = false;
        }

        public async void CheckUpdate()
        {
            IsCheckUpdateButtonEnabled = false;
            try
            {
                ResourceLoader _loader = ResourceLoader.GetForCurrentView("SettingsPage");
                UpdateInfo results = await UpdateHelper.CheckUpdateAsync("Coolapk-UWP", "Coolapk-Lite");
                if (results != null && results.IsExistNewVersion)
                {
                    UIHelper.ShowMessage($"{_loader.GetString("FindUpdate")} {VersionTextBlockText} -> {results.TagName}");
                }
                else
                {
                    UIHelper.ShowMessage(_loader.GetString("UpToDate"));
                }
            }
            catch (HttpRequestException ex)
            {
                UIHelper.ShowHttpExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                UIHelper.ShowMessage(ex.Message);
            }
            IsCheckUpdateButtonEnabled = true;
        }

        public Task Refresh(bool reset) => throw new NotImplementedException();

        bool IViewModel.IsEqual(IViewModel other) => Equals(other);
    }
}
