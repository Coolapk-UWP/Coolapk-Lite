using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
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
        private Point _clickPoint = new Point(0, 0);
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
                Provider = new ShowImageViewModel(Model, Dispatcher);
                DataContext = Provider;
            }
            else if (e.Parameter is ShowImageViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
            }
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
            Provider.PropertyChanged += Provider_PropertyChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.IsAppWindow())
            {
                AppWindow appWindow = this.GetWindowForElement();
                appWindow.Changed -= AppWindow_Changed;
                appWindow.Title = string.Empty;
            }
            else
            {
                Window.Current.SetTitleBar(null);
                SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
                Frame.Navigated -= On_Navigated;
            }
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed -= System_BackPressed; }
            Provider.PropertyChanged -= Provider_PropertyChanged;
        }

        private void Provider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Provider.Title):
                    UpdateTitle(Provider.Title);
                    break;
                default:
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsAppWindow())
            {
                AppWindow appWindow = this.GetWindowForElement();
                appWindow.Changed += AppWindow_Changed;
                appWindow.Title = Provider.Title;
            }
            else
            {
                Window.Current.SetTitleBar(CustomTitleBar);
                SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                SystemNavigationManager.AppViewBackButtonVisibility = TryGoBack(false);
                if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
                { UpdateContentLayout(false); }
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                SystemNavigationManager.BackRequested += System_BackRequested;
                ApplicationView.GetForCurrentView().Title = Provider.Title;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScrollViewer scrollViewer = element.Tag as ScrollViewer;
            switch (element.Name)
            {
                case "ZoomUp":
                    _ = scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor + 0.1f);
                    break;
                case "ZoomDown":
                    _ = scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor - 0.1f);
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
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
                    Provider.Images[Provider.Index].Type &= (ImageType)0xFE;
                    Provider.ShowOrigin = false;
                    break;
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Provider.IsShowHub = !Provider.IsShowHub;
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ChangeView(0, 0, 1);
            Provider.IsShowHub = !Provider.IsShowHub;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            FrameworkElement element = scrollViewer.Content as FrameworkElement;
            element.CanDrag = scrollViewer.ZoomFactor <= 1;
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await Provider.GetImageDataPackage(args.Data, "拖拽图片");
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                _clickPoint = e.GetCurrentPoint(element).Position;
            }
        }

        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScrollViewer scrollViewer = element.Parent as ScrollViewer;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                double x, y;
                Point point = e.GetCurrentPoint(element).Position;
                x = _clickPoint.X - point.X;
                y = _clickPoint.Y - point.Y;
                _ = scrollViewer.ChangeView(scrollViewer.HorizontalOffset + x, scrollViewer.VerticalOffset + y, null);
            }
        }

        public async void UpdateTitle(string title = null)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.ResumeForegroundAsync();
            }

            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Title = title ?? string.Empty;
            }
            else
            {
                ApplicationView.GetForCurrentView().Title = title ?? string.Empty;
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args) => UpdateContentLayout(sender.TitleBar.IsVisible);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateContentLayout(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);
    }
}
