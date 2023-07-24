using CoolapkLite.Common;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        internal List<CultureInfo> SupportCultures => LanguageHelper.SupportCultures;

        internal string FrameworkDescription => RuntimeInformation.FrameworkDescription;

        internal string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        internal string OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion.ToString();

        internal string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();

        internal bool IsExtendsTitleBar
        {
            get => CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            set
            {
                if (IsExtendsTitleBar != value)
                {
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = value;
                    ThemeHelper.UpdateSystemCaptionButtonColors();
                }
            }
        }

        internal bool IsUseAPI2
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseAPI2);
            set => SettingsHelper.Set(SettingsHelper.IsUseAPI2, value);
        }

        internal bool IsCustomUA
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            set
            {
                if (IsCustomUA != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsCustomUA, value);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                }
            }
        }

        internal int APIVersion
        {
            get => (int)SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion) - 4;
            set
            {
                if (APIVersion != value)
                {
                    SettingsHelper.Set(SettingsHelper.APIVersion, value + 4);
                    NetworkHelper.SetRequestHeaders();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                }
            }
        }

        internal bool IsUseTokenV2
        {
            get => SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion) == TokenVersions.TokenV2;
            set
            {
                if (IsUseTokenV2 != value)
                {
                    SettingsHelper.Set(SettingsHelper.TokenVersion, (int)(value ? TokenVersions.TokenV2 : TokenVersions.TokenV1));
                    NetworkHelper.SetRequestHeaders();
                }
            }
        }

        internal bool IsUseLiteHome
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome);
            set => SettingsHelper.Set(SettingsHelper.IsUseLiteHome, value);
        }

        internal bool IsUseBlurBrush
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseBlurBrush);
            set => SettingsHelper.Set(SettingsHelper.IsUseBlurBrush, value);
        }

        internal bool IsUseCompositor
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor);
            set => SettingsHelper.Set(SettingsHelper.IsUseCompositor, value);
        }

        internal double SemaphoreSlimCount
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
                }
            }
        }

        private string userAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
        internal string UserAgent
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

        public TestPage()
        {
            InitializeComponent();
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Test");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "OutPIP":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.Default); }
                    else if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.Default))
                    { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default); }
                    break;
                case "EnterPIP":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay); }
                    else if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                    { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay); }
                    break;
                case "CustomUA":
                    UserAgentDialog userAgentDialog = new UserAgentDialog(UserAgent);
                    await userAgentDialog.ShowAsync();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    break;
                case "NewWindow":
                    if (WindowHelper.IsSupported)
                    {
                        Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                        (AppWindow window, Frame frame) = await WindowHelper.CreateWindow();
                        window.TitleBar.ExtendsContentIntoTitleBar = true;
                        ThemeHelper.Initialize();
                        frame.Navigate(page);
                        await window.TryShowAsync();
                    }
                    break;
                case "CustomAPI":
                    APIVersionDialog _APIVersionDialog = new APIVersionDialog(UserAgent);
                    await _APIVersionDialog.ShowAsync();
                    UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    break;
                case "OpenEdge":
                    _ = Launcher.LaunchUriAsync(new Uri(WebUrl.Text));
                    break;
                case "ShowError":
                    throw new Exception(NotifyMessage.Text);
                case "GetContent":
                    Uri uri = WebUrl.Text.TryGetUri();
                    (bool isSucceed, string result) = uri == null ? (true, "这不是一个链接") : await RequestHelper.GetStringAsync(uri, "XMLHttpRequest", false);
                    if (!isSucceed)
                    {
                        result = "网络错误";
                    }
                    ContentDialog GetJsonDialog = new ContentDialog
                    {
                        Title = WebUrl.Text,
                        Content = new ScrollViewer
                        {
                            Content = new TextBlock
                            {
                                IsTextSelectionEnabled = true,
                                Text = result.ConvertJsonString()
                            },
                            VerticalScrollMode = ScrollMode.Enabled,
                            HorizontalScrollMode = ScrollMode.Enabled,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        },
                        CloseButtonText = "好的",
                        DefaultButton = ContentDialogButton.Close
                    };
                    _ = await GetJsonDialog.ShowAsync();
                    break;
                case "ShowMessage":
                    UIHelper.ShowMessage(NotifyMessage.Text);
                    break;
                case "OpenBrowser":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel(WebUrl.Text));
                    break;
                case "ShowAsyncError":
                    await Task.Run(() => throw new Exception(NotifyMessage.Text));
                    break;
                case "ShowProgressBar":
                    UIHelper.ShowProgressBar();
                    break;
                case "HideProgressBar":
                    UIHelper.HideProgressBar();
                    break;
                case "GoToExtensionPage":
                    if (ExtensionManager.IsSupported)
                    {
                        _ = Frame.Navigate(typeof(ExtensionPage));
                    }
                    break;
                case "ErrorProgressBar":
                    UIHelper.ErrorProgressBar();
                    break;
                case "OpenCharmSearch":
                    if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane"))
                    { SearchPane.GetForCurrentView().Show(); }
                    break;
                case "OpenCharmSettings":
                    if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
                    { SettingsPane.Show(); }
                    break;
                case "PausedProgressBar":
                    UIHelper.PausedProgressBar();
                    break;
                case "PrograssRingState":
                    if (UIHelper.IsShowingProgressBar)
                    {
                        UIHelper.HideProgressBar();
                    }
                    else
                    {
                        UIHelper.ShowProgressBar();
                    }
                    break;
                default:
                    break;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            switch (ComboBox.Tag.ToString())
            {
                case "Language":
                    string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
                    lang = lang == LanguageHelper.AutoLanguageCode ? LanguageHelper.GetCurrentLanguage() : lang;
                    CultureInfo culture = new CultureInfo(lang);
                    ComboBox.SelectedItem = culture;
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            switch (ComboBox.Tag.ToString())
            {
                case "Language":
                    CultureInfo culture = ComboBox.SelectedItem as CultureInfo;
                    if (culture.Name != LanguageHelper.GetCurrentLanguage())
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, culture.Name);
                    }
                    else
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, LanguageHelper.AutoLanguageCode);
                    }
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => UIHelper.ShowProgressBar(e.NewValue);
    }
}
