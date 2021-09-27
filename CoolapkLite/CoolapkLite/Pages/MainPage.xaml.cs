using CoolapkLite.Helpers;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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
                _ = HamburgerMenuFrame.Navigate(_page, vs, TransitionInfo);
            }
        }

        private void HamburgerMenu_ItemInvoked(object sender, ItemClickEventArgs e)
        {
            MenuItem MenuItem = e.ClickedItem as MenuItem;
            HamburgerMenu_Navigate(MenuItem, null);
        }

        private void HamburgerMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (Window.Current.Bounds.Width >= 1008)
            {
                HamburgerMenu.IsPaneOpen = true;
                TitleBar.RightMargin = new GridLength(16);
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (Window.Current.Bounds.Width <= 640)
            {
                TitleBar.RightMargin = new GridLength(48);
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.Overlay;
            }
            else
            {
                TitleBar.RightMargin = new GridLength(16);
                HamburgerMenu.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName((sender as FrameworkElement).Parent, "AutoSuggestBox");
            if (AutoSuggestBox != null)
            {
                AutoSuggestBox.Visibility = HamburgerMenu.IsPaneOpen ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void MainSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            FrameworkElement AutoSuggestBox = VisualTree.FindDescendantByName(sender, "AutoSuggestBox");
            if (AutoSuggestBox != null && AutoSuggestBox.Visibility == Visibility.Visible)
            {
                AutoSuggestBox.Visibility = Visibility.Collapsed;
            }
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

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }
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
        public string Name { get; set; }
        public Type PageType { get; set; }
        public int Index { get; set; }

        public static ObservableCollection<MenuItem> GetMainItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Home, Name = "首页", PageType = typeof(IndexPage), Index = 0}
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                 new MenuItem() { Icon = Symbol.Contact, Name = string.Empty/*, PageType = typeof(UserPage)*/, Index = 0},
                 new MenuItem() { Icon = Symbol.Setting, Name = "设置", PageType = typeof(SettingsPage), Index = 1}
            };
            return items;
        }
    }
}
