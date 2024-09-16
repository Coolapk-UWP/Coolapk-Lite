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
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Globalization;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.ApplicationSettings;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
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
                    { _ = this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.Default); }
                    else if (ApiInfoHelper.IsApplicationViewViewModeSupported
                        && ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.Default))
                    { _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default); }
                    break;
                case "OpenEdge":
                    _ = Launcher.LaunchUriAsync(new Uri(WebUrl.Text));
                    break;
                case "CloseApp":
                    CoreApplication.Exit();
                    break;
                case "EnterPIP":
                    if (this.IsAppWindow())
                    { _ = this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay); }
                    else if (ApiInfoHelper.IsApplicationViewViewModeSupported
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
                    _ = await WindowHelper.CreateWindowAsync(window =>
                    {
                        if (IsExtendsTitleBar)
                        {
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        }
                        Frame _frame = new Frame();
                        window.Content = _frame;
                        ThemeHelper.Initialize(window);
                        Type _page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                        _ = _frame.Navigate(_page, null, new DrillInNavigationTransitionInfo());
                    });
                    _ = UIHelper.HideProgressBarAsync(UIHelper.AppTitle);
                    break;
                case "ShowError":
                    throw new Exception(NotifyMessage.Text);
                case "CustomAPI":
                    APIVersionDialog versionDialog = new APIVersionDialog(Provider.UserAgent);
                    if (await versionDialog.ShowAsync() == ContentDialogResult.Primary)
                    {
                        Provider.APIVersion = 0;
                        Provider.UserAgent = NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString();
                    }
                    break;
                case "GetContent":
                    string text = WebUrl.Text;
                    await ThreadSwitcher.ResumeBackgroundAsync();
                    (bool isSucceed, string result) = text.TryGetUri(out Uri uri) ? await RequestHelper.GetStringAsync(uri, NetworkHelper.XMLHttpRequest, false).ConfigureAwait(true) : (true, "这不是一个链接");
                    result = isSucceed ? await result.ConvertJsonStringAsync().ConfigureAwait(false) : "网络错误";
                    await Dispatcher.ResumeForegroundAsync();
                    ContentDialog getJsonDialog = new ContentDialog
                    {
                        Title = text,
                        Content = new ScrollViewer
                        {
                            Content = new TextBlock
                            {
                                IsTextSelectionEnabled = true,
                                Text = result
                            },
                            VerticalScrollMode = ScrollMode.Enabled,
                            HorizontalScrollMode = ScrollMode.Enabled,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        },
                        CloseButtonText = "好的",
                        DefaultButton = ContentDialogButton.Close
                    };
                    _ = getJsonDialog.ShowAsync();
                    break;
                case "DeviceInfo":
                    _ = new DeviceInfoDialog().ShowAsync();
                    break;
                case "RestartApp" when ApiInfoHelper.IsRequestRestartAsyncSupported:
                    _ = CoreApplication.RequestRestartAsync(string.Empty);
                    break;
                case "ShowMessage":
                    _ = this.ShowMessageAsync(NotifyMessage.Text);
                    break;
                case "AddJumpList" when ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported():
                    JumpList list = await JumpList.LoadCurrentAsync();
                    if (!list.Items.Any(x => x.GroupName == "设置"))
                    {
                        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");
                        list.Items.Add(JumpListItem.CreateWithArguments("settings", loader.GetString("Setting")).AddGroupNameAndLogo("设置", new Uri("ms-appx:///Assets/Icons/Settings.png")));
                        list.Items.Add(JumpListItem.CreateWithArguments("caches", loader.GetString("Caches")).AddGroupNameAndLogo("设置", new Uri("ms-appx:///Assets/Icons/Package.png")));
                        list.Items.Add(JumpListItem.CreateWithArguments("flags", loader.GetString("Test")).AddGroupNameAndLogo("设置", new Uri("ms-appx:///Assets/Icons/DeveloperTools.png")));
                        if (ExtensionManager.IsOSSUploaderSupported)
                        {
                            list.Items.Add(JumpListItem.CreateWithArguments("extensions", loader.GetString("Extension")).AddGroupNameAndLogo("设置", new Uri("ms-appx:///Assets/Icons/AppIconDefault.png")));
                        }
                    }
                    _ = list.SaveAsync();
                    break;
                case "OpenBrowser":
                    _ = Frame.Navigate(typeof(BrowserPage), new BrowserViewModel(WebUrl.Text, Dispatcher));
                    break;
                case "MinimizeApp" when ApiInfoHelper.IsRequestInfoForAppAsyncSupported:
                    _ = (await AppDiagnosticInfo.RequestInfoForAppAsync()).FirstOrDefault()?.GetResourceGroups().FirstOrDefault()?.StartSuspendAsync();
                    break;
                case "NewAppWindow" when WindowHelper.IsAppWindowSupported:
                    (AppWindow appWindow, Frame frame) = await WindowHelper.CreateWindowAsync();
                    if (Provider.IsExtendsTitleBar)
                    {
                        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                    }
                    ThemeHelper.Initialize(appWindow);
                    Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                    _ = frame.Navigate(page, null, new DrillInNavigationTransitionInfo());
                    await appWindow.TryShowAsync();
                    _ = Dispatcher.HideProgressBarAsync();
                    break;
                case "OutFullWindow":
                    if (this.IsAppWindow())
                    { _ = this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.Default); }
                    else
                    { ApplicationView.GetForCurrentView().ExitFullScreenMode(); }
                    break;
                case "CleanJumpList" when ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported():
                    list = await JumpList.LoadCurrentAsync();
                    list.Items.Clear();
                    _ = list.SaveAsync();
                    break;
                case "ShowAsyncError":
                    _ = ThreadPool.RunAsync(_ => throw new Exception(NotifyMessage.Text));
                    break;
                case "ShowProgressBar":
                    _ = this.ShowProgressBarAsync();
                    break;
                case "HideProgressBar":
                    _ = this.HideProgressBarAsync();
                    break;
                case "EnterFullWindow":
                    _ = this.IsAppWindow()
                        ? this.GetWindowForElement().Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen)
                        : ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    break;
                case "ErrorProgressBar":
                    _ = this.ErrorProgressBarAsync();
                    break;
                case "OpenCharmSearch" when SettingsPaneRegister.IsSearchPaneSupported:
                    SearchPane.GetForCurrentView().Show();
                    break;
                case "GoToExtensionPage" when ExtensionManager.IsOSSUploaderSupported:
                    _ = Frame.Navigate(typeof(ExtensionPage));
                    break;
                case "OpenCharmSettings" when SettingsPaneRegister.IsSettingsPaneSupported:
                    SettingsPane.Show();
                    break;
                case "PausedProgressBar":
                    _ = this.PausedProgressBarAsync();
                    break;
                case "ProgressRingState":
                    _ = UIHelper.IsShowingProgressBar ? this.HideProgressBarAsync() : this.ShowProgressBarAsync();
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

        private void Rectangle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (!(sender is FrameworkElement frameworkElement)) { return; }
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(frameworkElement.Tag?.ToString());
            Clipboard.SetContent(dataPackage);
            if (e != null) { e.Handled = true; }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => _ = this.ShowProgressBarAsync(e.NewValue);
    }
}
