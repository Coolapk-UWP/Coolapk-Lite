using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class SettingsPaneRegister
    {
        private UIElement element;
        //private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public static bool IsSearchPaneSupported { get; } = /*ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane")*/false;
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public SettingsPaneRegister(Window window)
        {
            element = window.Content;

            //if (IsSearchPaneSupported)
            //{
            //    SearchPane searchPane = SearchPane.GetForCurrentView();
            //    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
            //    searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
            //    searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
            //    searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            //}

            if (IsSettingsPaneSupported)
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
        }

        public SettingsPaneRegister(UIElement element)
        {
            this.element = element;

            //if (IsSearchPaneSupported)
            //{
            //    SearchPane searchPane = SearchPane.GetForCurrentView();
            //    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
            //    searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
            //    searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
            //    searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            //}

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
            //if (IsSearchPaneSupported)
            //{
            //    SearchPane searchPane = SearchPane.GetForCurrentView();
            //    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
            //    searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
            //}

            if (IsSettingsPaneSupported)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                element.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }

            element = null;
        }

        //private async void SearchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        //{
        //    string keyWord = args.QueryText;
        //    List<string> results = new List<string>();
        //    SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
        //    await Task.Run(async () =>
        //    {
        //        await semaphoreSlim.WaitAsync();
        //        try
        //        {
        //            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true);
        //            if (isSucceed && result != null && result is JArray array && array.Count > 0)
        //            {
        //                foreach (JToken token in array)
        //                {
        //                    string key = string.Empty;
        //                    switch (token.Value<string>("entityType"))
        //                    {
        //                        case "apk":
        //                            key = new AppModel(token as JObject).Title;
        //                            break;
        //                        case "searchWord":
        //                        default:
        //                            key = new SearchWord(token as JObject).ToString();
        //                            break;
        //                    }
        //                    if (!string.IsNullOrEmpty(key) && !results.Contains(key))
        //                    {
        //                        results.Add(key);
        //                    }
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            semaphoreSlim.Release();
        //        }
        //    });
        //    args.Request.SearchSuggestionCollection.AppendQuerySuggestions(results);
        //    deferral.Complete();
        //}

        //private void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        //{
        //    if (!string.IsNullOrEmpty(args.QueryText))
        //    {
        //        _ = element.FindDescendant<Page>().NavigateAsync(typeof(SearchingPage), new SearchingViewModel(args.QueryText, element.Dispatcher));
        //    }
        //}

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("SettingsPane");
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Settings",
                    loader.GetString("Settings"),
                    async (handler) => new SettingsFlyoutControl { RequestedTheme = await ThemeHelper.GetActualThemeAsync() }.Show()));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedback",
                    loader.GetString("Feedback"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite/issues"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "LogFolder",
                    loader.GetString("LogFolder"),
                    async (handler) => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Translate",
                    loader.GetString("Translate"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://crowdin.com/project/CoolapkLite"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Repository",
                    loader.GetString("Repository"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite"))));
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.HasFlag(CoreAcceleratorKeyEventType.KeyDown) || args.EventType.HasFlag(CoreAcceleratorKeyEventType.SystemKeyUp))
            {
                CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X:
                                if (IsSettingsPaneSupported)
                                {
                                    SettingsPane.Show();
                                    args.Handled = true;
                                }
                                break;
                            //case VirtualKey.Q:
                            //    if (IsSearchPaneSupported)
                            //    {
                            //        SearchPane.GetForCurrentView().Show();
                            //        args.Handled = true;
                            //    }
                            //    break;
                        }
                    }
                }
            }
        }
    }
}
