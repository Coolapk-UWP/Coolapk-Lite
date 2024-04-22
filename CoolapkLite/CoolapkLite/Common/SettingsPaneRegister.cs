using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Common
{
    public class SettingsPaneRegister
    {
        private int count = -1;
        private UIElement element;

        public static bool IsSearchPaneSupported { get; } = ApiInfoHelper.IsSearchPaneSupported && CheckSearchExtension();
        public static bool IsSettingsPaneSupported => ApiInfoHelper.IsSettingsPaneSupported;

        public SettingsPaneRegister(Window window)
        {
            element = window.Content;

            if (IsSearchPaneSupported)
            {
                SearchPane searchPane = SearchPane.GetForCurrentView();
                searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            }

            if (IsSettingsPaneSupported)
            {
                SettingsPane settingsPane = SettingsPane.GetForCurrentView();
                settingsPane.CommandsRequested -= OnCommandsRequested;
                settingsPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
        }

        public SettingsPaneRegister(UIElement element)
        {
            this.element = element;

            if (IsSearchPaneSupported)
            {
                SearchPane searchPane = SearchPane.GetForCurrentView();
                searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            }

            if (IsSettingsPaneSupported)
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
            }
        }

        public static SettingsPaneRegister Register(Window window) => new SettingsPaneRegister(window);

        public static SettingsPaneRegister Register(UIElement element) => new SettingsPaneRegister(element);

        public void Unregister()
        {
            if (IsSearchPaneSupported)
            {
                SearchPane searchPane = SearchPane.GetForCurrentView();
                searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
            }

            if (IsSettingsPaneSupported)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                element.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }

            element = null;
        }

        private async void SearchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            string keyWord = args.QueryText;
            IList<string> results = null;
            SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            await Task.Run(async () =>
            {
                try
                {
                    count++;
                    await Task.Delay(500).ConfigureAwait(false);
                    if (count != 0) { return; }
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true).ConfigureAwait(false);
                    if (isSucceed && result != null && result is JArray array && array.Count > 0)
                    {
                        results = new List<string>(array.Count);
                        foreach (JObject token in array.OfType<JObject>())
                        {
                            string key = string.Empty;
                            switch (token.Value<string>("entityType"))
                            {
                                case "apk":
                                    key = new AppModel(token).Title;
                                    break;
                                case "searchWord":
                                default:
                                    key = new SearchWord(token).ToString();
                                    break;
                            }
                            if (!string.IsNullOrEmpty(key) && !results.Contains(key))
                            {
                                results.Add(key);
                            }
                        }
                    }
                }
                finally
                {
                    count--;
                }
            });
            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(results ?? Array.Empty<string>());
            deferral.Complete();
        }

        private void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                _ = element.FindDescendant<Page>().NavigateAsync(typeof(SearchingPage), new SearchingViewModel(args.QueryText, element.Dispatcher));
            }
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("SettingsPane");
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Settings",
                    loader.GetString("Settings"),
                    async handler => new SettingsFlyoutControl { RequestedTheme = await ThemeHelper.GetActualThemeAsync() }.Show()));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedback",
                    loader.GetString("Feedback"),
                    handler => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite/issues"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "LogFolder",
                    loader.GetString("LogFolder"),
                    async handler => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Translate",
                    loader.GetString("Translate"),
                    handler => _ = Launcher.LaunchUriAsync(new Uri("https://crowdin.com/project/CoolapkLite"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Repository",
                    loader.GetString("Repository"),
                    handler => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite"))));
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.Handled) { return; }
            if (args.EventType == CoreAcceleratorKeyEventType.KeyDown || args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown)
            {
                CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X when IsSettingsPaneSupported:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q when IsSearchPaneSupported:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }

        private static bool CheckSearchExtension()
        {
            XDocument doc = XDocument.Load(Path.Combine(Package.Current.InstalledLocation.Path, "AppxManifest.xml"));
            XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
            IEnumerable<XElement> extensions = doc.Root.Descendants(ns + "Extension");
            if (extensions != null)
            {
                foreach (XElement extension in extensions)
                {
                    XAttribute category = extension.Attribute("Category");
                    if (category != null && category.Value == "windows.search")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
