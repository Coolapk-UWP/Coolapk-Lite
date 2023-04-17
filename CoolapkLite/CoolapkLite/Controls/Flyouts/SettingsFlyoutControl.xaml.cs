﻿using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class SettingsFlyoutControl : SettingsFlyout
    {
        internal SettingsViewModel Provider;

        public SettingsFlyoutControl()
        {
            InitializeComponent();
            Provider = SettingsViewModel.Caches ?? new SettingsViewModel();
            ThemeHelper.UISettingChanged.Add(mode =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                        RequestedTheme = ElementTheme.Light;
                        break;
                    case UISettingChangedType.DarkMode:
                        RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        break;
                }
            });
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
            DataContext = Provider;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Reset":
                    SettingsHelper.LocalObject.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    break;
                case "MyDevice":
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel("https://m.coolapk.com/mp/do?c=userDevice&m=myDevice"));
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
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel("https://account.coolapk.com/account/settings"));
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

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => _ = UIHelper.OpenLinkAsync(e.Link);
    }
}
