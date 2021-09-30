using CoolapkLite.Helpers;
using CoolapkLite.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            }
            UIHelper.CheckTheme();
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
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
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
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
            if (!(_page is null) && !Equals(PreNavPageType, _page))
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
            switch((sender as FrameworkElement).Name)
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

        private void FrameworkElement_Loading(FrameworkElement sender, object args)
        {
            sender.Margin = new Thickness(0, UIHelper.HasStatusBar ? 0 : 32, 0, 0);
        }

        private AppViewBackButtonVisibility TryGoBack()
        {
            if (!HamburgerMenuFrame.CanGoBack)
            { return AppViewBackButtonVisibility.Disabled; }

            // Don't go back if the nav pane is overlayed.
            if (HamburgerMenu.IsPaneOpen &&
                (HamburgerMenu.DisplayMode == SplitViewDisplayMode.Overlay ||
                 HamburgerMenu.DisplayMode == SplitViewDisplayMode.CompactOverlay))
            { return AppViewBackButtonVisibility.Disabled; }

            HamburgerMenuFrame.GoBack();
            return AppViewBackButtonVisibility.Visible;
        }

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
        }
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
                new MenuItem() { Icon = Symbol.Home, Name = loader.GetString("Home"), PageType = null, ViewModels = new ViewModels.IndexPage.ViewModel("/main/indexV8"), Index = 0},
                new MenuItem() { Icon = Symbol.People, Name = loader.GetString("Circle"), PageType = null, Index = 1},
                new MenuItem() { Icon = Symbol.Favorite, Name = loader.GetString("Follow"), PageType = null, Index = 2},
                new MenuItem() { Icon = Symbol.Calendar, Name = loader.GetString("History"), PageType = typeof(HistoryPage),ViewModels = new ViewModels.HistoryPage.ViewModel("浏览历史"), Index = 3},
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                 new MenuItem() { Icon = Symbol.Contact, Name = loader.GetString("User"), PageType = typeof(BrowserPage), ViewModels = new object[]{ true}, Index = 0},
                 new MenuItem() { Icon = Symbol.Setting, Name = loader.GetString("Setting"), PageType = typeof(SettingsPage), Index = 1}
            };
            return items;
        }
    }
}
