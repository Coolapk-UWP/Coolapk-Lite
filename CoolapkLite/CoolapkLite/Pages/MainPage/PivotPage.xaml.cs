using CoolapkLite.BackgroundTasks;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PivotPage : Page, IHaveTitleBar
    {
        private Action Refresh;
        public Frame MainFrame => PivotContentFrame;

        public PivotPage()
        {
            InitializeComponent();
            UIHelper.AppTitle = this;
            UIHelper.ShellDispatcher = Dispatcher;
            PivotContentFrame.Navigate(typeof(Page));
            if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
            { CommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right; }
            AppTitle.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
            { UpdateTitleBarLayout(false); }
            NotificationsModel.Instance?.Update();
            LiveTileTask.Instance?.UpdateTile();
            UpdateTitleBarLayout(TitleBar);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current.SetTitleBar(CustomTitleBar);
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            // Add handler for ContentFrame navigation.
            PivotContentFrame.Navigated += On_Navigated;
            Pivot.ItemsSource = GetMainItems();
            if (e.Parameter is IActivatedEventArgs ActivatedEventArgs)
            { OpenActivatedEventArgs(ActivatedEventArgs); }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SetTitleBar(null);
            SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed -= System_BackPressed; }
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
            TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
            PivotContentFrame.Navigated -= On_Navigated;
        }

        private void OpenActivatedEventArgs(IActivatedEventArgs args)
        {
            _ = UIHelper.OpenActivatedEventArgs(args);
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            HideProgressBar();
            if (PivotContentFrame.Visibility == Visibility.Collapsed)
            {
                Pivot.Visibility = Visibility.Collapsed;
                PivotContentFrame.Visibility = Visibility.Visible;
            }
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = PivotContentFrame.BackStackDepth == 0 ? AppViewBackButtonVisibility.Collapsed : AppViewBackButtonVisibility.Visible;
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
                        : MenuItem.Tag.ToString().Contains("V")
                            ? $"/page?url={MenuItem.Tag}"
                            : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}"));
                Refresh = () => _ = (Frame.Content as AdaptivePage).Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                Refresh = () => _ = AdaptivePage.Refresh(true);
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
                Pivot.Visibility = Visibility.Visible;
                PivotContentFrame.Visibility = Visibility.Collapsed;
            }

            return true;
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (Refresh != null)
            {
                Refresh();
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is ListView ListView && ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "User":
                    _ = await SettingsHelper.CheckLoginAsync()
                        ? PivotContentFrame.Navigate(typeof(ProfilePage), new ProfileViewModel())
                        : PivotContentFrame.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                case "Setting":
                    PivotContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

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

        public void ShowMessage(string message = null)
        {
            if (CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar)
            {
                AppTitle.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
            }
            else
            {
                ApplicationView.GetForCurrentView().Title = message ?? string.Empty;
            }
        }
        #endregion

        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("CirclePage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "indexV8", Header = loader.GetString("indexV8"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("follow"), Content = new Frame() },
                new PivotItem() { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem() { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem() { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem() { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem() { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }
    }
}
