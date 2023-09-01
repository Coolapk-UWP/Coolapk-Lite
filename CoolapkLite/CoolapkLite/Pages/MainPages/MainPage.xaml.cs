using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
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
            { UpdateTitleBarLayout(false); }
            _ = NotificationsModel.Update();
            _ = LiveTileTask.Instance?.UpdateTileAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
            // Add handler for ContentFrame navigation.
            HamburgerMenuFrame.Navigated += On_Navigated;
            if (!isLoaded)
            {
                HamburgerMenu.ItemsSource = MenuItem.GetMainItems(Dispatcher);
                HamburgerMenu.OptionsItemsSource = MenuItem.GetOptionsItems(Dispatcher);
                if (e.Parameter is IActivatedEventArgs ActivatedEventArgs)
                { OpenActivatedEventArgs(ActivatedEventArgs); }
                else { HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as ObservableCollection<MenuItem>)[0], new EntranceNavigationTransitionInfo()); }
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
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
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

        private async void OpenActivatedEventArgs(IActivatedEventArgs args)
        {
            if (!await HamburgerMenuFrame.OpenActivatedEventArgsAsync(args))
            {
                HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as ObservableCollection<MenuItem>)[0], new EntranceNavigationTransitionInfo());
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
                    MenuItem item = (HamburgerMenu.ItemsSource as ObservableCollection<MenuItem>).FirstOrDefault(p => p.PageType == e.SourcePageType);
                    if (item != default)
                    {
                        HamburgerMenu.SelectedOptionsIndex = -1;
                        HamburgerMenu.SelectedIndex = item.Index;
                    }
                    else
                    {
                        item = (HamburgerMenu.OptionsItemsSource as ObservableCollection<MenuItem>).FirstOrDefault(p => p.PageType == e.SourcePageType);
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
            UIHelper.HideProgressBar(this as IHaveTitleBar);
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
            CustomTitleBar.Height = TitleBar.Height;
            LeftPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayRightInset);
        }

        private void UpdateTitleBarLayout(bool IsVisible)
        {
            TopPaddingRow.Height = IsVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? new GridLength(32) : new GridLength(0);
            CustomTitleBar.Visibility = IsVisible && !UIHelper.HasStatusBar && !UIHelper.HasTitleBar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args) => UpdateTitleBarLayout(sender.TitleBar.IsVisible);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        #region 搜索框

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<Entity> observableCollection = new ObservableCollection<Entity>();
                sender.ItemsSource = observableCollection;
                string keyWord = sender.Text;
                await ThreadSwitcher.ResumeBackgroundAsync();
                await semaphoreSlim.WaitAsync();
                try
                {
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true);
                    if (isSucceed && result != null && result is JArray array && array.Count > 0)
                    {
                        foreach (JToken token in array)
                        {
                            switch (token.Value<string>("entityType"))
                            {
                                case "apk":
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new AppModel(token as JObject)));
                                    break;
                                case "searchWord":
                                default:
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new SearchWord(token as JObject)));
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppModel app)
            {
                _ = HamburgerMenuFrame.Navigate(typeof(BrowserPage), new BrowserViewModel($"https://www.coolapk.com{app.Url}", Dispatcher));
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

        public async void ShowProgressBar()
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
        }

        public async void ShowProgressBar(double value)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = value;
        }

        public async void PausedProgressBar()
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = true;
        }

        public async void ErrorProgressBar()
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowPaused = false;
            ProgressBar.ShowError = true;
        }

        public async void HideProgressBar()
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = 0;
        }

        public async void ShowMessage(string message = null)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }

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
            set
            {
                if (name != value)
                {
                    name = value;
                    RaisePropertyChangedEvent();
                    if (value == loader.GetString("User"))
                    {
                        SetUserAvatar(true);
                        SettingsHelper.LoginChanged += (_, e) => SetUserAvatar(e);
                    }
                }
            }
        }

        public string icon;
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        private ImageSource image;
        public ImageSource Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
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

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public MenuItem(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        private async void SetUserAvatar(bool isLogin)
        {
            if (isLogin && await SettingsHelper.CheckLoginAsync())
            {
                string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (!string.IsNullOrEmpty(UID))
                {
                    (string UID, string UserName, string UserAvatar) results = await NetworkHelper.GetUserInfoByNameAsync(UID);
                    if (results.UID != UID) { return; }
                    Name = results.UserName;
                    PageType = typeof(ProfilePage);
                    if (Dispatcher?.HasThreadAccess == false)
                    {
                        await Dispatcher.ResumeForegroundAsync();
                    }
                    Image = new BitmapImage(new Uri(results.UserAvatar));
                }
            }
            else
            {
                Name = loader.GetString("User");
                Image = null;
                PageType = typeof(BrowserPage);
                ViewModels = new BrowserViewModel(UriHelper.LoginUri, Dispatcher);
            }
        }

        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");

        public static ObservableCollection<MenuItem> GetMainItems(CoreDispatcher dispatcher)
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem(dispatcher) { Icon = "\uE80F", Name = loader.GetString("Home"), PageType = typeof(IndexPage), Index = 0 },
                new MenuItem(dispatcher) { Icon = "\uE716", Name = loader.GetString("Circle"), PageType = typeof(CirclePage), Index = 1 },
                new MenuItem(dispatcher) { Icon = "\uE734", Name = loader.GetString("Bookmark"), PageType = typeof(BookmarkPage), Index = 2 },
                new MenuItem(dispatcher) { Icon = "\uE787", Name = loader.GetString("History"), PageType = typeof(HistoryPage), Index = 3 }
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems(CoreDispatcher dispatcher)
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                 new MenuItem(dispatcher) { Icon = "\uE77B", Name = loader.GetString("User"), PageType = typeof(BrowserPage), ViewModels = new BrowserViewModel(UriHelper.LoginUri, dispatcher), Index = 0 },
                 new MenuItem(dispatcher) { Icon = "\uE713", Name = loader.GetString("Setting"), PageType = typeof(SettingsPage), Index = 1 }
            };
            return items;
        }
    }
}
