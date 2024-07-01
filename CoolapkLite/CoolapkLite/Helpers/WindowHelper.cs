using CoolapkLite.Common;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CoolapkLite.Helpers
{
    /// <summary>
    /// Helpers class to allow the app to find the Window that contains an
    /// arbitrary <see cref="UIElement"/> (<see cref="GetWindowForElement(UIElement)"/>).
    /// To do this, we keep track of all active Windows. The app code must call
    /// <see cref="CreateWindowAsync(Action{Window})"/> rather than "new <see cref="Window"/>()"
    /// so we can keep track of all the relevant windows.
    /// </summary>
    public static class WindowHelper
    {
        public static bool IsAppWindowSupported => ApiInfoHelper.IsAppWindowSupported;
        public static bool IsXamlRootSupported => ApiInfoHelper.IsXamlRootSupported;

        public static async Task<bool> CreateWindowAsync(Action<Window> launched)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = await newView.Dispatcher.AwaitableRunAsync(() =>
            {
                Window newWindow = Window.Current;
                launched(newWindow);
                TrackWindow(newWindow);
                Window.Current.Activate();
                return ApplicationView.GetForCurrentView().Id;
            });
            return await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        public static async Task<(AppWindow, Frame)> CreateWindowAsync()
        {
            Frame newFrame = new Frame();
            AppWindow newWindow = await AppWindow.TryCreateAsync();
            ElementCompositionPreview.SetAppWindowContent(newWindow, newFrame);
            TrackWindow(newWindow, newFrame);
            return (newWindow, newFrame);
        }

        public static void TrackWindow(this Window window)
        {
            if (!ActiveWindows.ContainsKey(window.Dispatcher))
            {
                SettingsPaneRegister register = SettingsPaneRegister.Register(window);
                window.Closed += (sender, args) =>
                {
                    ActiveWindows.Remove(window.Dispatcher);
                    register.Unregister();
                    window = null;
                };
                ActiveWindows[window.Dispatcher] = window;
            }
        }

        private static void TrackWindow(this AppWindow window, Frame frame)
        {
            if (!ActiveAppWindows.TryGetValue(frame.Dispatcher, out HashSet<AppWindow> windows))
            {
                ActiveAppWindows[frame.Dispatcher] = windows = new HashSet<AppWindow>();
            }

            if (!windows.Contains(window))
            {
                window.Closed += (sender, args) =>
                {
                    windows.Remove(window);
                    if (windows.Count <= 0)
                    { ActiveAppWindows.Remove(frame.Dispatcher); }
                    frame.Content = null;
                    window = null;
                };
                windows.Add(window);
            }
        }

        public static bool IsAppWindow(this UIElement element) =>
            IsAppWindowSupported
            && element?.UIContext != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out HashSet<AppWindow> windows)
            && windows.Any(x => x.UIContext == element.UIContext);

        public static AppWindow GetWindowForElement(this UIElement element) =>
            IsAppWindowSupported
            && element?.UIContext != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out HashSet<AppWindow> windows)
                ? windows.FirstOrDefault(x => x.UIContext == element.UIContext) : null;

        public static UIElement GetXamlRootForWindow(this AppWindow window) =>
            ElementCompositionPreview.GetAppWindowContent(window);

        public static Size GetXAMLRootSize(this UIElement element) =>
            IsXamlRootSupported && element.XamlRoot != null
                ? element.XamlRoot.Size
                : Window.Current is Window window
                    ? window.Bounds.ToSize()
                    : CoreApplication.MainView.CoreWindow.Bounds.ToSize();

        public static UIElement GetXAMLRoot(this UIElement element) =>
            IsXamlRootSupported && element.XamlRoot != null
                ? element.XamlRoot.Content
                : Window.Current is Window window
                    ? window.Content : null;

        public static void SetXAMLRoot(this UIElement element, UIElement target)
        {
            if (IsXamlRootSupported)
            {
                element.XamlRoot = target?.XamlRoot;
            }
        }

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = new Dictionary<CoreDispatcher, Window>();
        public static Dictionary<CoreDispatcher, HashSet<AppWindow>> ActiveAppWindows { get; } = IsAppWindowSupported ? new Dictionary<CoreDispatcher, HashSet<AppWindow>>() : null;
    }
}
