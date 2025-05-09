using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PivotPage : Page, IHaveTitleBar
    {
        private bool isLoaded;
        private Action Refresh;

        public Frame MainFrame => PivotContentFrame;
        private static bool IsLogin => !string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid));

        public PivotPage()
        {
            InitializeComponent();
            _ = PivotContentFrame.Navigate(typeof(Page));
            UIHelper.AppTitle = UIHelper.AppTitle ?? this;
            if (ApiInfoHelper.IsUniversalApiContract14Present)
            { CommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right; }
            AppTitle.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? Package.Current.DisplayName;
            if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
            { UpdateTitleBarVisible(false); }
            _ = NotificationsModel.UpdateAsync();
            _ = LiveTileTask.Instance?.UpdateTileAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ApiInfoHelper.IsHardwareButtonsSupported)
            { HardwareButtons.BackPressed += System_BackPressed; }
            // Add handler for ContentFrame navigation.
            PivotContentFrame.Navigated += On_Navigated;
            if (!isLoaded)
            {
                Deferral deferral = null;
                if (ApiInfoHelper.IsICommandLineActivatedEventArgsSupported && e.Parameter is ICommandLineActivatedEventArgs CommandLineActivatedEventArgs)
                { deferral = CommandLineActivatedEventArgs.Operation.GetDeferral(); }
                Pivot.ItemsSource = GetMainItems();
                if (e.Parameter is IActivatedEventArgs ActivatedEventArgs)
                { await OpenActivatedEventArgsAsync(ActivatedEventArgs); }
                else if (e.Parameter is OpenLinkFactory factory)
                { await OpenLinkAsync(factory); }
                deferral?.Complete();
                isLoaded = true;
            }
            SettingsHelper.LoginChanged += OnLoginChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Changed -= AppWindow_Changed;
            }
            else
            {
                Window.Current.SetTitleBar(null);
                SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
            }
            if (ApiInfoHelper.IsHardwareButtonsSupported)
            { HardwareButtons.BackPressed -= System_BackPressed; }
            PivotContentFrame.Navigated -= On_Navigated;
            SettingsHelper.LoginChanged -= OnLoginChanged;
        }

        private async void OnLoginChanged(bool isLogin)
        {
            await Dispatcher.ResumeForegroundAsync();
            Pivot.ItemsSource = GetMainItems(isLogin);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Changed += AppWindow_Changed;
            }
            else
            {
                Window.Current.SetTitleBar(CustomTitleBar);
                SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
                UpdateTitleBarLayout(TitleBar);
            }
        }

        private Task OpenLinkAsync(OpenLinkFactory factory) => factory(PivotContentFrame);

        private Task OpenActivatedEventArgsAsync(IActivatedEventArgs args) => PivotContentFrame.OpenActivatedEventArgsAsync(args);

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            if (PivotContentFrame.Visibility == Visibility.Collapsed)
            {
                Pivot.Visibility = Visibility.Collapsed;
                PivotContentFrame.Visibility = Visibility.Visible;
            }
            if (!this.IsAppWindow())
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = PivotContentFrame.BackStackDepth == 0 ? AppViewBackButtonVisibility.Collapsed : AppViewBackButtonVisibility.Visible;
            }
            _ = UIHelper.HideProgressBarAsync(this as IHaveTitleBar);
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void System_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                    MenuItem.Tag.ToString() == "indexV8"
                        ? "/main/indexV8"
                        : MenuItem.Tag.ToString().Contains('V')
                            ? $"/page?url={MenuItem.Tag}"
                            : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}", Dispatcher));
                Refresh = () => (Frame.Content as AdaptivePage).Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame frame && frame.Content is AdaptivePage AdaptivePage)
            {
                Refresh = () => AdaptivePage.Refresh(true);
            }
        }

        private bool TryGoBack()
        {
            if (!Dispatcher.HasThreadAccess || !PivotContentFrame.CanGoBack)
            { return false; }

            if (PivotContentFrame.BackStackDepth > 1)
            {
                PivotContentFrame.GoBack();
            }
            else
            {
                PivotContentFrame.GoBack();
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(new DrillOutThemeAnimation
                {
                    EntranceTarget = Pivot,
                    ExitTarget = PivotContentFrame,
                    FillBehavior = FillBehavior.Stop
                });
                storyboard.Begin();
                Pivot.Visibility = Visibility.Visible;
                PivotContentFrame.Visibility = Visibility.Collapsed;
            }

            return true;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            CustomTitleBar.Opacity = TitleBar.SystemOverlayLeftInset > 48 ? 0 : 1;
            LeftPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayRightInset);
        }

        private void UpdateTitleBarVisible(bool IsVisible)
        {
            TopPaddingRow.Height = IsVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? new GridLength(32) : new GridLength(0);
            CustomTitleBar.Visibility = IsVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (Refresh != null)
            {
                Refresh();
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is ListView ListView && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag?.ToString())
            {
                case "User":
                    _ = await SettingsHelper.CheckLoginAsync()
                        ? PivotContentFrame.Navigate(typeof(ProfilePage))
                        : PivotContentFrame.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri, Dispatcher));
                    break;
                case "History":
                    _ = PivotContentFrame.Navigate(typeof(HistoryPage));
                    break;
                case "Setting":
                    _ = PivotContentFrame.Navigate(typeof(SettingsPage));
                    break;
                case "Bookmark":
                    _ = PivotContentFrame.Navigate(typeof(BookmarkPage));
                    break;
                case "SearchButton":
                    _ = PivotContentFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(string.Empty, Dispatcher));
                    break;
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args) => UpdateTitleBarVisible(sender.TitleBar.IsVisible);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarVisible(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        #region 搜索框

        private int count = -1;

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string keyWord = sender.Text;
                await ThreadSwitcher.ResumeBackgroundAsync();
                try
                {
                    count++;
                    await Task.Delay(500).ConfigureAwait(false);
                    if (count != 0) { return; }
                    ObservableCollection<Entity> observableCollection = new ObservableCollection<Entity>();
                    await sender.SetValueAsync(ItemsControl.ItemsSourceProperty, observableCollection);
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true).ConfigureAwait(false);
                    if (isSucceed && result != null && result is JArray array && array.Count > 0)
                    {
                        foreach (JObject token in array.OfType<JObject>())
                        {
                            switch (token.Value<string>("entityType"))
                            {
                                case "apk":
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new AppModel(token)));
                                    break;
                                case "searchWord":
                                default:
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new SearchWord(token)));
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    count--;
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppModel app)
            {
                _ = PivotContentFrame.OpenLinkAsync(app.Url);
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                _ = PivotContentFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(word.ToString(), Dispatcher));
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                _ = PivotContentFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text, Dispatcher));
            }
        }

        #endregion

        #region 进度条

        public async Task ShowProgressBarAsync()
        {
            await Dispatcher.ResumeForegroundAsync();
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
        }

        public async Task ShowProgressBarAsync(double value)
        {
            await Dispatcher.ResumeForegroundAsync();
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = value;
        }

        public async Task PausedProgressBarAsync()
        {
            await Dispatcher.ResumeForegroundAsync();
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = true;
        }

        public async Task ErrorProgressBarAsync()
        {
            await Dispatcher.ResumeForegroundAsync();
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowPaused = false;
            ProgressBar.ShowError = true;
        }

        public async Task HideProgressBarAsync()
        {
            await Dispatcher.ResumeForegroundAsync();
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = 0;
        }

        public async Task ShowMessageAsync(string message = null)
        {
            await Dispatcher.ResumeForegroundAsync();

            AppTitle.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? Package.Current.DisplayName;

            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Title = message ?? string.Empty;
            }
            else
            {
                ApplicationView.GetForCurrentView().Title = message ?? string.Empty;
            }
        }

        #endregion

        public static PivotItem[] GetMainItems() => GetMainItems(IsLogin);

        public static PivotItem[] GetMainItems(bool isLogin)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("CirclePage");
            PivotItem[] items = isLogin ? new[]
            {
                new PivotItem { Tag = "indexV8", Header = loader.GetString("indexV8"), Content = new Frame() },
                new PivotItem { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            } : new[]
            {
                new PivotItem { Tag = "indexV8", Header = loader.GetString("indexV8"), Content = new Frame() },
                new PivotItem { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() }
            };
            return items;
        }
    }
}
