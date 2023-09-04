using CoolapkLite.Common;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.ToolsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.SettingsPages;
using CoolapkLite.ViewModels.ToolsPages;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(TestViewModel),
                typeof(TestPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public TestViewModel Provider
        {
            get => (TestViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public TestPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Provider == null)
            {
                Provider = TestViewModel.Caches.TryGetValue(Dispatcher, out TestViewModel provider) ? provider : new TestViewModel(Dispatcher);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "OutPIP":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.Default); }
                    else if (ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "IsViewModeSupported")
                        && ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.Default))
                    { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default); }
                    break;
                case "CloseApp":
                    CoreApplication.Exit();
                    break;
                case "EnterPIP":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay); }
                    else if (ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "IsViewModeSupported")
                        && ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                    { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay); }
                    break;
                case "CustomUA":
                    UserAgentDialog userAgentDialog = new UserAgentDialog(Provider.UserAgent);
                    await userAgentDialog.ShowAsync();
                    Provider.UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    break;
                case "GCCollect":
                    GC.Collect();
                    break;
                case "NewWindow":
                    bool IsExtendsTitleBar = Provider.IsExtendsTitleBar;
                    await WindowHelper.CreateWindowAsync((window) =>
                    {
                        if (IsExtendsTitleBar)
                        {
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        }
                        Frame frame = new Frame();
                        window.Content = frame;
                        ThemeHelper.Initialize(window);
                        Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                        frame.Navigate(page, null, new DrillInNavigationTransitionInfo());
                    });
                    UIHelper.HideProgressBar(UIHelper.AppTitle);
                    break;
                case "NewAppWindow":
                    if (WindowHelper.IsAppWindowSupported)
                    {
                        (AppWindow window, Frame frame) = await WindowHelper.CreateWindowAsync();
                        if (Provider.IsExtendsTitleBar)
                        {
                            window.TitleBar.ExtendsContentIntoTitleBar = true;
                        }
                        ThemeHelper.Initialize(window);
                        Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                        frame.Navigate(page, null, new DrillInNavigationTransitionInfo());
                        await window.TryShowAsync();
                        Dispatcher.HideProgressBar();
                    }
                    break;
                case "CustomAPI":
                    APIVersionDialog _APIVersionDialog = new APIVersionDialog(Provider.UserAgent);
                    await _APIVersionDialog.ShowAsync();
                    Provider.UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
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
                case "RestartApp":
                    _ = CoreApplication.RequestRestartAsync(string.Empty);
                    break;
                case "ShowMessage":
                    this.ShowMessage(NotifyMessage.Text);
                    break;
                case "OpenBrowser":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel(WebUrl.Text, Dispatcher));
                    break;
                case "MinimizeApp":
                    if (ApiInformation.IsTypePresent("Windows.System.AppDiagnosticInfo"))
                    { _ = (await AppDiagnosticInfo.RequestInfoForAppAsync()).FirstOrDefault()?.GetResourceGroups().FirstOrDefault()?.StartSuspendAsync(); }
                    break;
                case "OutFullWindow":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen); }
                    else
                    { ApplicationView.GetForCurrentView().ExitFullScreenMode(); }
                    break;
                case "ShowAsyncError":
                    await Task.Run(() => throw new Exception(NotifyMessage.Text));
                    break;
                case "ShowProgressBar":
                    this.ShowProgressBar();
                    break;
                case "HideProgressBar":
                    this.HideProgressBar();
                    break;
                case "EnterFullWindow":
                    if (this.IsAppWindow())
                    { this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen); }
                    else
                    { _ = ApplicationView.GetForCurrentView().TryEnterFullScreenMode(); }
                    break;
                case "ErrorProgressBar":
                    this.ErrorProgressBar();
                    break;
                case "OpenCharmSearch":
                    if (SettingsPaneRegister.IsSearchPaneSupported)
                    { SearchPane.GetForCurrentView().Show(); }
                    break;
                case "GoToExtensionPage":
                    if (ExtensionManager.IsSupported)
                    { _ = Frame.Navigate(typeof(ExtensionPage)); }
                    break;
                case "OpenCharmSettings":
                    if (SettingsPaneRegister.IsSettingsPaneSupported)
                    { SettingsPane.Show(); }
                    break;
                case "PausedProgressBar":
                    this.PausedProgressBar();
                    break;
                case "ProgressRingState":
                    if (UIHelper.IsShowingProgressBar)
                    { this.HideProgressBar(); }
                    else
                    { this.ShowProgressBar(); }
                    break;
                case "GoToFansAnalyzePage":
                    _ = Frame.Navigate(typeof(FansAnalyzePage), new FansAnalyzeViewModel("1122745", Dispatcher));
                    break;
                default:
                    break;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
            lang = lang == LanguageHelper.AutoLanguageCode ? LanguageHelper.GetCurrentLanguage() : lang;
            CultureInfo culture = new CultureInfo(lang);
            ComboBox.SelectedItem = culture;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
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
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => this.ShowProgressBar(e.NewValue);
    }
}
