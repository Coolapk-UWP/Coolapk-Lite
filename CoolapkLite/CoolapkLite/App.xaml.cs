﻿using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Security.Authorization.AppCapabilityAccess;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace CoolapkLite
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            InitializeComponent();

            Suspending += OnSuspending;
            UnhandledException += Application_UnhandledException;

            if (ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.FocusVisualKind", "Reveal"))
            {
                FocusVisualKind = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" ? FocusVisualKind.Reveal : FocusVisualKind.HighVisibility;
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureWindow(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnActivated(e);
        }

        private async void EnsureWindow(IActivatedEventArgs e)
        {
            if (MainWindow == null)
            {
                RequestWIFIAccess();
                RegisterBackgroundTask();
                RegisterExceptionHandlingSynchronizationContext();

                MainWindow = Window.Current;

                if (ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList") && JumpList.IsSupported())
                {
                    JumpList JumpList = await JumpList.LoadCurrentAsync();
                    JumpList.SystemGroupKind = JumpListSystemGroupKind.None;
                }
            }

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (!(MainWindow.Content is Frame rootFrame))
            {
                if (SystemInformation.Instance.OperatingSystemVersion.Build >= 10586)
                {
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                }

                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
                {
                    SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                    rootFrame.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
                    Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Styles/SettingsFlyout.xaml") });
                }

                if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane"))
                {
                    SearchPane searchPane = SearchPane.GetForCurrentView();
                    searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                    searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
                }

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;

                ThemeHelper.Initialize();
            }

            if (e is LaunchActivatedEventArgs args)
            {
                if (!args.PrelaunchActivated)
                {
                    if (ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch"))
                    {
                        CoreApplication.EnablePrelaunch(true);
                    }
                }
                else { return; }
            }

            if (rootFrame.Content == null)
            {
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                rootFrame.Navigate(page, e, new DrillInNavigationTransitionInfo());
            }
            else if (rootFrame.Content is IHaveTitleBar page)
            {
                _ = page.OpenActivatedEventArgs(e);
            }

            // 确保当前窗口处于活动状态
            MainWindow.Activate();
        }

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private async void SearchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            string keyWord = args.QueryText;
            List<string> results = new List<string>();
            SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            await Task.Run(async () =>
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true);
                    if (isSucceed && result != null && result is JArray array && array.Count > 0)
                    {
                        foreach (JToken token in array)
                        {
                            string key = string.Empty;
                            switch (token.Value<string>("entityType"))
                            {
                                case "apk":
                                    key = new AppModel(token as JObject).Title;
                                    break;
                                case "searchWord":
                                default:
                                    key = new SearchWord(token as JObject).ToString();
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
                    semaphoreSlim.Release();
                }
            });
            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(results);
            deferral.Complete();
        }

        private void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                _ = UIHelper.AppTitle.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(args.QueryText));
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("SettingsPane");
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Settings",
                    loader.GetString("Settings"),
                    (handler) => new SettingsFlyoutControl { RequestedTheme = ThemeHelper.ActualTheme }.Show()));
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
            if (args.EventType.ToString().Contains("Down"))
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
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }

        private async void RequestWIFIAccess()
        {
            if (ApiInformation.IsMethodPresent("Windows.Security.Authorization.AppCapabilityAccess.AppCapability", "Create"))
            {
                AppCapability WIFIData = AppCapability.Create("wifiData");
                switch (WIFIData.CheckAccess())
                {
                    case AppCapabilityAccessStatus.DeniedByUser:
                    case AppCapabilityAccessStatus.DeniedBySystem:
                        // Do something
                        await WIFIData.RequestAccessAsync();
                        break;
                }
            }
        }

        private void Application_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (!(!SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException) || e.Exception is TaskCanceledException || e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                UIHelper.ShowMessage($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - Application").Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

        /// <summary>
        /// Should be called from OnActivated and OnLaunched
        /// </summary>
        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }

        private void SynchronizationContext_UnhandledException(object sender, Common.UnhandledExceptionEventArgs e)
        {
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                if (e.Exception is HttpRequestException || (e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895))
                {
                    UIHelper.ShowMessage($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
                else if (e.Exception is CoolapkMessageException)
                {
                    UIHelper.ShowMessage(e.Exception.Message);
                }
                else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
                {
                    UIHelper.ShowMessage($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - SynchronizationContext").Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

        private static async void RegisterBackgroundTask()
        {
            // Check for background access (optional)
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            if (status != BackgroundAccessStatus.Unspecified
                && status != BackgroundAccessStatus.Denied
                && status != (BackgroundAccessStatus)7)
            {
                RegisterLiveTileTask();
                RegisterNotificationsTask();
                RegisterToastBackgroundTask();
            }

            #region LiveTileTask

            void RegisterLiveTileTask()
            {
                uint time = SettingsHelper.Get<uint>(SettingsHelper.TileUpdateTime);
                if (time < 15) { return; }
#if ARM64
                const string LiveTileTask = "LiveTileTask";

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(LiveTileTask, new TimeTrigger(time, false), true);
#else
                if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(nameof(LiveTileTask)))
                {
                    // Register (Multi Process)
                    BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(typeof(LiveTileTask), new TimeTrigger(time, false), true);
                }
#endif
            }

            #endregion

            #region NotificationsTask

            void RegisterNotificationsTask()
            {
#if ARM64
                const string NotificationsTask = "NotificationsTask";

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(NotificationsTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _NotificationsTask = BackgroundTaskHelper.Register(NotificationsTask, new TimeTrigger(15, false), true);
#else
                if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(nameof(NotificationsTask)))
                {
                    // Register (Multi Process)
                    BackgroundTaskRegistration _NotificationsTask = BackgroundTaskHelper.Register(typeof(NotificationsTask), new TimeTrigger(15, false), true);
                }
#endif
            }

            #endregion

            #region ToastBackgroundTask

            void RegisterToastBackgroundTask()
            {
                if (!ApiInformation.IsTypePresent("Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs2"))
                { return; }

                const string ToastBackgroundTask = "ToastBackgroundTask";

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(ToastBackgroundTask)))
                { return; }

                // Create the background task
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder
                { Name = ToastBackgroundTask };

                // Assign the toast action trigger
                builder.SetTrigger(new ToastNotificationActionTrigger());

                // And register the task
                BackgroundTaskRegistration registration = builder.Register();
            }

            #endregion
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "LiveTileTask":
                    LiveTileTask.Instance?.Run(args.TaskInstance);
                    break;

                case "NotificationsTask":
                    NotificationsTask.Instance?.Run(args.TaskInstance);
                    break;

                case "ToastBackgroundTask":
                    if (args.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details)
                    {
                        //ToastArguments arguments = ToastArguments.Parse(details.Argument);
                        ValueSet userInput = details.UserInput;

                        // Perform tasks
                    }
                    break;

                default:
                    deferral.Complete();
                    break;
            }

            deferral.Complete();
        }

        public static Window MainWindow { get; private set; }
    }
}
