﻿using Windows.Foundation.Metadata;

namespace CoolapkLite.Helpers
{
    public static class ApiInfoHelper
    {
        #region Contract

        /// <summary>
        /// Get is <see cref="Windows.Foundation.UniversalApiContract"/> 2 (Windows 10 version 1511 (10586, November Update)) present.
        /// </summary>
        public static bool IsUniversalApiContract2Present { get; } = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2);

        /// <summary>
        /// Get is <see cref="Windows.Foundation.UniversalApiContract"/> 14 (Windows 11 version 21H2 (22000)) present.
        /// </summary>
        public static bool IsUniversalApiContract14Present { get; } = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 14);

        #endregion

        #region Types

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.Activation.ICommandLineActivatedEventArgs"/> supported.
        /// </summary>
        public static bool IsICommandLineActivatedEventArgsSupported => ApiInformation.IsTypePresent("Windows.ApplicationModel.Activation.ICommandLineActivatedEventArgs");

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.AppExtensions.AppExtension"/> supported.
        /// </summary>
        public static bool IsAppExtensionSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.AppExtensions.AppExtension");

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.Search.SearchPane"/> supported.
        /// </summary>
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane");

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.UserActivities.UserActivityChannel"/> supported.
        /// </summary>
        public static bool IsUserActivityChannelSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.UserActivities.UserActivityChannel");

        /// <summary>
        /// Gets is <see cref="Windows.Phone.UI.Input.HardwareButtons"/> supported.
        /// </summary>
        public static bool IsHardwareButtonsSupported { get; } = ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

        /// <summary>
        /// Gets is <see cref="Windows.Security.Authorization.AppCapabilityAccess.AppCapability"/> supported.
        /// </summary>
        public static bool IsAppCapabilitySupported { get; } = ApiInformation.IsTypePresent("Windows.Security.Authorization.AppCapabilityAccess.AppCapability");

        /// <summary>
        /// Gets is <see cref="Windows.UI.ApplicationSettings.SettingsPane"/> supported.
        /// </summary>
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        /// <summary>
        /// Gets is <see cref="Windows.UI.StartScreen.JumpList"/> supported.
        /// </summary>
        public static bool IsJumpListSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList");

        /// <summary>
        /// Gets is <see cref="Windows.UI.ViewManagement.StatusBar"/> supported.
        /// </summary>
        public static bool IsStatusBarSupported => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        /// <summary>
        /// Gets is <see cref="Windows.UI.WindowManagement.AppWindow"/> supported.
        /// </summary>
        public static bool IsAppWindowSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.ElementSoundPlayer"/> supported.
        /// </summary>
        public static bool IsElementSoundPlayerSupported => ApiInformation.IsTypePresent("Windows.UI.Xaml.ElementSoundPlayer");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.CommandBarFlyout"/> supported.
        /// </summary>
        public static bool IsCommandBarFlyoutSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.CommandBarFlyout");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.MediaPlayerElement"/> supported.
        /// </summary>
        public static bool IsMediaPlayerElementSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.MediaPlayerElement");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.RefreshContainer"/> supported.
        /// </summary>
        public static bool IsRefreshContainerSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.RefreshContainer");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Media.AcrylicBrush"/> supported.
        /// </summary>
        public static bool IsAcrylicBrushSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Media.XamlCompositionBrushBase"/> supported.
        /// </summary>
        public static bool IsXamlCompositionBrushBaseSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase");

        #endregion

        #region Methods

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(bool)"/> supported.
        /// </summary>
        public static bool IsEnablePrelaunchSupported { get; } = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync(string)"/> supported.
        /// </summary>
        public static bool IsRequestRestartAsyncSupported { get; } = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "RequestRestartAsync");

        /// <summary>
        /// Gets is <see cref="Windows.System.AppDiagnosticInfo.RequestInfoForAppAsync()"/> supported.
        /// </summary>
        public static bool IsRequestInfoForAppAsyncSupported { get; } = ApiInformation.IsMethodPresent("Windows.System.AppDiagnosticInfo", "RequestInfoForAppAsync");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Composition.Compositor.TryCreateBlurredWallpaperBackdropBrush"/> supported.
        /// </summary>
        public static bool IsTryCreateBlurredWallpaperBackdropBrushSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush");

        /// <summary>
        /// Gets is <see cref="Windows.UI.ViewManagement.ApplicationView.GetDisplayRegions"/> supported.
        /// </summary>
        public static bool IsGetDisplayRegionsSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "GetDisplayRegions");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(Windows.UI.Xaml.UIElement)"/> supported.
        /// </summary>
        public static bool IsGetElementVisualSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementVisual");

        #endregion

        #region Properties

        /// <summary>
        /// Gets is <see cref="Windows.ApplicationModel.Activation.LaunchActivatedEventArgs.TileActivatedInfo"/> supported.
        /// </summary>
        public static bool IsTileActivatedInfoSupported { get; } = ApiInformation.IsPropertyPresent("Windows.ApplicationModel.Activation.LaunchActivatedEventArgs", "TileActivatedInfo");

        /// <summary>
        /// Gets is <see cref="Windows.System.DispatcherQueue.HasThreadAccess"/> supported.
        /// </summary>
        public static bool IsHasThreadAccessSupported { get; } = ApiInformation.IsPropertyPresent("Windows.System.DispatcherQueue", "HasThreadAccess");

        /// <summary>
        /// Gets is <see cref="Windows.UI.ViewManagement.ApplicationView.ViewMode"/> supported.
        /// </summary>
        public static bool IsApplicationViewViewModeSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.ViewManagement.ApplicationView", "ViewMode");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.FrameworkElement.FocusVisualMargin"/> supported.
        /// </summary>
        public static bool IsFocusVisualMarginSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "FocusVisualMargin");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.FrameworkElement.IsLoaded"/> supported.
        /// </summary>
        public static bool IsFrameworkElementIsLoadedSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.UIElement.ContextFlyout"/> supported.
        /// </summary>
        public static bool IsContextFlyoutSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "ContextFlyout");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.UIElement.XamlRoot"/> supported.
        /// </summary>
        public static bool IsXamlRootSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.ContentDialog.DefaultButton"/> supported.
        /// </summary>
        public static bool IsDefaultButtonSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.ContentDialog.CloseButtonText"/> supported.
        /// </summary>
        public static bool IsCloseButtonTextSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "CloseButtonText");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.Control.CornerRadiusProperty"/> supported.
        /// </summary>
        public static bool IsCornerRadiusSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "CornerRadiusProperty");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.Control.IsFocusEngaged"/> supported.
        /// </summary>
        public static bool IsFocusEngagedSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "IsFocusEngaged");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.MenuFlyoutItem.Icon"/> supported.
        /// </summary>
        public static bool IsMenuFlyoutItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.MenuFlyoutSubItem.Icon"/> supported.
        /// </summary>
        public static bool IsMenuFlyoutSubItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutSubItem", "Icon");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.RichEditBox.SelectionFlyout"/> supported.
        /// </summary>
        public static bool IsSelectionFlyoutSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.RichEditBox", "SelectionFlyout");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.ScrollContentPresenter.SizesContentToTemplatedParent"/> supported.
        /// </summary>
        public static bool IsSizesContentToTemplatedParentSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ScrollContentPresenter", "SizesContentToTemplatedParent");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem.Icon"/> supported.
        /// </summary>
        public static bool IsToggleMenuFlyoutItemIconSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem", "Icon");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.Primitives.FlyoutBase.ShouldConstrainToRootBounds"/> supported.
        /// </summary>
        public static bool IsShouldConstrainToRootBoundsSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Primitives.FlyoutBase", "ShouldConstrainToRootBounds");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Media.Imaging.BitmapImage.AutoPlay"/> supported.
        /// </summary>
        public static bool IsBitmapImageAutoPlaySupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay");

        #endregion

        #region Enums

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.FocusVisualKind.Reveal"/> supported.
        /// </summary>
        public static bool IsRevealFocusVisualKindSupported { get; } = ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.FocusVisualKind", "Reveal");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Xaml.Controls.ItemsUpdatingScrollMode.KeepLastItemInView"/> supported.
        /// </summary>
        public static bool IsKeepLastItemInViewSupported { get; } = ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.Controls.ItemsUpdatingScrollMode", "KeepLastItemInView");

        #endregion
    }
}
