using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed partial class SettingsPage : Page
    {
        private Action<UISettingChangedType> UISettingChanged;

        internal SettingsViewModel Provider;

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UISettingChanged = (mode) => UpdateThemeRadio();
            Provider = SettingsViewModel.Caches ?? new SettingsViewModel(Dispatcher);
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
            DataContext = Provider;
            UpdateThemeRadio();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        private void UpdateThemeRadio()
        {
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
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Reset":
                    SettingsHelper.LocalObject.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingsPage));
                    Frame.GoBack();
                    break;
                case "MyDevice":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel("https://m.coolapk.com/mp/do?c=userDevice&m=myDevice"));
                    break;
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "LogFolder":
                    _ = await Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "CleanCache":
                    Provider?.CleanCache();
                    break;
                case "CheckUpdate":
                    Provider?.CheckUpdate();
                    break;
                case "AccountSetting":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel("https://account.coolapk.com/account/settings"));
                    break;
                case "AccountLogout":
                    SettingsHelper.Logout();
                    Provider.IsLogin = false;
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

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "ViewCache":
                    _ = Frame.Navigate(typeof(CachesPage));
                    break;
                case "OpenLogFile":
                    StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists);
                    IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                    StorageFile file = files.FirstOrDefault();
                    if (file != null) { _ = Launcher.LaunchFileAsync(file); }
                    break;
                default:
                    break;
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Frame.OpenLinkAsync(e.Link);
    }
}
