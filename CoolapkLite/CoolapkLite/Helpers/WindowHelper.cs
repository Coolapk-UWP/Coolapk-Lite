﻿using CoolapkLite.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CoolapkLite.Helpers
{
    // Helpers class to allow the app to find the Window that contains an
    // arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    // of all active Windows.  The app code must call WindowHelper.CreateWindow
    // rather than "new Window" so we can keep track of all the relevant
    // windows. In the future, we would like to support this in platform APIs.
    public static class WindowHelper
    {
        public static bool IsAppWindowSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");

        public static async Task<bool> CreateWindow(Action<Window> launched)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            await newView.Dispatcher.ResumeForegroundAsync();
            int newViewId = ApplicationView.GetForCurrentView().Id;
            var newWindow = Window.Current;
            launched(newWindow);
            TrackWindow(newWindow);
            Window.Current.Activate();
            return await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        public static async Task<(AppWindow, Frame)> CreateWindow()
        {
            Frame newFrame = new Frame();
            AppWindow newWindow = await AppWindow.TryCreateAsync();
            ElementCompositionPreview.SetAppWindowContent(newWindow, newFrame);
            TrackWindow(newWindow, newFrame);
            return (newWindow, newFrame);
        }

        public static void TrackWindow(this Window window)
        {
            SettingsPaneRegister register = SettingsPaneRegister.Register(window);
            window.Closed += (sender, args) =>
            {
                ActiveWindows.Remove(window.Dispatcher);
                register.Unregister();
                window = null;
            };
            ActiveWindows.Add(window.Dispatcher, window);
        }

        public static void TrackWindow(this AppWindow window, Frame frame)
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

            if (!ActiveAppWindows.ContainsKey(frame.Dispatcher))
            {
                ActiveAppWindows[frame.Dispatcher] = new Dictionary<UIElement, AppWindow>();
            }

            ActiveAppWindows[frame.Dispatcher][frame] = window;
        }

        public static bool IsAppWindow(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot?.Content != null
            && ActiveAppWindows.ContainsKey(element.Dispatcher)
            && ActiveAppWindows[element.Dispatcher].ContainsKey(element.XamlRoot.Content);

        public static AppWindow GetWindowForElement(this UIElement element) =>
            IsAppWindowSupported
            && element?.XamlRoot?.Content != null
            && ActiveAppWindows.TryGetValue(element.Dispatcher, out var windows)
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

        public static void SetXAMLRoot(this UIElement element, UIElement target)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot"))
            {
                element.XamlRoot = target?.XamlRoot;
            }
        }

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = new Dictionary<CoreDispatcher, Window>();
        public static Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>> ActiveAppWindows { get; } = IsAppWindowSupported ? new Dictionary<CoreDispatcher, Dictionary<UIElement, AppWindow>>() : null;
    }
}
