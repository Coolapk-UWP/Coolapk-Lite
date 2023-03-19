using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static double TitleBarHeight = 32;
        public static double PageTitleHeight => HasStatusBar || HasTitleBar ? 48 : 48 + TitleBarHeight;
        public static Thickness StackPanelMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerPadding => new Thickness(0, -PageTitleHeight, 0, 0);
        public static Thickness PivotTitleMargin => new Thickness(0, HasStatusBar || HasTitleBar ? 0 : TitleBarHeight, 0, 0);

        private static CoreDispatcher shellDispatcher;
        public static CoreDispatcher ShellDispatcher
        {
            get => shellDispatcher;
            set
            {
                if (shellDispatcher == null)
                {
                    shellDispatcher = value;
                }
            }
        }

        private static readonly List<string> MessageList = new List<string>();
    }

    internal static partial class UIHelper
    {
        public static IHaveTitleBar AppTitle;

        public static async void ShowProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ShowProgressBar());
            }
        }

        public static async void ShowProgressBar(double value)
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ShowProgressBar(value));
            }
        }

        public static async void PausedProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.PausedProgressBar());
        }

        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ErrorProgressBar());
        }

        public static async void HideProgressBar()
        {
            IsShowingProgressBar = false;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
        }

        public static async void ShowMessage(string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                while (MessageList.Count > 0)
                {
                    if (HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        if (!string.IsNullOrEmpty(MessageList[0]))
                        {
                            statusBar.ProgressIndicator.Text = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                            statusBar.ProgressIndicator.ProgressValue = IsShowingProgressBar ? null : (double?)0;
                            await statusBar.ProgressIndicator.ShowAsync();
                            await Task.Delay(3000);
                        }
                        MessageList.RemoveAt(0);
                        if (MessageList.Count == 0 && !IsShowingProgressBar) { await statusBar.ProgressIndicator.HideAsync(); }
                        statusBar.ProgressIndicator.Text = string.Empty;
                    }
                    else
                    {
                        await AppTitle?.Dispatcher.AwaitableRunAsync(async () =>
                        {
                            if (AppTitle != null)
                            {
                                if (!string.IsNullOrEmpty(MessageList[0]))
                                {
                                    string messages = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                                    AppTitle.ShowMessage(messages);
                                    await Task.Delay(3000);
                                }
                                MessageList.RemoveAt(0);
                                if (MessageList.Count == 0)
                                {
                                    AppTitle.ShowMessage();
                                }
                            }
                        });
                    }
                }
                IsShowingMessage = false;
            }
        }

        public static void ShowInAppMessage(MessageType type, string message = null)
        {
            switch (type)
            {
                case MessageType.Message:
                    ShowMessage(message);
                    break;
                default:
                    ShowMessage(type.ConvertMessageTypeToMessage());
                    break;
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowInAppMessage(MessageType.Message, $"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowInAppMessage(MessageType.Message, "无法连接网络。"); }
            else { ShowInAppMessage(MessageType.Message, $"请检查网络连接。 {e.Message}"); }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            if (source == originalSource) { return true; }

            bool result = false;
            FrameworkElement DependencyObject = originalSource as FrameworkElement;
            if (DependencyObject.FindAscendant<ButtonBase>() == null && !(originalSource is ButtonBase) && !(originalSource is RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    result = source == DependencyObject.FindAscendant(FrameworkElement.Name);
                }
            }

            return DependencyObject.Tag == null && result;
        }

        public static string ConvertMessageTypeToMessage(this MessageType type)
        {
            switch (type)
            {
                case MessageType.NoMore:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMore");

                case MessageType.NoMoreShare:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreShare");

                case MessageType.NoMoreReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreReply");

                case MessageType.NoMoreHotReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreHotReply");

                case MessageType.NoMoreLikeUser:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreLikeUser");

                default: return string.Empty;
            }
        }

        public static void SetBadgeNumber(string badgeGlyphValue)
        {
            // Get the blank badge XML payload for a badge number
            XmlDocument badgeXml =
                BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> function)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.Invoke().ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });
            TResult taskResult = task.Result;
            return taskResult;
        }

        public static bool IsTypePresent(string AssemblyName, string TypeName)
        {
            try
            {
                Assembly asmb = Assembly.Load(new AssemblyName(AssemblyName));
                Type supType = asmb.GetType($"{AssemblyName}.{TypeName}");
                if (supType != null)
                {
                    try { Activator.CreateInstance(supType); }
                    catch (MissingMethodException) { }
                }
                return supType != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public enum NavigationThemeTransition
    {
        Default,
        Entrance,
        DrillIn,
        Suppress
    }

    internal static partial class UIHelper
    {
        public static MainPage MainPage;

        public static void Navigate(Type pageType, object e = null, NavigationThemeTransition Type = NavigationThemeTransition.Default)
        {
            switch (Type)
            {
                case NavigationThemeTransition.DrillIn:
                    _ = MainPage?.Dispatcher.AwaitableRunAsync(() => MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new DrillInNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Entrance:
                    _ = MainPage?.Dispatcher.AwaitableRunAsync(() => MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new EntranceNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Suppress:
                    _ = MainPage?.Dispatcher.AwaitableRunAsync(() => MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new SuppressNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Default:
                    _ = MainPage?.Dispatcher.AwaitableRunAsync(() => MainPage?.HamburgerMenuFrame.Navigate(pageType, e));
                    break;
                default:
                    _ = MainPage?.Dispatcher.AwaitableRunAsync(() => MainPage?.HamburgerMenuFrame.Navigate(pageType, e));
                    break;
            }
        }
        
        public static async Task ShowImageAsync(ImageModel image)
        {
            CoreApplicationView View = CoreApplication.CreateNewView();
            int ViewId = 0;
            await View.ExecuteOnUIThreadAsync(() =>
            {
                if (SystemInformation.Instance.OperatingSystemVersion.Build >= 10586)
                {
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                }
                Window window = Window.Current;
                WindowHelper.TrackWindow(window);
                Frame frame = new Frame();
                frame.Navigate(typeof(ShowImagePage), image);
                window.Content = frame;
                ThemeHelper.Initialize(window);
                window.Activate();
                ViewId = ApplicationView.GetForCurrentView().Id;
            });
            _ = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(ViewId);
        }
    }

    internal static partial class UIHelper
    {
        public static async Task OpenLinkAsync(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return; }

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    _ = ShowImageAsync(new ImageModel(origin, ImageType.SmallImage));
                    return;
                }
                else
                {
                    Regex coolapk = new Regex(@"\w*?.?coolapk.\w*/");
                    if (coolapk.IsMatch(link))
                    {
                        link = coolapk.Replace(link, string.Empty);
                    }
                    else
                    {
                        Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
                        return;
                    }
                }
            }

            if (link.FirstOrDefault() != '/')
            {
                link = $"/{link}";
            }

            if (link == "/contacts/fans")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
            }
            else if (link == "/user/myFollowList")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Replace("/page?", string.Empty);
                Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Replace("/u/", string.Empty);
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
                else
                {
                    ShowMessage("暂不支持");
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new QuestionViewModel(id));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                int end = link.IndexOf('?');
                string tag = end > 3 ? link.Substring(3, end - 3) : link.Substring(3);
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5);
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    Navigate(typeof(AdaptivePage), new AdaptiveViewModel(link));
                }
                else
                {
                    string tag = link.Substring(9);
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag);
                    if (provider != null)
                    {
                        Navigate(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(12);
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                Navigate(typeof(HTMLPage), new HTMLViewModel(origin));
            }
            else
            {
                Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
            }
        }
    }

    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }
}
