using CoolapkLite.Common;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
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
        public static bool IsAppWindowSupported { get; } = ApiInfoHelper.IsAppWindowSupported;
        public static bool IsXamlRootSupported { get; } = ApiInfoHelper.IsXamlRootSupported;

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

        public static void TrackWindow(this AppWindow window, Frame frame)
        {
            if (!ActiveAppWindows.ContainsKey(frame.Dispatcher))
            {
                ActiveAppWindows[frame.Dispatcher] = new Dictionary<UIElement, AppWindow>();
            }

            if (!ActiveAppWindows[frame.Dispatcher].ContainsKey(frame))
            {
                window.Closed += (sender, args) =>
                {
                    if (ActiveAppWindows.TryGetValue(frame.Dispatcher, out Dictionary<UIElement, AppWindow> windows))
                    {
                        windows?.Remove(frame);
                    }
                    frame.Content = null;
                    window = null;
                };
                ActiveAppWindows[frame.Dispatcher][frame] = window;
            }
        }

        public static bool IsAppWindow(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot?.Content != null
            && ActiveAppWindows.ContainsKey(element.Dispatcher)
            && ActiveAppWindows[element.Dispatcher].ContainsKey(element.XamlRoot.Content);

        public static AppWindow GetWindowForElement(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot?.Content != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out Dictionary<UIElement, AppWindow> windows)
            && windows.TryGetValue(element.XamlRoot.Content, out AppWindow window)
                ? window : null;

        public static UIElement GetXamlRootForWindow(this AppWindow window)
        {
            foreach (Dictionary<UIElement, AppWindow> windows in ActiveAppWindows.Values)
            {
                foreach (KeyValuePair<UIElement, AppWindow> element in windows)
                {
                    if (element.Value == window)
                    {
                        return element.Key;
                    }
                }
            }
            return null;
        }

        public static Size GetXAMLRootSize(this UIElement element) =>
            IsXamlRootSupported && element.XamlRoot != null
                ? element.XamlRoot.Size
                : Window.Current is Window window
                    ? window.Bounds.ToSize()
                    : CoreApplication.MainView.CoreWindow.Bounds.ToSize();

        public static void SetXAMLRoot(this UIElement element, UIElement target)
        {
            if (IsXamlRootSupported)
            {
                element.XamlRoot = target?.XamlRoot;
            }
        }

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = new Dictionary<CoreDispatcher, Window>();
        public static Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>> ActiveAppWindows { get; } = IsAppWindowSupported ? new Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>>() : null;
    }
}
