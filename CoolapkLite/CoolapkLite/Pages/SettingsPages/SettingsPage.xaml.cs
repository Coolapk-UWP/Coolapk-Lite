using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Update;
using System;
using System.ComponentModel;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private Thickness ScrollViewerPadding => UIHelper.ScrollViewerPadding;

        private const string IssuePath = "https://github.com/Coolapk-UWP/Coolapk-Lite/issues";

        internal bool IsLogin
        {
            get => SettingsHelper.CheckLoginInfoFast();
            set => RaisePropertyChangedEvent();
        }

        internal bool IsNoPicsMode
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

        internal bool? ShowOtherException
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

        internal bool? CheckUpdateWhenLuanching
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
        internal bool IsCleanCacheButtonEnabled
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
        internal bool IsCheckUpdateButtonEnabled
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

        internal string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                string name = loader?.GetString("AppName") ?? "酷安 Lite";
                return $"{name} v{ver}";
            }
        }

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Setting");
#if DEBUG
            GoToTestPage.Visibility = Visibility.Visible;
#endif
            switch (ThemeHelper.ActualTheme)
            {
                case ElementTheme.Light:
                    Light.IsChecked = true;
                    break;
                case ElementTheme.Dark:
                    Dark.IsChecked = true;
                    break;
                case ElementTheme.Default:
                    Default.IsChecked = true;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Reset":
                    ApplicationData.Current.LocalSettings.Values.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingsPage));
                    Frame.GoBack();
                    break;
                case "MyDevice":
                    _ = Frame.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
                    break;
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "FeedBack":
                    _ = Frame.Navigate(typeof(BrowserPage), new object[] { false, IssuePath });
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "CleanCache":
                    IsCleanCacheButtonEnabled = false;
                    await ImageCacheHelper.CleanCacheAsync();
                    IsCleanCacheButtonEnabled = true;
                    break;
                case "CheckUpdate":
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
                    break;
                case "AccountSetting":
                    _ = Frame.Navigate(typeof(BrowserPage), new object[] { false, "https://account.coolapk.com/account/settings" });
                    break;
                case "AccountLogout":
                    SettingsHelper.Logout();
                    IsLogin = false;
                    if (AccountLogout.Flyout is Flyout flyout_logout)
                    {
                        flyout_logout.Hide();
                    }
                    break;
                default:
                    break;
            }
        }

        private void Button_Checked(object sender, RoutedEventArgs _)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Dark":
                    ThemeHelper.RootTheme = ElementTheme.Dark;
                    break;
                case "Light":
                    ThemeHelper.RootTheme = ElementTheme.Light;
                    break;
                case "Default":
                    ThemeHelper.RootTheme = ElementTheme.Default;
                    break;
                default:
                    break;
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
