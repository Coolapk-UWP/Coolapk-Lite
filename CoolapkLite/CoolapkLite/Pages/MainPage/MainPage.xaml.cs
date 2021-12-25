using CoolapkLite.Helpers;
using CoolapkLite.Media;
using CoolapkLite.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.FeedPages;
using LiteDB;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IHaveTitleBar
    {
        public MainPage()
        {
            InitializeComponent();
            UIHelper.ShellDispatcher = Dispatcher;
            UIHelper.AppThemeChanged += CheckTheme;
            UIHelper.AppTitle = UIHelper.MainPage = this;
            AppTitle.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            Window.Current.SetTitleBar(CustomTitleBar);
            if (SettingsHelper.WindowsVersion >= 10586)
            {
                TitleBar.ExtendViewIntoTitleBar = true;
            }
            UpdateTitleBarLayout(TitleBar);
            UIHelper.ChangeTheme();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
            TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
            HamburgerMenuFrame.Navigated -= On_Navigated;
            UIHelper.AppThemeChanged -= CheckTheme;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            HideProgressBar();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack();
            if (HamburgerMenuFrame.SourcePageType != null)
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
                }
            }
        }

        private void HamburgerMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // You can also add items in code.
            HamburgerMenu.ItemsSource = MenuItem.GetMainItems();
            HamburgerMenu.OptionsItemsSource = MenuItem.GetOptionsItems();

            // Add handler for ContentFrame navigation.
            HamburgerMenuFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default, so load home page.
            HamburgerMenu.SelectedIndex = 0;
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            HamburgerMenu_Navigate((HamburgerMenu.ItemsSource as ObservableCollection<MenuItem>)[0], new EntranceNavigationTransitionInfo());

            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += System_BackPressed;
            }
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

        private void HamburgerMenu_Navigate(MenuItem MenuItem, NavigationTransitionInfo TransitionInfo, object[] vs = null)
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
            // entries in the backstack.
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

        private void HamburgerMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement SearchList = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "SearchList");
            if (SearchList != null)
            {
                SearchList.Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Collapsed : Visibility.Visible;
            }

            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "AutoSuggestBox");
            if (AutoSuggestBox != null)
            {
                AutoSuggestBox.Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
            }

            if (Window.Current.Bounds.Width >= 1008)
            {
                HamburgerMenu.IsPaneOpen = true;
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (Window.Current.Bounds.Width <= 640)
            {
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.Overlay;
            }
            else
            {
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement SearchList = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "SearchList");
            if (SearchList != null)
            {
                SearchList.Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
            }
            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "AutoSuggestBox");
            if (AutoSuggestBox != null)
            {
                AutoSuggestBox.Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void MainSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            FrameworkElement SearchList = VisualTree.FindDescendantByName(sender, "SearchList");
            if (SearchList != null && SearchList.Visibility == Visibility.Collapsed)
            {
                SearchList.Visibility = Visibility.Visible;
            }
            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName(sender, "AutoSuggestBox");
            if (AutoSuggestBox != null && AutoSuggestBox.Visibility == Visibility.Visible)
            {
                AutoSuggestBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchList_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            HamburgerMenu.IsPaneOpen = true;
            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "AutoSuggestBox");
            if (AutoSuggestBox != null)
            {
                AutoSuggestBox.Visibility = Visibility.Visible;
                (sender as FrameworkElement).Visibility = Visibility.Collapsed;
            }
        }

        private void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "SearchList":
                    (sender as FrameworkElement).Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case "AutoSuggestBox":
                    (sender as FrameworkElement).Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void CheckTheme(object sender, ElementTheme e) => CheckTheme();

        private void CheckTheme()
        {
            Background = UIHelper.ApplicationPageBackgroundThemeWindowBrush();
            HamburgerMenu.PaneBackground = UIHelper.ApplicationPageBackgroundThemeElementBrush();
        }

        private AppViewBackButtonVisibility TryGoBack()
        {
            if (!HamburgerMenuFrame.CanGoBack)
            { return AppViewBackButtonVisibility.Disabled; }

            HamburgerMenuFrame.GoBack();
            return AppViewBackButtonVisibility.Visible;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            Thickness TitleMargin = CustomTitleBar.Margin;
            CustomTitleBar.Height = TitleBar.Height;
            CustomTitleBar.Margin = new Thickness(SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible ? 48 : 0, TitleMargin.Top, TitleBar.SystemOverlayRightInset, TitleMargin.Bottom);
        }

        private void HamburgerListView_Loading(FrameworkElement sender, object args)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("ms-appx:///Controls/HamburgerMenu/HamburgerMenuTemplate.xaml");
            Style Style = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.RevealBrush") ? (Style)dict["HamburgerMenuItemRevealStyle"] : (Style)dict["HamburgerMenuItemStyle"];
            if (sender is ListView ListView)
            {
                ListView.ItemContainerStyle = Style;
            }
            else if (sender is ListViewItem ListViewItem)
            {
                ListViewItem.Style = Style;
            }
        }

        private void Page_Loading(FrameworkElement sender, object args) => CheckTheme();

        private void FrameworkElement_Loading(FrameworkElement sender, object args) => sender.Margin = new Thickness(0, UIHelper.HasStatusBar || UIHelper.HasTitleBar ? 0 : UIHelper.TitleBarHeight, 0, 0);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => CustomTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        #region 搜索框
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }
        #endregion

        #region 进度条
        public void ShowProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
        }

        public void ShowProgressBar(double value)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = value;
        }

        public void PausedProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = true;
        }

        public void ErrorProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowPaused = false;
            ProgressBar.ShowError = true;
        }

        public void HideProgressBar()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = 0;
        }

        public void ShowMessage(string message = null) => AppTitle.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
        #endregion
    }

    public class MenuItem : INotifyPropertyChanged
    {
        public Symbol Icon { get; set; }
        private ImageSource image;
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource Image
        {
            get => image;
            set
            {
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
        public int Index { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public object ViewModels { get; set; }

        private static readonly ResourceLoader loader = ResourceLoader.GetForCurrentView("MainPage");

        public static ObservableCollection<MenuItem> GetMainItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Home, Name = loader.GetString("Home"), PageType = typeof(IndexPage), ViewModels = new IndexViewModel("/main/indexV8"), Index = 0},
                new MenuItem() { Icon = Symbol.People, Name = loader.GetString("Circle"), PageType = typeof(CirclePage), Index = 1},
                new MenuItem() { Icon = Symbol.Favorite, Name = loader.GetString("Favorite"), PageType = typeof(FavoritePage),ViewModels = new FavoriteViewModel(), Index = 2 },
                new MenuItem() { Icon = Symbol.Calendar, Name = loader.GetString("History"), PageType = typeof(HistoryPage),ViewModels = new HistoryViewModel("浏览历史"), Index = 3},
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                 new MenuItem() { Icon = Symbol.Contact, Name = loader.GetString("User"), PageType = typeof(BrowserPage), ViewModels = new object[]{ true }, Index = 0 },
                 new MenuItem() { Icon = Symbol.Setting, Name = loader.GetString("Setting"), PageType = typeof(SettingsPage), Index = 1}
            };
            return items;
        }
    }

    public class DisplayModeToBool : IValueConverter
    {
        private static bool HasConnectedAnimation => ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation");
        private static bool HasConnectedAnimationConfiguration => ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation", "Configuration");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!HasConnectedAnimationConfiguration && value is SplitViewDisplayMode split && parameter is string mode)
            {
                return !(split.ToString() == mode) && HasConnectedAnimation;
            }
            return HasConnectedAnimation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
