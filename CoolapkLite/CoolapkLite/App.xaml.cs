using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Models.Network;
using CoolapkLite.Pages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Security.Authorization.AppCapabilityAccess;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

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
#if NETCORE463
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            if (ApiInfoHelper.IsRevealFocusVisualKindSupported)
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
            if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLaunching))
            {
                _ = CheckUpdateAsync();
            }
        }

        #region OnActivated

        protected override void OnActivated(IActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnActivated(e);
        }

        protected override void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnCachedFileUpdaterActivated(e);
        }

        protected override void OnFileActivated(FileActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileActivated(e);
        }

        protected override void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileOpenPickerActivated(e);
        }

        protected override void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileSavePickerActivated(e);
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnSearchActivated(e);
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnShareTargetActivated(e);
        }

        #endregion

        private void EnsureWindow(IActivatedEventArgs e)
        {
            if (!isLoaded)
            {
                _ = CreateJumpListAsync();
                _ = RequestWIFIAccessAsync();
                _ = RegisterBackgroundTaskAsync();
                RegisterExceptionHandlingSynchronizationContext();
                isLoaded = true;
            }

            Window window = Window.Current;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (!(window.Content is Frame rootFrame))
            {
                if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                {
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                }

                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                window.Content = rootFrame;

                WindowHelper.TrackWindow(window);
                ThemeHelper.Initialize();
            }

            if (e is LaunchActivatedEventArgs args)
            {
                if (!args.PrelaunchActivated)
                {
                    if (ApiInfoHelper.IsEnablePrelaunchSupported)
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
                _ = page.OpenActivatedEventArgsAsync(e);
            }

            // 确保当前窗口处于活动状态
            window.Activate();
        }

        private static async Task CreateJumpListAsync()
        {
            if (ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported())
            {
                JumpList JumpList = await JumpList.LoadCurrentAsync();
                JumpList.SystemGroupKind = JumpListSystemGroupKind.None;

                if (!JumpList.Items.Any(x => x.GroupName == "导航"))
                {
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("MainPage");
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("home", loader.GetString("Home")).AddGroupNameAndLogo("导航", new Uri("ms-appx:///Assets/Icons/Home.png")));
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("circle", loader.GetString("Circle")).AddGroupNameAndLogo("导航", new Uri("ms-appx:///Assets/Icons/People.png")));
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("favorites", loader.GetString("Bookmark")).AddGroupNameAndLogo("导航", new Uri("ms-appx:///Assets/Icons/FavoriteStar.png")));
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("history", loader.GetString("History")).AddGroupNameAndLogo("导航", new Uri("ms-appx:///Assets/Icons/Calendar.png")));
                }

                if (!JumpList.Items.Any(x => x.GroupName == "个人"))
                {
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("me", "个人空间").AddGroupNameAndLogo("个人", new Uri("ms-appx:///Assets/Icons/Contact.png")));
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("notifications", "通知").AddGroupNameAndLogo("个人", new Uri("ms-appx:///Assets/Icons/Message.png")));
                    JumpList.Items.Add(JumpListItem.CreateWithArguments("create", "写动态").AddGroupNameAndLogo("个人", new Uri("ms-appx:///Assets/Icons/Label.png")));
                }

                await JumpList.SaveAsync();
            }
        }

        private static async Task CheckUpdateAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            if (mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
#if CANARY
                UpdateInfo results = await UpdateHelper.CheckUpdateAsync("wherewhere", "Coolapk-UWP", 5).ConfigureAwait(false);
#else
                UpdateInfo results = await UpdateHelper.CheckUpdateAsync("Coolapk-UWP", "Coolapk-Lite").ConfigureAwait(false);
#endif
                if (results != null && results.IsExistNewVersion)
                {
                    ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse();

                    string name = _loader?.GetString("AppName") ?? Package.Current.DisplayName;
                    string ver = Package.Current.Id.Version.ToFormattedString(3);

                    new ToastContentBuilder()
                        .SetToastScenario(ToastScenario.Default)
                        .AddArgument("action", "hasUpdate")
                        .AddArgument("url", results?.ReleaseUrl)
                        .AddText(_loader.GetString("HasUpdateTitle"))
                        .AddText($"{name} v{ver} -> {results?.Version.ToString(3)}")
                        .AddText(string.Format(_loader.GetString("HasUpdateSubtitle"), results?.PublishedAt.ConvertDateTimeOffsetToReadable()))
                        .AddButton(new ToastButton()
#if CANARY
                            .SetContent(_loader.GetString("GoToDevOps"))
#else
                            .SetContent(_loader.GetString("GoToGithub"))
#endif
                            .SetProtocolActivation(results?.ReleaseUrl?.TryGetUri()))
                        .AddButton(new ToastButton()
                            .SetDismissActivation())
                        .Show();
                }
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
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
        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private static async Task RequestWIFIAccessAsync()
        {
            if (ApiInfoHelper.IsAppCapabilitySupported)
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

        private static void Application_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
            if (e.Exception is HttpRequestException || (e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895))
            {
                _ = UIHelper.ShowMessageAsync($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
            }
            else if (e.Exception is CoolapkMessageException)
            {
                _ = UIHelper.ShowMessageAsync(e.Exception.Message);
            }
            else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
            {
                _ = UIHelper.ShowMessageAsync($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - Application").Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

#if NETCORE463
        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception Exception)
            {
                SettingsHelper.LogManager.GetLogger("Unhandled Exception - CurrentDomain").Error(Exception.ExceptionToMessage(), Exception);
            }
        }
#endif

        /// <summary>
        /// Should be called from OnActivated and OnLaunched
        /// </summary>
        private static void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }

        private static void SynchronizationContext_UnhandledException(object sender, Common.UnhandledExceptionEventArgs e)
        {
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                if (e.Exception is HttpRequestException || (e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895))
                {
                    _ = UIHelper.ShowMessageAsync($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
                else if (e.Exception is CoolapkMessageException)
                {
                    _ = UIHelper.ShowMessageAsync(e.Exception.Message);
                }
                else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
                {
                    _ = UIHelper.ShowMessageAsync($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)} (0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
            }
            SettingsHelper.LogManager.GetLogger("Unhandled Exception - SynchronizationContext").Error(e.Exception.ExceptionToMessage(), e.Exception);
            e.Handled = true;
        }

        private static async Task RegisterBackgroundTaskAsync()
        {
            if (!ApiInfoHelper.IsTileActivatedInfoSupported)
            { return; }

            // Check for background access (optional)
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            if (status != BackgroundAccessStatus.Unspecified
                && status != BackgroundAccessStatus.Denied
                && status != (BackgroundAccessStatus)7)
            {
                if (SettingsHelper.Get<bool>(SettingsHelper.IsUseBackgroundTask))
                {
                    RegisterLiveTileTask();
                    RegisterNotificationsTask();
                }
                else
                {
                    UnregisterLiveTileTask();
                    UnregisterNotificationsTask();
                }
                RegisterToastBackgroundTask();
            }

            #region LiveTileTask

            const string LiveTileTask = nameof(BackgroundTasks.LiveTileTask);

            void RegisterLiveTileTask()
            {
                uint time = SettingsHelper.Get<uint>(SettingsHelper.TileUpdateTime);
                if (time < 15)
                {
                    UnregisterLiveTileTask();
                    return;
                }

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(LiveTileTask, new TimeTrigger(time, false), true);
            }

            void UnregisterLiveTileTask()
            {
                // If background task is not registered, do nothing
                if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Unregister (Single Process)
                BackgroundTaskHelper.Unregister(LiveTileTask);
            }

            #endregion

            #region NotificationsTask

            const string NotificationsTask = nameof(BackgroundTasks.NotificationsTask);

            void RegisterNotificationsTask()
            {
                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(NotificationsTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _NotificationsTask = BackgroundTaskHelper.Register(NotificationsTask, new TimeTrigger(15, false), true);
            }

            void UnregisterNotificationsTask()
            {
                // If background task is not registered, do nothing
                if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(NotificationsTask)))
                { return; }

                // Unregister (Single Process)
                BackgroundTaskHelper.Unregister(NotificationsTask);
            }

            #endregion

            #region ToastBackgroundTask

            void RegisterToastBackgroundTask()
            {
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

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case nameof(LiveTileTask):
                    LiveTileTask.Instance?.Run(args.TaskInstance);
                    break;

                case nameof(NotificationsTask):
                    NotificationsTask.Instance?.Run(args.TaskInstance);
                    break;

                case "ToastBackgroundTask":
                    if (args.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details)
                    {
                        ToastArguments arguments = ToastArguments.Parse(details.Argument);
                        if (arguments.TryGetValue("action", out string action))
                        {
                            switch (action)
                            {
                                case "hasUpdate":
                                    if (arguments.TryGetValue("url", out string url) && url.TryGetUri(out Uri uri))
                                    {
                                        _ = await Launcher.LaunchUriAsync(uri);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    deferral.Complete();
                    break;

                default:
                    deferral.Complete();
                    break;
            }
        }

        private bool isLoaded;
    }
}
