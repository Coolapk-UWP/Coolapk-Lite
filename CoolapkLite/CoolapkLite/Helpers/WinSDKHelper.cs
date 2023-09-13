using Windows.Foundation.Metadata;

namespace CoolapkLite.Helpers
{
    public static class ApiInfoHelper
    {
        #region Types

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.Activation.ICommandLineActivatedEventArgs"/> supported.
        /// </summary>
        public static bool IsICommandLineActivatedEventArgsSupported => ApiInformation.IsTypePresent("Windows.ApplicationModel.Activation.ICommandLineActivatedEventArgs");

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.AppExtensions.AppExtension"/> supported.
        /// </summary>
        public static bool IsAppExtensionSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.AppExtensions.AppExtension");

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.Search.SearchPane"/> supported.
        /// </summary>
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane");

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.UserActivities.UserActivityChannel"/> supported.
        /// </summary>
        public static bool IsUserActivityChannelSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.UserActivities.UserActivityChannel");

        /// <summary>
        /// Is <see cref="Windows.Phone.UI.Input.HardwareButtons"/> supported.
        /// </summary>
        public static bool IsHardwareButtonsSupported { get; } = ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

        /// <summary>
        /// Is <see cref="Windows.Security.Authorization.AppCapabilityAccess.AppCapability"/> supported.
        /// </summary>
        public static bool IsAppCapabilitySupported { get; } = ApiInformation.IsTypePresent("Windows.Security.Authorization.AppCapabilityAccess.AppCapability");

        /// <summary>
        /// Is <see cref="Windows.System.AppDiagnosticInfo"/> supported.
        /// </summary>
        public static bool IsAppDiagnosticInfoSupported { get; } = ApiInformation.IsTypePresent("Windows.System.AppDiagnosticInfo");

        /// <summary>
        /// Is <see cref="Windows.UI.ApplicationSettings.SettingsPane"/> supported.
        /// </summary>
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        /// <summary>
        /// Is <see cref="Windows.UI.StartScreen.JumpList"/> supported.
        /// </summary>
        public static bool IsJumpListSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList");

        /// <summary>
        /// Is <see cref="Windows.UI.ViewManagement.StatusBar"/> supported.
        /// </summary>
        public static bool IsStatusBarSupported => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        /// <summary>
        /// Is <see cref="Windows.UI.WindowManagement.AppWindow"/> supported.
        /// </summary>
        public static bool IsAppWindowSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.CommandBarFlyout"/> supported.
        /// </summary>
        public static bool IsCommandBarFlyoutSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.CommandBarFlyout");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.MediaPlayerElement"/> supported.
        /// </summary>
        public static bool IsMediaPlayerElementSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.MediaPlayerElement");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.RefreshContainer"/> supported.
        /// </summary>
        public static bool IsRefreshContainerSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.RefreshContainer");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Media.AcrylicBrush"/> supported.
        /// </summary>
        public static bool IsAcrylicBrushSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Media.XamlCompositionBrushBase"/> supported.
        /// </summary>
        public static bool IsXamlCompositionBrushBaseSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase");

        #endregion

        #region Methods

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(bool)"/> supported.
        /// </summary>
        public static bool IsEnablePrelaunchSupported { get; } = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

        /// <summary>
        /// Is <see cref="Windows.UI.Composition.Compositor.TryCreateBlurredWallpaperBackdropBrush"/> supported.
        /// </summary>
        public static bool IsTryCreateBlurredWallpaperBackdropBrushSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush");

        /// <summary>
        /// Is <see cref="Windows.UI.ViewManagement.ApplicationView.GetDisplayRegions"/> supported.
        /// </summary>
        public static bool IsGetDisplayRegionsSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "GetDisplayRegions");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(Windows.UI.Xaml.UIElement)"/> supported.
        /// </summary>
        public static bool IsGetElementVisualSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementVisual");

        #endregion

        #region Properties

        /// <summary>
        /// Is <see cref="Windows.ApplicationModel.Activation.LaunchActivatedEventArgs.TileActivatedInfo"/> supported.
        /// </summary>
        public static bool IsTileActivatedInfoSupported { get; } = ApiInformation.IsPropertyPresent("Windows.ApplicationModel.Activation.LaunchActivatedEventArgs", "TileActivatedInfo");

        /// <summary>
        /// Is <see cref="Windows.System.DispatcherQueue.HasThreadAccess"/> supported.
        /// </summary>
        public static bool IsHasThreadAccessSupported { get; } = ApiInformation.IsPropertyPresent("Windows.System.DispatcherQueue", "HasThreadAccess");

        /// <summary>
        /// Is <see cref="Windows.UI.ViewManagement.ApplicationView.ViewMode"/> supported.
        /// </summary>
        public static bool IsApplicationViewViewModeSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.ViewManagement.ApplicationView", "ViewMode");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.FrameworkElement.FocusVisualMargin"/> supported.
        /// </summary>
        public static bool IsFocusVisualMarginSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "FocusVisualMargin");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.FrameworkElement.IsLoaded"/> supported.
        /// </summary>
        public static bool IsFrameworkElementIsLoadedSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.UIElement.XamlRoot"/> supported.
        /// </summary>
        public static bool IsXamlRootSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.ContentDialog.DefaultButton"/> supported.
        /// </summary>
        public static bool IsDefaultButtonSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.ContentDialog.CloseButtonText"/> supported.
        /// </summary>
        public static bool IsCloseButtonTextSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "CloseButtonText");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.Control.CornerRadiusProperty"/> supported.
        /// </summary>
        public static bool IsCornerRadiusSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "CornerRadiusProperty");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.MenuFlyoutItem.Icon"/> supported.
        /// </summary>
        public static bool IsMenuFlyoutItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.MenuFlyoutSubItem.Icon"/> supported.
        /// </summary>
        public static bool IsMenuFlyoutSubItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutSubItem", "Icon");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.RichEditBox.SelectionFlyout"/> supported.
        /// </summary>
        public static bool IsSelectionFlyoutSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.RichEditBox", "SelectionFlyout");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.ScrollContentPresenter.SizesContentToTemplatedParent"/> supported.
        /// </summary>
        public static bool IsSizesContentToTemplatedParentSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ScrollContentPresenter", "SizesContentToTemplatedParent");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem.Icon"/> supported.
        /// </summary>
        public static bool IsToggleMenuFlyoutItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem", "Icon");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.Primitives.FlyoutBase.ShouldConstrainToRootBounds"/> supported.
        /// </summary>
        public static bool IsShouldConstrainToRootBoundsSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Primitives.FlyoutBase", "ShouldConstrainToRootBounds");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Media.Imaging.BitmapImage.AutoPlay"/> supported.
        /// </summary>
        public static bool IsBitmapImageAutoPlaySupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay");

        #endregion

        #region Enums

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.FocusVisualKind.Reveal"/> supported.
        /// </summary>
        public static bool IsRevealFocusVisualKindSupported { get; } = ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.FocusVisualKind", "Reveal");

        /// <summary>
        /// Is <see cref="Windows.UI.Xaml.Controls.ItemsUpdatingScrollMode.KeepLastItemInView"/> supported.
        /// </summary>
        public static bool IsKeepLastItemInViewSupported { get; } = ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.Controls.ItemsUpdatingScrollMode", "KeepLastItemInView");

        #endregion
    }
}
