using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Users;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IHaveTitleBar
    {
        private bool isLoaded;
        private static bool firstLoad = true;

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
                HamburgerMenu.ItemsSource = MenuItem.GetMainItems(Dispatcher);
                (MenuItem[] options, PersonMenuItem person) = MenuItem.GetOptionsItems(Dispatcher);
                HamburgerMenu.OptionsItemsSource = options;
                if (firstLoad) { await WaitFirstRequestAsync(); }
                _ = person.InitializeAsync();
                _ = NotificationsModel.UpdateAsync();
                _ = LiveTileTask.Instance?.UpdateTileAsync();
                if (e.Parameter is IActivatedEventArgs ActivatedEventArgs)
                { OpenActivatedEventArgs(ActivatedEventArgs); }
                else if (e.Parameter is OpenLinkFactory factory)
                { OpenLinkAsync(factory); }
                else { HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(), new EntranceNavigationTransitionInfo()); }
                isLoaded = true;
            }
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
            HamburgerMenuFrame.Navigated -= On_Navigated;
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
                if (isLoaded)
                { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack(false); }
            }
        }

        private async Task WaitFirstRequestAsync()
        {
            if (!mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            { return; }
            try
            {
                _ = UIHelper.ShowProgressBarAsync((IHaveTitleBar)this);
                await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetInitPage, DateTimeOffset.UtcNow.ToUnixTimeSeconds()), true);
            }
            finally
            {
                firstLoad = false;
                _ = UIHelper.HideProgressBarAsync((IHaveTitleBar)this);
            }
        }

        private async void OpenLinkAsync(OpenLinkFactory factory)
        {
            if (!await factory(HamburgerMenuFrame))
            {
                HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(), new EntranceNavigationTransitionInfo());
            }
        }

        private async void OpenActivatedEventArgs(IActivatedEventArgs args)
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
                else
                {
                    MenuItem item = (HamburgerMenu.ItemsSource as IEnumerable<MenuItem>).FirstOrDefault(p => p.PageType == e.SourcePageType);
                    if (item != default)
                    {
                        HamburgerMenu.SelectedOptionsIndex = -1;
                        HamburgerMenu.SelectedIndex = item.Index;
                    }
                    else
                    {
                        item = (HamburgerMenu.OptionsItemsSource as IEnumerable<MenuItem>).FirstOrDefault(p => p.PageType == e.SourcePageType);
                        if (item != default)
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

        private void HamburgerMenu_Navigate(MenuItem MenuItem, NavigationTransitionInfo TransitionInfo, object vs = null)
        {
            Type _page;
            if (MenuItem.PageType != null)
            {
                _page = MenuItem.PageType;
            }
            else
            {
                return;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the back stack.
            Type PreNavPageType = HamburgerMenuFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Equals(PreNavPageType, _page))
            {
                _ = HamburgerMenuFrame.Navigate(_page, vs ?? MenuItem.ViewModels, TransitionInfo);
            }
        }

        private void HamburgerMenu_ItemInvoked(object sender, ItemClickEventArgs e)
        {
            MenuItem MenuItem = e.ClickedItem as MenuItem;
            HamburgerMenu_Navigate(MenuItem, null);
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
                _ = HamburgerMenuFrame.OpenLinkAsync(app.Url);
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                _ = HamburgerMenuFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(word.ToString(), Dispatcher));
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                _ = HamburgerMenuFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text, Dispatcher));
            }
            else
            {
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

    public class MenuItem : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public Type PageType { get; set; }
        public IViewModel ViewModels { get; set; }

        public CoreDispatcher Dispatcher { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public MenuItem(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        protected static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");

        public static MenuItem[] GetMainItems(CoreDispatcher dispatcher)
        {
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
            PersonMenuItem person = new PersonMenuItem(dispatcher) { Icon = "\uE77B", Name = loader.GetString("Login"), PageType = typeof(BrowserPage), ViewModels = new BrowserViewModel(UriHelper.LoginUri, dispatcher), Index = 0 };
            MenuItem[] items = new[]
            {
                 person,
                 new MenuItem(dispatcher) { Icon = "\uE713", Name = loader.GetString("Setting"), PageType = typeof(SettingsPage), Index = 1 }
            };
            return (items, person);
        }
    }

    public class PersonMenuItem : MenuItem
    {
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

        public PersonMenuItem(CoreDispatcher dispatcher) : base(dispatcher) { }

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
            if (isLogin)
            {
                string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (!string.IsNullOrEmpty(UID))
                {
                    UserInfoModel results = await NetworkHelper.GetUserInfoByNameAsync(UID).ConfigureAwait(false);
                    if (results.UID.ToString() != UID) { return; }
                    Name = results.UserName;
                    PageType = typeof(ProfilePage);
                    await Dispatcher.ResumeForegroundAsync();
                    Image = results.UserAvatar;
                    ViewModels = null;
                    if (NotificationsModel == null)
                    {
                        NotificationsModel = NotificationsModel.Caches.TryGetValue(Dispatcher, out NotificationsModel model) ? model : new NotificationsModel(Dispatcher);
                    }
                }
            }
            else
            {
                Name = loader.GetString("Login");
                Image = null;
                PageType = typeof(BrowserPage);
                ViewModels = new BrowserViewModel(UriHelper.LoginUri, Dispatcher);
                NotificationsModel = null;
            }
        }
    }

    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate PersonPicture { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) => item is PersonMenuItem ? PersonPicture : Default;

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
