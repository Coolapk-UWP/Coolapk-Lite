using CoolapkLite.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        // Keep reference so it does not get optimized/garbage collected
        public static UISettings UISettings;

        public static WeakEvent<UISettingChangedType> UISettingChanged { get; } = new WeakEvent<UISettingChangedType>();

        #region ActualTheme

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme => GetActualTheme();

        public static ElementTheme GetActualTheme() =>
            GetActualTheme(Window.Current ?? CurrentApplicationWindow);

        public static ElementTheme GetActualTheme(Window window) =>
            window == null
                ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                : window.Dispatcher?.HasThreadAccess == false
                    ? window.Dispatcher?.AwaitableRunAsync(() =>
                        window.Content is FrameworkElement _rootElement
                            && _rootElement.RequestedTheme != ElementTheme.Default
                                ? _rootElement.RequestedTheme
                                : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                        CoreDispatcherPriority.High)?.AwaitByTaskCompleteSource()
                        ?? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                    : window.Content is FrameworkElement rootElement
                        && rootElement.RequestedTheme != ElementTheme.Default
                            ? rootElement.RequestedTheme
                            : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

        public static Task<ElementTheme> GetActualThemeAsync() =>
            GetActualThemeAsync(Window.Current ?? CurrentApplicationWindow);

        public static async Task<ElementTheme> GetActualThemeAsync(Window window) =>
            window == null
                ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                : window.Dispatcher?.HasThreadAccess == false
                    ? await window.Dispatcher?.AwaitableRunAsync(() =>
                        window.Content is FrameworkElement _rootElement
                            && _rootElement.RequestedTheme != ElementTheme.Default
                                ? _rootElement.RequestedTheme
                                : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                            CoreDispatcherPriority.High)
                    : window.Content is FrameworkElement rootElement
                        && rootElement.RequestedTheme != ElementTheme.Default
                            ? rootElement.RequestedTheme
                            : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

        #endregion

        #region RootTheme

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get => GetRootTheme();
            set => SetRootTheme(value);
        }

        public static ElementTheme GetRootTheme() =>
            GetRootTheme(Window.Current ?? CurrentApplicationWindow);

        public static ElementTheme GetRootTheme(Window window) =>
            window == null
                ? ElementTheme.Default
                : window.Dispatcher.HasThreadAccess
                    ? window.Content is FrameworkElement rootElement
                        ? rootElement.RequestedTheme
                        : ElementTheme.Default
                    : window.Dispatcher.AwaitableRunAsync(() =>
                        window.Content is FrameworkElement _rootElement
                            ? _rootElement.RequestedTheme
                            : ElementTheme.Default,
                        CoreDispatcherPriority.High).AwaitByTaskCompleteSource();

        public static Task<ElementTheme> GetRootThemeAsync() =>
            GetRootThemeAsync(Window.Current ?? CurrentApplicationWindow);

        public static async Task<ElementTheme> GetRootThemeAsync(Window window) =>
            window == null
                ? ElementTheme.Default
                : window.Dispatcher.HasThreadAccess
                    ? window.Content is FrameworkElement rootElement
                        ? rootElement.RequestedTheme
                        : ElementTheme.Default
                    : await window.Dispatcher.AwaitableRunAsync(() =>
                        window.Content is FrameworkElement _rootElement
                            ? _rootElement.RequestedTheme
                            : ElementTheme.Default,
                        CoreDispatcherPriority.High);

        public static async void SetRootTheme(ElementTheme value)
        {
            WindowHelper.ActiveWindows.Values.ForEach(async window =>
            {
                if (!window.Dispatcher.HasThreadAccess)
                {
                    await window.Dispatcher.ResumeForegroundAsync();
                }

                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }

                if (WindowHelper.IsAppWindowSupported && WindowHelper.ActiveAppWindows.TryGetValue(window.Dispatcher, out System.Collections.Generic.Dictionary<UIElement, AppWindow> appWindows))
                {
                    foreach (FrameworkElement element in appWindows.Keys.OfType<FrameworkElement>())
                    {
                        element.RequestedTheme = value;
                    }
                }
            });

            SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
        }

        public static async Task SetRootThemeAsync(ElementTheme value)
        {
            await Task.WhenAll(WindowHelper.ActiveWindows.Values.Select(async window =>
            {
                if (!window.Dispatcher.HasThreadAccess)
                {
                    await window.Dispatcher.ResumeForegroundAsync();
                }

                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }

                if (WindowHelper.IsAppWindowSupported && WindowHelper.ActiveAppWindows.TryGetValue(window.Dispatcher, out System.Collections.Generic.Dictionary<UIElement, AppWindow> appWindows))
                {
                    foreach (FrameworkElement element in appWindows.Keys.OfType<FrameworkElement>())
                    {
                        element.RequestedTheme = value;
                    }
                }
            }));

            SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
        }

        #endregion

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = Window.Current;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        public static async void Initialize(Window window)
        {
            CurrentApplicationWindow = CurrentApplicationWindow ?? window;
            if (window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = await GetActualThemeAsync();
            }
            UpdateSystemCaptionButtonColors(window);
        }

        public static async void Initialize(AppWindow window)
        {
            if (window?.GetXamlRootForWindow() is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = await GetActualThemeAsync();
            }
            UpdateSystemCaptionButtonColors(window);
        }

        private static async void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
        }

        public static bool IsDarkTheme()
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static async Task<bool> IsDarkThemeAsync()
        {
            ElementTheme ActualTheme = await GetActualThemeAsync();
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static bool IsDarkTheme(this ElementTheme ActualTheme)
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static void UpdateExtendViewIntoTitleBar(bool IsExtendsTitleBar)
        {
            WindowHelper.ActiveWindows.Values.ForEach(async window =>
            {
                if (window.Dispatcher?.HasThreadAccess == false)
                {
                    await window.Dispatcher.ResumeForegroundAsync();
                }

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = IsExtendsTitleBar;

                if (WindowHelper.IsAppWindowSupported && WindowHelper.ActiveAppWindows.TryGetValue(window.Dispatcher, out System.Collections.Generic.Dictionary<UIElement, AppWindow> appWindows))
                {
                    foreach (AppWindow appWindow in appWindows.Values)
                    {
                        appWindow.TitleBar.ExtendsContentIntoTitleBar = IsExtendsTitleBar;
                    }
                }
            });
        }

        public static async void UpdateSystemCaptionButtonColors()
        {
            bool IsDark = await IsDarkThemeAsync();
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            WindowHelper.ActiveWindows.Values.ForEach(async window =>
            {
                if (window.Dispatcher?.HasThreadAccess == false)
                {
                    await window.Dispatcher.ResumeForegroundAsync();
                }

                if (UIHelper.HasStatusBar)
                {
                    StatusBar StatusBar = StatusBar.GetForCurrentView();
                    StatusBar.ForegroundColor = ForegroundColor;
                    StatusBar.BackgroundColor = BackgroundColor;
                    StatusBar.BackgroundOpacity = 0; // 透明度
                }
                else
                {
                    bool ExtendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
                    ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                    TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                    TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                    TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
                }

                if (WindowHelper.IsAppWindowSupported && WindowHelper.ActiveAppWindows.TryGetValue(window.Dispatcher, out System.Collections.Generic.Dictionary<UIElement, AppWindow> appWindows))
                {
                    foreach (AppWindow appWindow in appWindows.Values)
                    {
                        bool ExtendViewIntoTitleBar = appWindow.TitleBar.ExtendsContentIntoTitleBar;
                        AppWindowTitleBar TitleBar = appWindow.TitleBar;
                        TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                        TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                        TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
                    }
                }
            });
        }

        public static async void UpdateSystemCaptionButtonColors(Window window)
        {
            if (window.Dispatcher?.HasThreadAccess == false)
            {
                await window.Dispatcher.ResumeForegroundAsync();
            }

            bool IsDark = window?.Content is FrameworkElement rootElement ? IsDarkTheme(rootElement.RequestedTheme) : await IsDarkThemeAsync();
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            if (UIHelper.HasStatusBar)
            {
                StatusBar StatusBar = StatusBar.GetForCurrentView();
                StatusBar.ForegroundColor = ForegroundColor;
                StatusBar.BackgroundColor = BackgroundColor;
                StatusBar.BackgroundOpacity = 0; // 透明度
            }
            else
            {
                bool ExtendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
            }
        }

        public static async void UpdateSystemCaptionButtonColors(AppWindow window)
        {
            if (!(ThreadSwitcher.IsHasThreadAccessPropertyAvailable && window.DispatcherQueue?.HasThreadAccess == false))
            {
                await window.DispatcherQueue.ResumeForegroundAsync();
            }

            bool IsDark = window.GetXamlRootForWindow() is FrameworkElement rootElement ? IsDarkTheme(rootElement.RequestedTheme) : await IsDarkThemeAsync();
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            bool ExtendViewIntoTitleBar = window.TitleBar.ExtendsContentIntoTitleBar;
            AppWindowTitleBar TitleBar = window.TitleBar;
            TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
            TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
            TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }
}
