using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private MarkdownTextBlock Markdown;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;
        private Thickness ScrollViewerMargin => UIHelper.ScrollViewerMargin;
        private Thickness ScrollViewerPadding => UIHelper.ScrollViewerPadding;

        private const string IssuePath = "https://github.com/Coolapk-UWP/Coolapk-Lite/issues";

        internal bool IsNoPicsMode
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
            set
            {
                if (IsNoPicsMode != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsNoPicsMode, value);
                    RaisePropertyChangedEvent();
                    SettingsHelper.UISettingChanged?.Invoke(UISettingChangedType.NoPicChanged);
                }
            }
        }

        internal bool IsDarkMode
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode);
            set
            {
                if (IsDarkMode != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsDarkMode, value);
                    UIHelper.ChangeTheme();
                    RaisePropertyChangedEvent();
                }
            }
        }

        internal bool IsBackgroundColorFollowSystem
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem);
            set
            {
                if (IsBackgroundColorFollowSystem != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsBackgroundColorFollowSystem, value);
                    RaisePropertyChangedEvent();
                    IsDarkMode = SettingsHelper.UISettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).Equals(Colors.Black);
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
            if (IsBackgroundColorFollowSystem)
            {
                Default.IsChecked = true;
            }
            else if (IsDarkMode)
            {
                Dark.IsChecked = true;
            }
            else
            {
                Light.IsChecked = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (Markdown != null)
            {
                Markdown.LinkClicked -= MarkdownTextBlock_LinkClicked;
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
                case "TestPage":
                    _ = Frame.Navigate(typeof(TestPage));
                    break;
                case "FeedBack":
                    UIHelper.OpenLinkAsync(IssuePath);
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
                    //await CheckUpdate.CheckUpdateAsync(true, false);
                    IsCheckUpdateButtonEnabled = true;
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
                    IsBackgroundColorFollowSystem = false;
                    IsDarkMode = true;
                    break;
                case "Light":
                    IsBackgroundColorFollowSystem = false;
                    IsDarkMode = false;
                    break;
                case "Default":
                    IsBackgroundColorFollowSystem = true;
                    SettingsHelper.UISettingChanged?.Invoke(IsDarkMode ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
                    break;
                default:
                    break;
            }
        }

        private void Readme_Loaded(object sender, RoutedEventArgs e)
        {
            if (UIHelper.IsTypePresent("Microsoft.Toolkit.Uwp.UI", "Media.BackdropBlurBrush"))
            {
                Markdown = new MarkdownTextBlock()
                {
                    FontSize = GoToTestPage.FontSize,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Colors.Transparent),
                    Text = ResourceLoader.GetForViewIndependentUse("MarkDown").GetString("about")
                };
                Markdown.LinkClicked += MarkdownTextBlock_LinkClicked;
                (sender as Grid).Children.Add(Markdown);
            }
            else
            {
                (sender as Grid).Children.Add(new HyperlinkButton()
                {
                    Content = "查看更多",
                    NavigateUri = new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite/blob/master/About.md")
                });
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
