using CoolapkLite.Core.Exceptions;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Helpers;
using CoolapkLite.Pages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
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
            RegisterExceptionHandlingSynchronizationContext();
            ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
            view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;

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

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    if (SettingsHelper.WindowsVersion > 10586)
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(PivotPage), e.Arguments);
                    }
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
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

            Utils.NeedShowInAppMessageEvent += (_, arg) =>
            {
                switch (arg.Type)
                {
                    case MessageType.Message:
                        UIHelper.ShowMessage(arg.Message);
                        break;
                    default:
                        UIHelper.ShowMessage(arg.Type.ConvertMessageTypeToMessage());
                        break;
                }
            };
        }

        private void SynchronizationContext_UnhandledException(object sender, Core.Exceptions.UnhandledExceptionEventArgs e)
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
    }
}
