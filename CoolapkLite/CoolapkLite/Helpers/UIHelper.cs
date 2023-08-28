using CoolapkLite.Common;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static CoreDispatcher ShellDispatcher { get; set; }
        public static List<string> MessageList { get; } = new List<string>();
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
    }

    internal static partial class UIHelper
    {
        public static IHaveTitleBar AppTitle;

        public static async void ShowProgressBar()
        {
            IsShowingProgressBar = true;
            if (!AppTitle.Dispatcher.HasThreadAccess)
            {
                await AppTitle.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                AppTitle?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                AppTitle?.ShowProgressBar();
            }
        }

        public static async void ShowProgressBar(double value)
        {
            IsShowingProgressBar = true;
            if (!AppTitle.Dispatcher.HasThreadAccess)
            {
                await AppTitle.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                AppTitle?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                AppTitle?.ShowProgressBar(value);
            }
        }

        public static async void PausedProgressBar()
        {
            IsShowingProgressBar = true;
            if (!AppTitle.Dispatcher.HasThreadAccess)
            {
                await AppTitle.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            AppTitle?.PausedProgressBar();
        }

        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (!AppTitle.Dispatcher.HasThreadAccess)
            {
                await AppTitle.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            AppTitle?.ErrorProgressBar();
        }

        public static async void HideProgressBar()
        {
            IsShowingProgressBar = false;
            if (!AppTitle.Dispatcher.HasThreadAccess)
            {
                await AppTitle.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            AppTitle?.HideProgressBar();
        }

        public static async void ShowMessage(string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                if (!AppTitle.Dispatcher.HasThreadAccess)
                {
                    await AppTitle.Dispatcher.ResumeForegroundAsync();
                }
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
                    }
                }
                IsShowingMessage = false;
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowMessage("无法连接网络。"); }
            else { ShowMessage($"请检查网络连接。 {e.Message}"); }
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
            _ = builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { _ = builder.AppendLine($"Message: {ex.Message}"); }
            _ = builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { _ = builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { _ = builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken = default)
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
            }, cancellationToken);
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

        public static CoreDispatcher TryGetForCurrentCoreDispatcher()
        {
            try
            {
                return CoreApplication.GetCurrentView().Dispatcher;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UIHelper)).Warn(ex.ExceptionToMessage(), ex);
                return Window.Current?.Dispatcher ?? CoreApplication.MainView.Dispatcher;
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
        public static Task<bool> NavigateAsync(this DependencyObject element, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            IHaveTitleBar mainPage = element is IHaveTitleBar page ? page : element.FindAscendant<MainPage>() ?? element.FindAscendant<PivotPage>() ?? AppTitle;
            return mainPage.MainFrame.NavigateAsync(pageType, parameter, infoOverride);
        }

        public static Task<bool> NavigateAsync(this IHaveTitleBar mainPage, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null) =>
            mainPage.MainFrame.NavigateAsync(pageType, parameter, infoOverride);

        public static async Task<bool> NavigateAsync(this Frame frame, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            try
            {
                if (!frame.Dispatcher.HasThreadAccess)
                { await frame.Dispatcher.ResumeForegroundAsync(); }
                return infoOverride is null
                    ? frame.Navigate(pageType, parameter)
                    : frame.Navigate(pageType, parameter, infoOverride);
            }
            catch (Exception e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UIHelper)).Error(e.ExceptionToMessage(), e);
                return false;
            }
        }

        public static Task<bool> ShowImageAsync(this DependencyObject element, ImageModel image)
        {
            IHaveTitleBar mainPage = element is IHaveTitleBar page ? page : element.FindAscendant<MainPage>() ?? element.FindAscendant<PivotPage>() ?? AppTitle;
            return mainPage.ShowImageAsync(image);
        }

        public static async Task<bool> ShowImageAsync(this IHaveTitleBar mainPage, ImageModel image)
        {
            if (!mainPage.Dispatcher.HasThreadAccess)
            { await mainPage.Dispatcher.ResumeForegroundAsync(); }
            if (SettingsHelper.Get<bool>(SettingsHelper.IsUseMultiWindow))
            {
                if (WindowHelper.IsAppWindowSupported && SettingsHelper.Get<bool>(SettingsHelper.IsUseAppWindow))
                {
                    (AppWindow window, Frame frame) = await WindowHelper.CreateWindow();
                    if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                    {
                        window.TitleBar.ExtendsContentIntoTitleBar = true;
                    }
                    ThemeHelper.Initialize(window);
                    frame.Navigate(typeof(ShowImagePage), image, new DrillInNavigationTransitionInfo());
                    return await window.TryShowAsync();
                }
                else
                {
                    return await WindowHelper.CreateWindow((window) =>
                    {
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                        {
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        }
                        Frame frame = new Frame();
                        window.Content = frame;
                        ThemeHelper.Initialize(window);
                        frame.Navigate(typeof(ShowImagePage), image.Clone(window.Dispatcher), new DrillInNavigationTransitionInfo());
                    });
                }
            }
            else
            {
                return (mainPage as Page).Frame.Navigate(typeof(ShowImagePage), image, new DrillInNavigationTransitionInfo());
            }
        }
    }

    internal static partial class UIHelper
    {
        public static Task<bool> OpenLinkAsync(this DependencyObject element, string link)
        {
            IHaveTitleBar mainPage = element is IHaveTitleBar page ? page : element.FindAscendant<MainPage>() ?? element.FindAscendant<PivotPage>() ?? AppTitle;
            return mainPage.MainFrame.OpenLinkAsync(link);
        }

        public static Task<bool> OpenLinkAsync(this IHaveTitleBar mainPage, string link) =>
            mainPage.MainFrame.OpenLinkAsync(link);

        public static async Task<bool> OpenLinkAsync(this Frame frame, string link)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();

            if (string.IsNullOrWhiteSpace(link)) { return false; }

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    return await frame.ShowImageAsync(new ImageModel(origin, ImageType.SmallImage));
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
                        return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin));
                    }
                }
            }
            else if (link.StartsWith("coolapk://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(10);
            }
            else if (link.StartsWith("coolmarket://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(13);
            }

            if (link.FirstOrDefault() != '/')
            {
                link = $"/{link}";
            }

            if (link == "/contacts/fans")
            {
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
            }
            else if (link == "/user/myFollowList")
            {
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(6);
                return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(3, "?");
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
                else
                {
                    ShowMessage("暂不支持");
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new QuestionViewModel(id));
                }
            }
            else if (link.StartsWith("/vote/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new VoteViewModel(id));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(3, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(link));
                }
                else
                {
                    string tag = link.Substring(9, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(12, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                return await frame.NavigateAsync(typeof(HTMLPage), new HTMLViewModel(origin));
            }
            else if (origin.StartsWith("http://") || link.StartsWith("https://"))
            {
                return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin));
            }
            else if (origin.Contains("://") && origin.TryGetUri(out Uri uri))
            {
                if (!frame.Dispatcher.HasThreadAccess)
                { await frame.Dispatcher.ResumeForegroundAsync(); }
                return await Launcher.LaunchUriAsync(uri);
            }

            return false;
        }

        public static Task<bool> OpenActivatedEventArgs(this IHaveTitleBar mainPage, IActivatedEventArgs args) =>
            mainPage.MainFrame.OpenActivatedEventArgs(args);

        public static async Task<bool> OpenActivatedEventArgs(this Frame frame, IActivatedEventArgs args)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    LaunchActivatedEventArgs LaunchActivatedEventArgs = (LaunchActivatedEventArgs)args;
                    if (!string.IsNullOrWhiteSpace(LaunchActivatedEventArgs.Arguments))
                    {
                        switch (LaunchActivatedEventArgs.Arguments)
                        {
                            case "me":
                                return await frame.NavigateAsync(typeof(ProfilePage), new ProfileViewModel());
                            case "home":
                                return await frame.NavigateAsync(typeof(IndexPage), new IndexViewModel());
                            case "flags":
                                return await frame.NavigateAsync(typeof(TestPage));
                            case "circle":
                                return await frame.NavigateAsync(typeof(CirclePage));
                            case "search":
                                return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(string.Empty));
                            case "history":
                                return await frame.NavigateAsync(typeof(HistoryPage), new HistoryViewModel());
                            case "settings":
                                return await frame.NavigateAsync(typeof(SettingsPage));
                            case "favorites":
                                return await frame.NavigateAsync(typeof(BookmarkPage), new BookmarkViewModel());
                            case "extensions":
                                return await frame.NavigateAsync(typeof(ExtensionPage));
                            case "notifications":
                                return await frame.NavigateAsync(typeof(NotificationsPage));
                            default:
                                return await frame.OpenLinkAsync(LaunchActivatedEventArgs.Arguments);
                        }
                    }
                    else if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs2")
                            && LaunchActivatedEventArgs.TileActivatedInfo != null)
                    {
                        if (LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.Any())
                        {
                            string TileArguments = LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.FirstOrDefault().Arguments;
                            return !string.IsNullOrWhiteSpace(LaunchActivatedEventArgs.Arguments) && await frame.OpenLinkAsync(TileArguments);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                case ActivationKind.Protocol:
                    IProtocolActivatedEventArgs ProtocolActivatedEventArgs = (IProtocolActivatedEventArgs)args;
                    switch (ProtocolActivatedEventArgs.Uri.Host)
                    {
                        case "www.coolapk.com":
                        case "coolapk.com":
                        case "www.coolmarket.com":
                        case "coolmarket.com":
                            return await frame.OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsolutePath);
                        case "http":
                        case "https":
                            return await frame.OpenLinkAsync($"{ProtocolActivatedEventArgs.Uri.Host}:{ProtocolActivatedEventArgs.Uri.AbsolutePath}");
                        case "me":
                            return await frame.NavigateAsync(typeof(ProfilePage), new ProfileViewModel());
                        case "home":
                            return await frame.NavigateAsync(typeof(IndexPage), new IndexViewModel());
                        case "flags":
                            return await frame.NavigateAsync(typeof(TestPage));
                        case "circle":
                            return await frame.NavigateAsync(typeof(CirclePage));
                        case "search":
                            return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(ProtocolActivatedEventArgs.Uri.AbsolutePath.Length > 1 ? ProtocolActivatedEventArgs.Uri.AbsolutePath.Substring(1, ProtocolActivatedEventArgs.Uri.AbsolutePath.Length - 1) : string.Empty));
                        case "history":
                            return await frame.NavigateAsync(typeof(HistoryPage), new HistoryViewModel());
                        case "settings":
                            return await frame.NavigateAsync(typeof(SettingsPage));
                        case "favorites":
                            return await frame.NavigateAsync(typeof(BookmarkPage), new BookmarkViewModel());
                        case "extensions":
                            return await frame.NavigateAsync(typeof(ExtensionPage));
                        case "notifications":
                            return await frame.NavigateAsync(typeof(NotificationsPage));
                        default:
                            return await frame.OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsoluteUri);
                    }
                default:
                    return false;
            }
        }

        private static string Substring(this string str, int startIndex, string endString)
        {
            int end = str.IndexOf(endString);
            return end > startIndex ? str.Substring(startIndex, end - startIndex) : str.Substring(startIndex);
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
