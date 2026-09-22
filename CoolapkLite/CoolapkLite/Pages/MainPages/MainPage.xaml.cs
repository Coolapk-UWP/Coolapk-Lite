using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.NavigatePages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.NavigatePages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IHaveTitleBar
    {
        private bool isLoaded;

        public Frame MainFrame => HamburgerMenuFrame;

        public MainPage()
        {
            InitializeComponent();
            UIHelper.AppTitle = UIHelper.AppTitle ?? this;
            AppTitle.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? Package.Current.DisplayName;
            if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
            { UpdateTitleBarVisible(false); }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ApiInfoHelper.IsHardwareButtonsSupported)
            { HardwareButtons.BackPressed += System_BackPressed; }
            // Add handler for ContentFrame navigation.
            HamburgerMenuFrame.Navigated += On_Navigated;
            if (!isLoaded)
            {
                Deferral deferral = null;
                if (ApiInfoHelper.IsICommandLineActivatedEventArgsSupported && e.Parameter is ICommandLineActivatedEventArgs commandLineActivatedEventArgs)
                { deferral = commandLineActivatedEventArgs.Operation.GetDeferral(); }
                HamburgerMenu.ItemsSource = MenuItem.GetMainItems(Dispatcher);
                (MenuItem[] options, PersonMenuItem person) = MenuItem.GetOptionsItems(Dispatcher);
                HamburgerMenu.OptionsItemsSource = options;
                _ = person.InitializeAsync();
                _ = NotificationsModel.UpdateAsync();
                _ = LiveTileTask.UpdateTileAsync();
                if (e.Parameter is IActivatedEventArgs activatedEventArgs)
                { await OpenActivatedEventArgsAsync(activatedEventArgs); }
                else if (e.Parameter is OpenLinkFactory factory)
                { await OpenLinkAsync(factory); }
                else { HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(), new EntranceNavigationTransitionInfo()); }
                deferral?.Complete();
                isLoaded = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.IsAppWindow())
            {
                AppWindow window = this.GetWindowForElement();
                window.Frame.DragRegionVisuals.Clear();
                window.Changed -= AppWindow_Changed;
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
            HamburgerMenuFrame.Navigated -= On_Navigated;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsAppWindow())
            {
                AppWindow window = this.GetWindowForElement();
                window.Frame.DragRegionVisuals.Add(CustomTitleBar);
                window.Changed += AppWindow_Changed;
            }
            else
            {
                Window.Current.SetTitleBar(CustomTitleBar);
                SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
                UpdateTitleBarLayout(TitleBar);
                if (isLoaded)
                { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack(false); }
            }
        }

        private async Task OpenLinkAsync(OpenLinkFactory factory)
        {
            if (!await factory(HamburgerMenuFrame))
            {
                HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(), new EntranceNavigationTransitionInfo());
            }
        }

        private async Task OpenActivatedEventArgsAsync(IActivatedEventArgs args)
        {
            if (!await HamburgerMenuFrame.OpenActivatedEventArgsAsync(args))
            {
                HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(), new EntranceNavigationTransitionInfo());
            }
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            if (isLoaded && !this.IsAppWindow())
            { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack(); }
            if (HamburgerMenuFrame.SourcePageType != null)
            {
                if (e.Parameter is BrowserViewModel browserViewModel)
                {
                    HamburgerMenu.SelectedIndex = -1;
                    HamburgerMenu.SelectedOptionsIndex = browserViewModel.IsLoginPage == true ? 0 : -1;
                }
                else if (e.SourcePageType == typeof(SearchingPage))
                {
                    HamburgerMenu.SelectedIndex = HamburgerMenu.SelectedOptionsIndex = -1;
                }
                else
                {
                    MenuItem item = (HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(p => p.IsCurrentPage(e.SourcePageType));
                    if (item != null)
                    {
                        HamburgerMenu.SelectedOptionsIndex = -1;
                        HamburgerMenu.SelectedIndex = item.Index;
                    }
                    else
                    {
                        item = (HamburgerMenu.OptionsItemsSource as IEnumerable<MenuItem>).FirstOrDefault(p => p.IsCurrentPage(e.SourcePageType));
                        if (item != null)
                        {
                            HamburgerMenu.SelectedIndex = -1;
                            HamburgerMenu.SelectedOptionsIndex = item.Index;
                        }
                        else
                        {
                            HamburgerMenu.SelectedIndex = -1;
                            HamburgerMenu.SelectedOptionsIndex = -1;
                        }
                    }
                }
            }
            _ = UIHelper.HideProgressBarAsync(this as IHaveTitleBar);
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack() == AppViewBackButtonVisibility.Visible;
            }
        }

        private void System_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack() == AppViewBackButtonVisibility.Visible;
            }
        }

        private void HamburgerMenu_Navigate(MenuItem item, NavigationTransitionInfo transitionInfo, object vs = null)
        {
            if (!(item.PageType is Type _page))
            {
                return;
            }

            // Get the page type before navigation so you can prevent duplicate
            // entries in the back stack.
            Type preNavPageType = HamburgerMenuFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (preNavPageType != _page)
            {
                _ = HamburgerMenuFrame.Navigate(_page, vs ?? item.ViewModel, transitionInfo);
            }
        }

        private void HamburgerMenu_ItemInvoked(object sender, ItemClickEventArgs e)
        {
            MenuItem item = e.ClickedItem as MenuItem;
            HamburgerMenu_Navigate(item, null);
            if (HamburgerMenu.DisplayMode != SplitViewDisplayMode.CompactInline)
            {
                HamburgerMenu.IsPaneOpen = false;
            }
        }

        private AppViewBackButtonVisibility TryGoBack(bool goBack = true)
        {
            if (!Dispatcher.HasThreadAccess || !HamburgerMenuFrame.CanGoBack)
            { return AppViewBackButtonVisibility.Collapsed; }

            if (goBack) { HamburgerMenuFrame.GoBack(); }
            return AppViewBackButtonVisibility.Visible;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar titleBar)
        {
            CustomTitleBar.Opacity = titleBar.SystemOverlayLeftInset > 48 ? 0 : 1;
            LeftPaddingColumn.Width = new GridLength(titleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(titleBar.SystemOverlayRightInset);
        }

        private void UpdateTitleBarVisible(bool isVisible)
        {
            TopPaddingRow.Height = isVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? new GridLength(32) : new GridLength(0);
            CustomTitleBar.Visibility = isVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args) => UpdateTitleBarVisible(sender.TitleBar.IsVisible);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarVisible(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Tag)
            {
                case "Logout":
                    SettingsHelper.Logout();
                    break;
                case "CreateFeed":
                    new CreateFeedControl
                    {
                        FeedType = CreateFeedType.Feed,
                        PopupTransitions = new TransitionCollection
                        {
                            new PopupThemeTransition()
                        }
                    }.Show(this);
                    break;
                case "SwitchUser":
                    _ = HamburgerMenuFrame.Navigate(typeof(AccountsPage));
                    break;
                default:
                    break;
            }
        }

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
            switch (args.ChosenSuggestion)
            {
                case AppModel app:
                    _ = HamburgerMenuFrame.OpenLinkAsync(app.Url);
                    break;
                case SearchWord word:
                    _ = HamburgerMenuFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(word.ToString(), Dispatcher));
                    break;
                case null when !string.IsNullOrEmpty(sender.Text):
                    _ = HamburgerMenuFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text, Dispatcher));
                    break;
                default:
                    return;
            }

            if (HamburgerMenu.DisplayMode != SplitViewDisplayMode.CompactInline)
            {
                HamburgerMenu.IsPaneOpen = false;
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
    }

    public class MenuItem : DispatcherNotifyPropertyChanged
    {
        public int Index { get; set; }
        public Type PageType { get; set; }
        public Type[] OtherPageTypes { get; set; }
        public IViewModel ViewModel { get; set; }

        public string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string icon;
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        public MenuItem(CoreDispatcher dispatcher) : base(dispatcher) { }

        public bool IsCurrentPage(Type pageType) => PageType == pageType || (OtherPageTypes?.Any(p => p == pageType) == true);

        public static MenuItem[] GetMainItems(CoreDispatcher dispatcher)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");
            MenuItem[] items = new[]
            {
                new MenuItem(dispatcher) { Icon = "\uE80F", Name = loader.GetString("Home"), PageType = typeof(IndexPage), Index = 0 },
                new MenuItem(dispatcher) { Icon = "\uE716", Name = loader.GetString("Circle"), PageType = typeof(CirclePage), Index = 1 },
                new MenuItem(dispatcher) { Icon = "\uE734", Name = loader.GetString("Bookmark"), PageType = typeof(BookmarkPage), Index = 2 },
                new MenuItem(dispatcher) { Icon = "\uE787", Name = loader.GetString("History"), PageType = typeof(HistoryPage), Index = 3 }
            };
            return items;
        }

        public static (MenuItem[], PersonMenuItem) GetOptionsItems(CoreDispatcher dispatcher)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");
            PersonMenuItem person = new PersonMenuItem(dispatcher) { Icon = "\uE77B", Name = loader.GetString("Login"), Index = 0 };
            MenuItem[] items = new[]
            {
                 person,
                 new MenuItem(dispatcher) { Icon = "\uE713", Name = loader.GetString("Setting"), PageType = typeof(SettingsPage), OtherPageTypes = new[] { typeof(TestPage), typeof(CachesPage), typeof(ExtensionPage) }, Index = 1 }
            };
            return (items, person);
        }
    }

    public sealed class PersonMenuItem : MenuItem
    {
        private bool isLogin;
        public bool IsLogin
        {
            get => isLogin;
            private set => SetProperty(ref isLogin, value);
        }

        private ImageModel image;
        public ImageModel Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        private NotificationsModel _notificationsModel;
        public NotificationsModel NotificationsModel
        {
            get => _notificationsModel;
            private set => SetProperty(ref _notificationsModel, value);
        }

        public PersonMenuItem(CoreDispatcher dispatcher) : base(dispatcher)
        {
            PageType = typeof(BrowserPage);
            OtherPageTypes = new[] { typeof(ProfilePage), typeof(NotificationsPage), typeof(AccountsPage) };
            ViewModel = new BrowserViewModel(UriHelper.LoginUri, dispatcher);
        }

        ~PersonMenuItem() => SettingsHelper.LoginChanged -= OnLoginChanged;

        public async Task InitializeAsync()
        {
            try
            {
                await SetUserAvatarAsync().ConfigureAwait(false);
            }
            finally
            {
                SettingsHelper.LoginChanged += OnLoginChanged;
            }
        }

        private void OnLoginChanged(bool args) => _ = SetUserAvatarAsync(args);

        private Task SetUserAvatarAsync() =>
            SettingsHelper.CheckLoginAsync().ContinueWith(x => SetUserAvatarAsync(x.Result)).Unwrap();

        private async Task SetUserAvatarAsync(bool isLogin)
        {
            IsLogin = isLogin;
            if (isLogin)
            {
                string uid = SettingsHelper.Get<Account>(SettingsHelper.CurrentAccount).UID;
                if (!string.IsNullOrEmpty(uid))
                {
                    UserInfoModel results = await NetworkHelper.GetUserInfoByNameAsync(uid).ConfigureAwait(false);
                    if (results.UID.ToString() != uid) { return; }
                    Name = results.UserName;
                    PageType = typeof(ProfilePage);
                    OtherPageTypes = new[] { typeof(NotificationsPage), typeof(AccountsPage) };
                    await Dispatcher.ResumeForegroundAsync();
                    Image = results.UserAvatar;
                    ViewModel = null;
                    if (NotificationsModel == null)
                    {
                        NotificationsModel = NotificationsModel.TryGetCache(Dispatcher, out NotificationsModel model) ? model : new NotificationsModel(Dispatcher);
                    }
                }
            }
            else
            {
                Name = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Login");
                Image = null;
                PageType = typeof(BrowserPage);
                OtherPageTypes = new[] { typeof(ProfilePage), typeof(NotificationsPage), typeof(AccountsPage) };
                ViewModel = new BrowserViewModel(UriHelper.LoginUri, Dispatcher);
                NotificationsModel = null;
            }
        }
    }

    public sealed class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate PersonPicture { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) => item is PersonMenuItem ? PersonPicture : Default;

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
