using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
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
        private bool isShowHub = true;
        private Point _clickPoint = new Point(0, 0);

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(ShowImageViewModel),
                typeof(ShowImagePage),
                null);

        /// <summary>
        /// Get the <see cref="IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public ShowImageViewModel Provider
        {
            get => (ShowImageViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        #region ScrollViewer

        /// <summary>
        /// Identifies the <see cref="ScrollViewer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register(
                nameof(ScrollViewer),
                typeof(ScrollViewer),
                typeof(ShowImagePage),
                null);

        public ScrollViewer ScrollViewer
        {
            get => (ScrollViewer)GetValue(ScrollViewerProperty);
            private set => SetValue(ScrollViewerProperty, value);
        }

        #endregion

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
            }
            else if (e.Parameter is ShowImageViewModel ViewModel)
            {
                Provider = ViewModel;
            }
            if (ApiInfoHelper.IsHardwareButtonsSupported)
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
            if (ApiInfoHelper.IsHardwareButtonsSupported)
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
                { UpdateTitleBarVisible(false); }
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                SystemNavigationManager.BackRequested += System_BackRequested;
                ApplicationView.GetForCurrentView().Title = Provider.Title;
                TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
                Frame.Navigated += On_Navigated;
                UpdateTitleBarLayout(TitleBar);
            }
            UpdateScrollViewer(FlipView);
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

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            CustomTitleBar.Opacity = TitleBar.SystemOverlayLeftInset > 48 ? 0 : 1;
            LeftPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayRightInset);
        }

        private void UpdateTitleBarVisible(bool IsVisible)
        {
            CustomTitleBar.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case nameof(ZoomUp):
                    _ = ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + 0.1f);
                    break;
                case nameof(ZoomDown):
                    _ = ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor - 0.1f);
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(FlipView.SelectedItem is ImageModel image)) { return; }
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Copy":
                    image.CopyPic();
                    break;
                case "Save":
                    image.SavePic();
                    break;
                case "Share":
                    image.SharePic();
                    break;
                case "Refresh":
                    _ = image.Refresh();
                    break;
                case "Origin":
                    image.Type &= (ImageType)0xFE;
                    break;
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            UpdateHubVisibilityStates();
            if (e != null) { e.Handled = true; }
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ChangeView(0, 0, 1);
            UpdateHubVisibilityStates();
            if (e != null) { e.Handled = true; }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            FrameworkElement element = scrollViewer.Content as FrameworkElement;
            element.CanDrag = scrollViewer.ZoomFactor <= 1;
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            DragOperationDeferral deferral = args.GetDeferral();
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await (FlipView.SelectedItem as ImageModel)?.GetImageDataPackageAsync(args.Data, "拖拽图片");
            deferral.Complete();
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            FrameworkElement element = sender as FrameworkElement;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                _clickPoint = e.GetCurrentPoint(element).Position;
            }
            if (e != null) { e.Handled = true; }
        }

        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
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
            if (e != null) { e.Handled = true; }
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

        public void UpdateScrollViewer(FlipView flipView)
        {
            if (flipView != null && flipView.SelectedItem != null)
            {
                ScrollViewer = flipView.ContainerFromItem(flipView.SelectedItem)?.FindDescendant<ScrollViewer>();
            }
        }

        private void UpdateHubVisibilityStates()
        {
            isShowHub = !isShowHub;
            _ = isShowHub ? VisualStateManager.GoToState(this, "HubVisible", true)
                : VisualStateManager.GoToState(this, "HubCollapsed", true);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateScrollViewer(sender as FlipView);

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args) => UpdateTitleBarVisible(sender.TitleBar.IsVisible);

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarVisible(sender.IsVisible);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);
    }
}
