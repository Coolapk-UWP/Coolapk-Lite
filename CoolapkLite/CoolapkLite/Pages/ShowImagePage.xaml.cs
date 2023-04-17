﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowImagePage : Page
    {
        private ShowImageViewModel Provider;

        public ShowImagePage()
        {
            InitializeComponent();
            if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
            { CommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ImageModel Model)
            {
                Provider = new ShowImageViewModel(Model);
                DataContext = Provider;
            }
            else if (e.Parameter is ShowImageViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
            }
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!this.IsAppWindow())
            {
                Window.Current.SetTitleBar(null);
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
                Frame.Navigated -= On_Navigated;
            }
            SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed -= System_BackPressed; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.IsAppWindow())
            {
                Window.Current.SetTitleBar(CustomTitleBar);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack(false);
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
                { UpdateContentLayout(false); }
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
                Frame.Navigated += On_Navigated;
                UpdateTitleBarLayout(TitleBar);
            }
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack();
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

        private AppViewBackButtonVisibility TryGoBack(bool goBack = true)
        {
            if (!Dispatcher.HasThreadAccess || !Frame.CanGoBack)
            { return AppViewBackButtonVisibility.Collapsed; }

            if (goBack) { Frame.GoBack(); }
            return AppViewBackButtonVisibility.Visible;
        }

        private void UpdateContentLayout(bool IsVisible)
        {
            CustomTitleBar.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            CustomTitleBar.Height = TitleBar.Height;
            LeftPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayRightInset);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Copy":
                    Provider.CopyPic();
                    break;
                case "Save":
                    Provider.SavePic();
                    break;
                case "Share":
                    Provider.SharePic();
                    break;
                case "Refresh":
                    _ = Provider.Refresh();
                    break;
                case "Origin":
                    Provider.Images[Provider.Index].Type = ImageType.OriginImage;
                    Provider.ShowOrigin = false;
                    break;
            }
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await Provider.GetImageDataPackage(args.Data, "拖拽图片");
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateContentLayout(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        private void FlipView_Tapped(object sender, TappedRoutedEventArgs e) => CommandBar.Visibility = CommandBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
}
