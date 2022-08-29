using CoolapkLite.BackgroundTasks;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Exceptions;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Security.Authorization.AppCapabilityAccess;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            AddBrushResource();
            RequestWifiAccess();
            RegisterBackgroundTask();
            RegisterExceptionHandlingSynchronizationContext();
            EnsureWindow(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnActivated(e);
        }

        private void EnsureWindow(IActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e is LaunchActivatedEventArgs args)
            {
                if (args.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        // 当导航堆栈尚未还原时，导航到第一页，
                        // 并通过将所需信息作为导航参数传入来配置
                        // 参数
                        rootFrame.Navigate(typeof(MainPage), args.Arguments);
                    }
                    ThemeHelper.Initialize();
                    // 确保当前窗口处于活动状态
                    Window.Current.Activate();
                }
                else
                {
                    return;
                }
            }
            else if (rootFrame.Content == null)
            {
                AddBrushResource();
                RequestWifiAccess();
                RegisterBackgroundTask();
                RegisterExceptionHandlingSynchronizationContext();

                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                rootFrame.Navigate(typeof(MainPage));

                ThemeHelper.Initialize();
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }

            if (e.Kind == ActivationKind.Protocol && UIHelper.MainPage != null)
            {
                ProtocolActivatedEventArgs protocolArgs = (ProtocolActivatedEventArgs)e;
                switch (protocolArgs.Uri.Host)
                {
                    case "www.coolapk.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolapk.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "www.coolmarket.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolmarket.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "http":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.Host + ":" + protocolArgs.Uri.AbsolutePath);
                        break;
                    case "https":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.Host + ":" + protocolArgs.Uri.AbsolutePath);
                        break;
                    case "settings":
                        UIHelper.Navigate(typeof(SettingsPage));
                        break;
                    case "flags":
                        UIHelper.Navigate(typeof(TestPage));
                        break;
                    case "history":
                        UIHelper.Navigate(typeof(HistoryPage));
                        break;
                    default:
                        UIHelper.OpenLinkAsync("/" + protocolArgs.Uri.Host + protocolArgs.Uri.AbsolutePath);
                        break;
                }
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

        private void AddBrushResource()
        {
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
            {
                ResourceDictionary MicaBrushs = new ResourceDictionary();
                MicaBrushs.Source = new Uri("ms-appx:///Styles/Brushes/MicaBrushes.xaml");
                Resources.MergedDictionaries.Add(MicaBrushs);
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                ResourceDictionary AcrylicBrushs = new ResourceDictionary();
                AcrylicBrushs.Source = new Uri("ms-appx:///Styles/Brushes/AcrylicBrushes.xaml");
                Resources.MergedDictionaries.Add(AcrylicBrushs);
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                ResourceDictionary BlurBrushs = new ResourceDictionary();
                BlurBrushs.Source = new Uri("ms-appx:///Styles/Brushes/BlurBrushes.xaml");
                Resources.MergedDictionaries.Add(BlurBrushs);
            }
            else
            {
                ResourceDictionary SolidBrushs = new ResourceDictionary();
                SolidBrushs.Source = new Uri("ms-appx:///Styles/Brushes/SolidBrushes.xaml");
                Resources.MergedDictionaries.Add(SolidBrushs);
            }
        }

        private async void RequestWifiAccess()
        {
            if (ApiInformation.IsMethodPresent("Windows.Security.Authorization.AppCapabilityAccess.AppCapability", "Create"))
            {
                var wifiData = AppCapability.Create("wifiData");
                switch (wifiData.CheckAccess())
                {
                    case AppCapabilityAccessStatus.DeniedByUser:
                    case AppCapabilityAccessStatus.DeniedBySystem:
                        // Do something
                        await AppCapability.Create("wifiData").RequestAccessAsync();
                        break;
                }
            }
        }

        private void Application_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (!(!SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException) || e.Exception is TaskCanceledException || e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                UIHelper.ShowMessage($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)}\n(0x{Convert.ToString(e.Exception.HResult, 16)})"
#if DEBUG
                                    + $"\n{e.Exception.StackTrace}"
#endif
                                );
            }
            SettingsHelper.LogManager.GetLogger(e.Exception.GetType()).Error($"\nMessage: {e.Exception.Message}\n{e.Exception.HResult}(0x{Convert.ToString(e.Exception.HResult, 16)}){e.Exception.HelpLink}", e.Exception);
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

        private void SynchronizationContext_UnhandledException(object sender, Helpers.Exceptions.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                if (e.Exception is System.Net.Http.HttpRequestException || (e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895))
                {
                    UIHelper.ShowMessage($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
                else if (e.Exception is CoolapkMessageException)
                {
                    UIHelper.ShowMessage(e.Exception.Message);
                }
                else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
                {
                    UIHelper.ShowMessage($"{(string.IsNullOrEmpty(e.Exception.Message) ? loader.GetString("ExceptionThrown") : e.Exception.Message)}\n(0x{Convert.ToString(e.Exception.HResult, 16)})"
#if DEBUG
                                            + $"\n{e.Exception.StackTrace}"
#endif
                                        );
                }
            }
            SettingsHelper.LogManager.GetLogger(e.Exception.GetType()).Error($"\nMessage: {e.Exception.Message}\n{e.Exception.HResult}(0x{Convert.ToString(e.Exception.HResult, 16)}){e.Exception.HelpLink}", e.Exception);
        }

        private static async void RegisterBackgroundTask()
        {
            // Check for background access (optional)
            await BackgroundExecutionManager.RequestAccessAsync();

            #region LiveTileTask

            if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(nameof(LiveTileTask)))
            {
                // Register (Multi Process)
                BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(typeof(LiveTileTask), new TimeTrigger(15, false), true);
            }

            #endregion

            #region NotificationsTask

            if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(nameof(NotificationsTask)))
            {
                // Register (Single Process)
                BackgroundTaskRegistration _NotificationsTask = BackgroundTaskHelper.Register(typeof(NotificationsTask), new TimeTrigger(15, false), true);
            }

            #endregion

            #region ToastBackgroundTask
            const string ToastBackgroundTask = "ToastBackgroundTask";

            // If background task is already registered, do nothing
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(ToastBackgroundTask)))
                return;

            // Otherwise request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // Create the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder { Name = ToastBackgroundTask };

            // Assign the toast action trigger
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the task
            try { BackgroundTaskRegistration registration = builder.Register(); } catch { }
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
    }
}
