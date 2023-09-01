using CoolapkLite.Common;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
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
    public static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static List<string> MessageList { get; } = new List<string>();
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static IHaveTitleBar AppTitle { get; internal set; }

        public static async Task<IHaveTitleBar> GetAppTitleAsync()
        {
            return Window.Current is Window window ? await window.GetAppTitleAsync() : AppTitle;
        }

        public static async Task<IHaveTitleBar> GetAppTitleAsync(this Window window)
        {
            if (!window.Dispatcher.HasThreadAccess)
            {
                await window.Dispatcher.ResumeForegroundAsync();
            }
            Page page = window.Content.FindDescendant<Page>();
            return page is IHaveTitleBar appTitle ? appTitle : AppTitle;
        }

        public static async Task<IHaveTitleBar> GetAppTitleAsync(this CoreDispatcher dispatcher)
        {
            if (WindowHelper.ActiveWindows.TryGetValue(dispatcher, out Window window))
            {
                if (!window.Dispatcher.HasThreadAccess)
                {
                    await window.Dispatcher.ResumeForegroundAsync();
                }
                Page page = window.Content.FindDescendant<Page>();
                return page is IHaveTitleBar appTitle ? appTitle : AppTitle;
            }
            else
            {
                return AppTitle;
            }
        }

        public static async Task<IHaveTitleBar> GetAppTitleAsync(this DependencyObject element)
        {
            if (element is IHaveTitleBar mainPage)
            {
                return mainPage;
            }

            if (element.Dispatcher?.HasThreadAccess == false)
            {
                await element.Dispatcher.ResumeForegroundAsync();
            }

            if (WindowHelper.IsXamlRootSupported
                && element is UIElement uiElement
                && uiElement.XamlRoot != null)
            {
                Page page = uiElement.XamlRoot.Content.FindDescendant<Page>();
                return page is IHaveTitleBar appTitle ? appTitle : AppTitle;
            }

            return element.FindAscendant<Page>((x) => x is IHaveTitleBar) as IHaveTitleBar
                ?? await element.Dispatcher.GetAppTitleAsync();
        }

        public static async void ShowProgressBar(this CoreDispatcher dispatcher) => ShowProgressBar(await dispatcher.GetAppTitleAsync());

        public static async void ShowProgressBar(this DependencyObject element) => ShowProgressBar(await element.GetAppTitleAsync());

        public static async void ShowProgressBar(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            if (mainPage.Dispatcher?.HasThreadAccess == false)
            {
                await mainPage.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                mainPage?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                mainPage?.ShowProgressBar();
            }
        }

        public static async void ShowProgressBar(this CoreDispatcher dispatcher, double value) => ShowProgressBar(await dispatcher.GetAppTitleAsync(), value);

        public static async void ShowProgressBar(this DependencyObject element, double value) => ShowProgressBar(await element.GetAppTitleAsync(), value);

        public static async void ShowProgressBar(IHaveTitleBar mainPage, double value)
        {
            IsShowingProgressBar = true;
            if (mainPage.Dispatcher?.HasThreadAccess == false)
            {
                await mainPage.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                mainPage?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                mainPage?.ShowProgressBar(value);
            }
        }

        public static async void PausedProgressBar(this CoreDispatcher dispatcher) => PausedProgressBar(await dispatcher.GetAppTitleAsync());

        public static async void PausedProgressBar(this DependencyObject element) => PausedProgressBar(await element.GetAppTitleAsync());

        public static async void PausedProgressBar(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            if (mainPage.Dispatcher?.HasThreadAccess == false)
            {
                await mainPage.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            mainPage?.PausedProgressBar();
        }

        public static async void ErrorProgressBar(this CoreDispatcher dispatcher) => ErrorProgressBar(await dispatcher.GetAppTitleAsync());

        public static async void ErrorProgressBar(this DependencyObject element) => ErrorProgressBar(await element.GetAppTitleAsync());

        public static async void ErrorProgressBar(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            if (mainPage.Dispatcher?.HasThreadAccess == false)
            {
                await mainPage.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            mainPage?.ErrorProgressBar();
        }

        public static async void HideProgressBar(this CoreDispatcher dispatcher) => HideProgressBar(await dispatcher.GetAppTitleAsync());

        public static async void HideProgressBar(this DependencyObject element) => HideProgressBar(await element.GetAppTitleAsync());

        public static async void HideProgressBar(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = false;
            if (mainPage.Dispatcher?.HasThreadAccess == false)
            {
                await mainPage.Dispatcher.ResumeForegroundAsync();
            }
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            mainPage?.HideProgressBar();
        }

        public static async void ShowMessage(string message) => ShowMessage(await GetAppTitleAsync(), message);

        public static async void ShowMessage(this CoreDispatcher dispatcher, string message) => ShowMessage(await dispatcher.GetAppTitleAsync(), message);

        public static async void ShowMessage(this DependencyObject element, string message) => ShowMessage(await element.GetAppTitleAsync(), message);

        public static async void ShowMessage(IHaveTitleBar mainPage, string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                if (mainPage.Dispatcher?.HasThreadAccess == false)
                {
                    await mainPage.Dispatcher.ResumeForegroundAsync();
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
                        if (mainPage != null)
                        {
                            if (!string.IsNullOrEmpty(MessageList[0]))
                            {
                                string messages = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                                mainPage.ShowMessage(messages);
                                await Task.Delay(3000);
                            }
                            MessageList.RemoveAt(0);
                            if (MessageList.Count == 0)
                            {
                                mainPage.ShowMessage();
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

        public static async Task<T> GetValueAsync<T>(this DependencyObject element, DependencyProperty dp)
        {
            if (element.Dispatcher?.HasThreadAccess == false)
            {
                await element.Dispatcher.ResumeForegroundAsync();
            }
            return (T)element.GetValue(dp);
        }

        public static async Task SetValueAsync<T>(this DependencyObject element, DependencyProperty dp, T value)
        {
            if (element.Dispatcher?.HasThreadAccess == false)
            {
                await element.Dispatcher.ResumeForegroundAsync();
            }
            element.SetValue(dp, value);
        }
    }

    public static partial class UIHelper
    {
        public static async Task<bool> NavigateAsync(this DependencyObject element, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null) =>
            await NavigateAsync(await element.GetAppTitleAsync(), pageType, parameter, infoOverride);

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

        public static Task<bool> LaunchUriAsync(this DependencyObject element, Uri uri) =>
            element.Dispatcher.LaunchUriAsync(uri);

        public static async Task<bool> LaunchUriAsync(this CoreDispatcher dispatcher, Uri uri)
        {
            if (!dispatcher.HasThreadAccess)
            { await dispatcher.ResumeForegroundAsync(); }
            return await Launcher.LaunchUriAsync(uri);
        }

        public static async Task<bool> ShowImageAsync(this DependencyObject element, ImageModel image) =>
            await ShowImageAsync(await element.GetAppTitleAsync(), image);

        public static async Task<bool> ShowImageAsync(this IHaveTitleBar mainPage, ImageModel image)
        {
            if (!mainPage.Dispatcher.HasThreadAccess)
            { await mainPage.Dispatcher.ResumeForegroundAsync(); }
            if (SettingsHelper.Get<bool>(SettingsHelper.IsUseMultiWindow))
            {
                if (WindowHelper.IsAppWindowSupported && SettingsHelper.Get<bool>(SettingsHelper.IsUseAppWindow))
                {
                    (AppWindow window, Frame frame) = await WindowHelper.CreateWindowAsync();
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
                    return await WindowHelper.CreateWindowAsync((window) =>
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

    public static partial class UIHelper
    {
        public static async Task<bool> OpenLinkAsync(this DependencyObject element, string link) =>
            await OpenLinkAsync(await element.GetAppTitleAsync(), link);

        public static Task<bool> OpenLinkAsync(this IHaveTitleBar mainPage, string link) =>
            mainPage.MainFrame.OpenLinkAsync(link);

        public static async Task<bool> OpenLinkAsync(this Frame frame, string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return false; }

            await ThreadSwitcher.ResumeBackgroundAsync();

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    return await frame.ShowImageAsync(new ImageModel(origin, ImageType.SmallImage, frame.Dispatcher));
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
                        return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin, frame.Dispatcher));
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
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我", frame.Dispatcher));
            }
            else if (link == "/user/myFollowList")
            {
                return await frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我", frame.Dispatcher));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(6);
                return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url, frame.Dispatcher));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(3, "?");
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid, frame.Dispatcher);
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
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id, frame.Dispatcher));
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
                    return await frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new QuestionViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/vote/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return await frame.NavigateAsync(typeof(FeedShellPage), new VoteViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(3, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag, frame.Dispatcher);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag, frame.Dispatcher);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    return await frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(link, frame.Dispatcher));
                }
                else
                {
                    string tag = link.Substring(9, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(12, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id, frame.Dispatcher);
                if (provider != null)
                {
                    return await frame.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                return await frame.NavigateAsync(typeof(HTMLPage), new HTMLViewModel(origin, frame.Dispatcher));
            }
            else if (origin.StartsWith("http://") || link.StartsWith("https://"))
            {
                return await frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin, frame.Dispatcher));
            }
            else if (origin.Contains("://") && origin.TryGetUri(out Uri uri))
            {
                return await frame.LaunchUriAsync(uri);
            }

            return false;
        }

        public static Task<bool> OpenActivatedEventArgsAsync(this IHaveTitleBar mainPage, IActivatedEventArgs args) =>
            mainPage.MainFrame.OpenActivatedEventArgsAsync(args);

        public static async Task<bool> OpenActivatedEventArgsAsync(this Frame frame, IActivatedEventArgs args)
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
                                return await frame.NavigateAsync(typeof(ProfilePage));
                            case "home":
                                return await frame.NavigateAsync(typeof(IndexPage));
                            case "flags":
                                return await frame.NavigateAsync(typeof(TestPage));
                            case "circle":
                                return await frame.NavigateAsync(typeof(CirclePage));
                            case "search":
                                return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(string.Empty, frame.Dispatcher));
                            case "history":
                                return await frame.NavigateAsync(typeof(HistoryPage));
                            case "settings":
                                return await frame.NavigateAsync(typeof(SettingsPage));
                            case "favorites":
                                return await frame.NavigateAsync(typeof(BookmarkPage));
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
                            return await frame.NavigateAsync(typeof(ProfilePage));
                        case "home":
                            return await frame.NavigateAsync(typeof(IndexPage));
                        case "flags":
                            return await frame.NavigateAsync(typeof(TestPage));
                        case "circle":
                            return await frame.NavigateAsync(typeof(CirclePage));
                        case "search":
                            return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(ProtocolActivatedEventArgs.Uri.AbsolutePath.Length > 1 ? ProtocolActivatedEventArgs.Uri.AbsolutePath.Substring(1, ProtocolActivatedEventArgs.Uri.AbsolutePath.Length - 1) : string.Empty, frame.Dispatcher));
                        case "history":
                            return await frame.NavigateAsync(typeof(HistoryPage));
                        case "settings":
                            return await frame.NavigateAsync(typeof(SettingsPage));
                        case "favorites":
                            return await frame.NavigateAsync(typeof(BookmarkPage));
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
