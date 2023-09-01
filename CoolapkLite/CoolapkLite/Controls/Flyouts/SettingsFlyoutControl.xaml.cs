﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class SettingsFlyoutControl : SettingsFlyout
    {
        private Action<UISettingChangedType> UISettingChanged;

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(SettingsViewModel),
                typeof(SettingsFlyoutControl),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="SettingsFlyout"/>.
        /// </summary>
        public SettingsViewModel Provider
        {
            get => (SettingsViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public SettingsFlyoutControl()
        {
            InitializeComponent();
            ResourceDictionary ThemeResources = new ResourceDictionary { Source = new Uri("ms-appx:///Styles/SettingsFlyout.xaml") };
            Style = (Style)ThemeResources["DefaultSettingsFlyoutStyle"];
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            UISettingChanged = (mode) =>
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
                UpdateThemeRadio();
            };
            Provider = Provider ?? (SettingsViewModel.Caches.TryGetValue(Dispatcher, out SettingsViewModel provider) ? provider : new SettingsViewModel(Dispatcher));
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
            UpdateThemeRadio();
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        private async void UpdateThemeRadio()
        {
            ElementTheme theme = await ThemeHelper.GetActualThemeAsync();

            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }

            switch (theme)
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
                    break;
                case "MyDevice":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel("https://m.coolapk.com/mp/do?c=userDevice&m=myDevice", Dispatcher));
                    break;
                case "LogFolder":
                    _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "CleanCache":
                    _ = (Provider?.CleanCacheAsync());
                    break;
                case "CheckUpdate":
                    _ = (Provider?.CheckUpdateAsync());
                    break;
                case "AccountSetting":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel("https://account.coolapk.com/account/settings", Dispatcher));
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

        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
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
                    _ = this.NavigateAsync(typeof(CachesPage));
                    break;
                case "OpenCache":
                    _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists));
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

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => _ = this.OpenLinkAsync(e.Link);
    }
}
