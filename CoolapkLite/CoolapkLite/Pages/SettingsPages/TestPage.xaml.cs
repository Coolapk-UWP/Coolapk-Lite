using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.ViewModels.BrowserPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
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

        internal int APIVersion
        {
            get => (int)SettingsHelper.Get<APIVersion>(SettingsHelper.APIVersion) - 5;
            set
            {
                if (APIVersion != value)
                {
                    SettingsHelper.Set(SettingsHelper.APIVersion, value + 5);
                    NetworkHelper.SetRequestHeaders();
                }
            }
        }

        internal bool IsUseTokenV2
        {
            get => SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion) == TokenVersion.TokenV2;
            set
            {
                if (IsUseTokenV2 != value)
                {
                    SettingsHelper.Set(SettingsHelper.TokenVersion, (int)(value ? TokenVersion.TokenV2 : TokenVersion.TokenV1));
                    NetworkHelper.SetRequestHeaders();
                }
            }
        }

        internal bool IsUseLiteHome
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome);
            set => SettingsHelper.Set(SettingsHelper.IsUseLiteHome, value);
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public TestPage()
        {
            InitializeComponent();
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Test");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "OutPIP":
                    _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                    break;
                case "EnterPIP":
                    if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay)) { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay); }
                    break;
                case "OpenEdge":
                    _ = Launcher.LaunchUriAsync(new Uri(WebUrl.Text));
                    break;
                case "ShowError":
                    throw new Exception(NotifyMessage.Text);
                case "GetContent":
                    Uri uri = WebUrl.Text.ValidateAndGetUri();
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
                case "GoToTestingPage":
                    _ = Frame.Navigate(typeof(BlankPage));
                    break;
                case "ErrorProgressBar":
                    UIHelper.ErrorProgressBar();
                    break;
                case "OpenCharmSettings":
                    if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
                    {
                        SettingsPane.Show();
                    }
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
                //case "GoToFansAnalyzePage":
                //    _ = Frame.Navigate(typeof(FansAnalyzePage), new FansAnalyzeViewModel("536381"));
                //    break;
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
