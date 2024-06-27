using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
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
        public static Queue<string> MessageQueue { get; } = new Queue<string>();
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInfoHelper.IsStatusBarSupported;

        public static IHaveTitleBar AppTitle { get; internal set; }

        public static async Task<IHaveTitleBar> GetAppTitleAsync() =>
            Window.Current is Window window ? await window.GetAppTitleAsync().ConfigureAwait(false) : AppTitle;

        public static async Task<IHaveTitleBar> GetAppTitleAsync(this Window window)
        {
            await window.Dispatcher.ResumeForegroundAsync();
            Page page = window.Content.FindDescendant<Page>();
            return page is IHaveTitleBar appTitle ? appTitle : AppTitle;
        }

        public static async Task<IHaveTitleBar> GetAppTitleAsync(this CoreDispatcher dispatcher)
        {
            if (WindowHelper.ActiveWindows.TryGetValue(dispatcher, out Window window))
            {
                await window.Dispatcher.ResumeForegroundAsync();
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

            await element.Dispatcher.ResumeForegroundAsync();

            if (WindowHelper.IsXamlRootSupported
                && element is UIElement uiElement
                && uiElement.XamlRoot != null)
            {
                Page page = uiElement.XamlRoot.Content.FindDescendant<Page>();
                return page is IHaveTitleBar appTitle ? appTitle : AppTitle;
            }

            return element.FindAscendant<Page>(x => x is IHaveTitleBar) as IHaveTitleBar
                ?? await element.Dispatcher.GetAppTitleAsync().ConfigureAwait(false);
        }

        public static Task ShowProgressBarAsync(this CoreDispatcher dispatcher) => dispatcher.GetAppTitleAsync().ContinueWith(x => ShowProgressBarAsync(x.Result)).Unwrap();

        public static Task ShowProgressBarAsync(this DependencyObject element) => element.GetAppTitleAsync().ContinueWith(x => ShowProgressBarAsync(x.Result)).Unwrap();

        public static async Task ShowProgressBarAsync(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            await mainPage.Dispatcher.ResumeForegroundAsync();
            if (HasStatusBar)
            {
                if (mainPage != null) { await mainPage.HideProgressBarAsync(); }
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await mainPage.ShowProgressBarAsync();
            }
        }

        public static Task ShowProgressBarAsync(this CoreDispatcher dispatcher, double value) => dispatcher.GetAppTitleAsync().ContinueWith(x => ShowProgressBarAsync(x.Result, value)).Unwrap();

        public static Task ShowProgressBarAsync(this DependencyObject element, double value) => element.GetAppTitleAsync().ContinueWith(x => ShowProgressBarAsync(x.Result, value));

        public static async Task ShowProgressBarAsync(IHaveTitleBar mainPage, double value)
        {
            IsShowingProgressBar = true;
            await mainPage.Dispatcher.ResumeForegroundAsync();
            if (HasStatusBar)
            {
                if (mainPage != null) { await mainPage.HideProgressBarAsync(); }
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await mainPage.ShowProgressBarAsync(value);
            }
        }

        public static Task PausedProgressBarAsync(this CoreDispatcher dispatcher) => dispatcher.GetAppTitleAsync().ContinueWith(x => PausedProgressBarAsync(x.Result)).Unwrap();

        public static Task PausedProgressBarAsync(this DependencyObject element) => element.GetAppTitleAsync().ContinueWith(x => PausedProgressBarAsync(x.Result)).Unwrap();

        public static async Task PausedProgressBarAsync(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            await mainPage.Dispatcher.ResumeForegroundAsync();
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await mainPage.PausedProgressBarAsync();
        }

        public static Task ErrorProgressBarAsync(this CoreDispatcher dispatcher) => dispatcher.GetAppTitleAsync().ContinueWith(x => ErrorProgressBarAsync(x.Result)).Unwrap();

        public static Task ErrorProgressBarAsync(this DependencyObject element) => element.GetAppTitleAsync().ContinueWith(x => ErrorProgressBarAsync(x.Result)).Unwrap();

        public static async Task ErrorProgressBarAsync(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = true;
            await mainPage.Dispatcher.ResumeForegroundAsync();
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await mainPage.ErrorProgressBarAsync();
        }

        public static Task HideProgressBarAsync(this CoreDispatcher dispatcher) => dispatcher.GetAppTitleAsync().ContinueWith(x => HideProgressBarAsync(x.Result)).Unwrap();

        public static Task HideProgressBarAsync(this DependencyObject element) => element.GetAppTitleAsync().ContinueWith(x => HideProgressBarAsync(x.Result)).Unwrap();

        public static async Task HideProgressBarAsync(IHaveTitleBar mainPage)
        {
            IsShowingProgressBar = false;
            await mainPage.Dispatcher.ResumeForegroundAsync();
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await mainPage.HideProgressBarAsync();
        }

        public static Task ShowMessageAsync(string message) => GetAppTitleAsync().ContinueWith(x => ShowMessageAsync(x.Result, message)).Unwrap();

        public static Task ShowMessageAsync(this CoreDispatcher dispatcher, string message) => dispatcher.GetAppTitleAsync().ContinueWith(x => ShowMessageAsync(x.Result, message)).Unwrap();

        public static Task ShowMessageAsync(this DependencyObject element, string message) => element.GetAppTitleAsync().ContinueWith(x => ShowMessageAsync(x.Result, message)).Unwrap();

        public static async Task ShowMessageAsync(IHaveTitleBar mainPage, string message)
        {
            MessageQueue.Enqueue(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                await mainPage.Dispatcher.ResumeForegroundAsync();
                if (HasStatusBar)
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    while (MessageQueue.Count > 0)
                    {
                        message = MessageQueue.Dequeue();
                        if (!string.IsNullOrEmpty(message))
                        {
                            statusBar.ProgressIndicator.Text = $"[{MessageQueue.Count + 1}] {message.Replace("\n", " ")}";
                            statusBar.ProgressIndicator.ProgressValue = IsShowingProgressBar ? null : (double?)0;
                            await statusBar.ProgressIndicator.ShowAsync();
                            await Task.Delay(Duration);
                        }
                    }
                    if (!IsShowingProgressBar) { await statusBar.ProgressIndicator.HideAsync(); }
                    statusBar.ProgressIndicator.Text = string.Empty;
                }
                else if (mainPage != null)
                {
                    while (MessageQueue.Count > 0)
                    {
                        message = MessageQueue.Dequeue();
                        if (!string.IsNullOrEmpty(message))
                        {
                            string messages = $"[{MessageQueue.Count + 1}] {message.Replace("\n", " ")}";
                            await mainPage.ShowMessageAsync(messages);
                            await Task.Delay(Duration);
                        }
                    }
                    await mainPage.ShowMessageAsync();
                }
                IsShowingMessage = false;
            }
        }
    }

    public static partial class UIHelper
    {
        public static Task ShowHttpExceptionMessageAsync(HttpRequestException e)
        {
            return e.Message.IndexOfAny(new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1
                ? ShowMessageAsync($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}")
                : e.Message == "An error occurred while sending the request."
                    ? ShowMessageAsync("无法连接网络。")
                    : ShowMessageAsync($"请检查网络连接。 {e.Message}");
        }

        public static void SetBadgeNumber(uint number)
        {
            BadgeNumericContent content = new BadgeNumericContent(number);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(content.GetXml());
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static void SetBadgeNumber(BadgeGlyphValue glyph)
        {
            BadgeGlyphContent content = new BadgeGlyphContent(glyph);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(content.GetXml());
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static string ExceptionToMessage(this Exception ex) =>
            new StringBuilder().AppendLine()
                               .TryAppendLineFormat("Message: {0}", ex.Message)
                               .AppendLineFormat("HResult: {0} (0x{1:X})", ex.HResult, ex.HResult)
                               .TryAppendLine(ex.StackTrace)
                               .TryAppendLineFormat("HelperLink: {0}", ex.HelpLink)
                               .ToString();

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
            await element.Dispatcher.ResumeForegroundAsync();
            return (T)element.GetValue(dp);
        }

        public static async Task SetValueAsync<T>(this DependencyObject element, DependencyProperty dp, T value)
        {
            await element.Dispatcher.ResumeForegroundAsync();
            element.SetValue(dp, value);
        }

        public static JumpListItem AddGroupNameAndLogo(this JumpListItem item, string groupName, Uri uri)
        {
            item.GroupName = groupName;
            item.Logo = uri;
            return item;
        }
    }

    public static partial class UIHelper
    {
        public static Task<bool> NavigateAsync(this DependencyObject element, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null) =>
            element.GetAppTitleAsync().ContinueWith(x => x.Result.MainFrame.NavigateAsync(pageType, parameter, infoOverride)).Unwrap();

        public static Task<bool> NavigateAsync(this IHaveTitleBar mainPage, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null) =>
            mainPage.MainFrame.NavigateAsync(pageType, parameter, infoOverride);

        public static async Task<bool> NavigateAsync(this Frame frame, Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            try
            {
                await frame.Dispatcher.ResumeForegroundAsync();
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

        public static async Task<bool> NavigateOutsideAsync(this CoreDispatcher dispatcher, Type pageType, Func<CoreDispatcher, object> parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            try
            {
                OpenLinkFactory factory = frame => frame.NavigateAsync(pageType, parameter(frame.Dispatcher), infoOverride);

                await dispatcher.ResumeForegroundAsync();

                if (WindowHelper.IsAppWindowSupported && SettingsHelper.Get<bool>(SettingsHelper.IsUseAppWindow))
                {
                    (AppWindow window, Frame frame) = await WindowHelper.CreateWindowAsync();
                    if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                    {
                        window.TitleBar.ExtendsContentIntoTitleBar = true;
                    }
                    ThemeHelper.Initialize(window);
                    Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                    _ = frame.Navigate(page, factory, new DrillInNavigationTransitionInfo());
                    bool result = await window.TryShowAsync();
                    _ = dispatcher.HideProgressBarAsync();
                    return result;
                }
                else
                {
                    bool result = await WindowHelper.CreateWindowAsync(window =>
                    {
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                        {
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        }
                        Frame frame = new Frame();
                        window.Content = frame;
                        ThemeHelper.Initialize(window);
                        Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                        _ = frame.Navigate(page, factory, new DrillInNavigationTransitionInfo());
                    });
                    _ = HideProgressBarAsync(AppTitle);
                    return result;
                }
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
            await dispatcher.ResumeForegroundAsync();
            return await Launcher.LaunchUriAsync(uri);
        }

        public static async Task<bool> LaunchFileAsync(this CoreDispatcher dispatcher, IStorageFile file)
        {
            await dispatcher.ResumeForegroundAsync();
            return await Launcher.LaunchFileAsync(file);
        }

        public static Task<bool> ShowImageAsync(this DependencyObject element, ImageModel image) =>
            element.GetAppTitleAsync().ContinueWith(x => x.Result.ShowImageAsync(image)).Unwrap();

        public static async Task<bool> ShowImageAsync(this IHaveTitleBar mainPage, ImageModel image)
        {
            await mainPage.Dispatcher.ResumeForegroundAsync();
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
                    _ = frame.Navigate(typeof(ShowImagePage), image, new DrillInNavigationTransitionInfo());
                    return await window.TryShowAsync();
                }
                else
                {
                    return await WindowHelper.CreateWindowAsync(window =>
                    {
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                        {
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        }
                        Frame frame = new Frame();
                        window.Content = frame;
                        ThemeHelper.Initialize(window);
                        _ = frame.Navigate(typeof(ShowImagePage), image.Clone(window.Dispatcher), new DrillInNavigationTransitionInfo());
                    }).ConfigureAwait(false);
                }
            }
            else
            {
                return (mainPage as Page).Frame.Navigate(typeof(ShowImagePage), image, new DrillInNavigationTransitionInfo());
            }
        }
    }

    public delegate Task<bool> OpenLinkFactory(Frame frame);

    public static partial class UIHelper
    {
        public static Task<bool> OpenLinkAsync(this DependencyObject element, string link) =>
            element.GetAppTitleAsync().ContinueWith(x => x.Result.MainFrame.OpenLinkAsync(link)).Unwrap();

        public static Task<bool> OpenLinkAsync(this IHaveTitleBar mainPage, string link) =>
            mainPage.MainFrame.OpenLinkAsync(link);

        public static async Task<bool> OpenLinkAsync(this Frame frame, string link) =>
            !string.IsNullOrWhiteSpace(link)
                && await TryGetOpenLinkFactory(link) is OpenLinkFactory factory
                && await factory(frame);

        public static async Task<bool> OpenLinkOutsideAsync(this CoreDispatcher dispatcher, string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return false; }

            if (!(await TryGetOpenLinkFactory(link) is OpenLinkFactory factory)) { return false; }

            await dispatcher.ResumeForegroundAsync();

            if (WindowHelper.IsAppWindowSupported && SettingsHelper.Get<bool>(SettingsHelper.IsUseAppWindow))
            {
                (AppWindow window, Frame frame) = await WindowHelper.CreateWindowAsync();
                if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                {
                    window.TitleBar.ExtendsContentIntoTitleBar = true;
                }
                ThemeHelper.Initialize(window);
                Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                _ = frame.Navigate(page, factory, new DrillInNavigationTransitionInfo());
                bool result = await window.TryShowAsync();
                _ = dispatcher.HideProgressBarAsync();
                return result;
            }
            else
            {
                bool result = await WindowHelper.CreateWindowAsync(window =>
                {
                    if (SettingsHelper.Get<bool>(SettingsHelper.IsExtendsTitleBar))
                    {
                        CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                    }
                    Frame frame = new Frame();
                    window.Content = frame;
                    ThemeHelper.Initialize(window);
                    Type page = SettingsHelper.Get<bool>(SettingsHelper.IsUseLiteHome) ? typeof(PivotPage) : typeof(MainPage);
                    _ = frame.Navigate(page, factory, new DrillInNavigationTransitionInfo());
                });
                _ = HideProgressBarAsync(AppTitle);
                return result;
            }
        }

        public static async Task<OpenLinkFactory> TryGetOpenLinkFactory(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return null; }

            await ThreadSwitcher.ResumeBackgroundAsync();

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com", StringComparison.OrdinalIgnoreCase))
                {
                    return frame => frame.ShowImageAsync(new ImageModel(origin, ImageType.SmallImage, frame.Dispatcher));
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
                        return frame => frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin, frame.Dispatcher));
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
                return frame => frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我", frame.Dispatcher));
            }
            else if (link == "/user/myFollowList")
            {
                return frame => frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我", frame.Dispatcher));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(6);
                return frame => frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url, frame.Dispatcher));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string url = link.Substring(3, "?");
                    string uid = int.TryParse(url, out _) ? url : await NetworkHelper.GetUserInfoByNameAsync(url).ContinueWith(x => x.Result.UID.ToString());
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/feed/changeHistoryList", StringComparison.OrdinalIgnoreCase))
                {
                    return frame => frame.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetSinglePageProvider(link, "动态编辑记录", frame.Dispatcher));
                }
                else
                {
                    string id = link.Substring(6, "?");
                    if (int.TryParse(id, out _))
                    {
                        return async frame =>
                        {
                            FeedShellViewModel provider = await FeedShellViewModel.GetProviderAsync(id, frame.Dispatcher).ConfigureAwait(false);
                            if (provider != null)
                            {
                                return await frame.NavigateAsync(typeof(FeedShellPage), provider).ConfigureAwait(false);
                            }
                            return false;
                        };
                    }
                    else
                    {
                        _ = ShowMessageAsync("暂不支持");
                    }
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(9, "?");
                if (int.TryParse(id, out _))
                {
                    return frame => frame.NavigateAsync(typeof(FeedShellPage), new FeedDetailViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    return frame => frame.NavigateAsync(typeof(FeedShellPage), new QuestionViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/vote/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    return frame => frame.NavigateAsync(typeof(FeedShellPage), new VoteViewModel(id, frame.Dispatcher));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string tag = link.Substring(3, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string tag = link.Substring(5, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    return frame => frame.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(link, frame.Dispatcher));
                }
                else
                {
                    return async frame =>
                    {
                        string tag = link.Substring(9, "?");
                        FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag, frame.Dispatcher);
                        if (provider != null)
                        {
                            return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                        }
                        return false;
                    };
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string id = link.Substring(12, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/apk/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string id = link.Substring(5, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.AppPageList, id, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/appba/", StringComparison.OrdinalIgnoreCase))
            {
                return async frame =>
                {
                    string id = link.Substring(7, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.AppPageList, id, frame.Dispatcher);
                    if (provider != null)
                    {
                        return await frame.NavigateAsync(typeof(FeedListPage), provider).ConfigureAwait(false);
                    }
                    return false;
                };
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                return frame => frame.NavigateAsync(typeof(HTMLPage), new HTMLViewModel(origin, frame.Dispatcher));
            }
            else if (origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return frame => frame.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(origin, frame.Dispatcher));
            }
            else if (origin.Contains("://") && origin.TryGetUri(out Uri uri))
            {
                return frame => frame.LaunchUriAsync(uri);
            }

            return null;
        }

        public static Task<bool> OpenActivatedEventArgsAsync(this IHaveTitleBar mainPage, IActivatedEventArgs args) =>
            mainPage.MainFrame.OpenActivatedEventArgsAsync(args);

        public static async Task<bool> OpenActivatedEventArgsAsync(this Frame frame, IActivatedEventArgs args)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            switch (args.Kind)
            {
                case ActivationKind.Launch when args is LaunchActivatedEventArgs LaunchActivatedEventArgs:
                    if (!string.IsNullOrWhiteSpace(LaunchActivatedEventArgs.Arguments))
                    {
                        return await ProcessArgumentsAsync(frame, LaunchActivatedEventArgs.Arguments.Split(' ')).ConfigureAwait(false);
                    }
                    else if (ApiInfoHelper.IsTileActivatedInfoSupported && LaunchActivatedEventArgs.TileActivatedInfo != null)
                    {
                        if (LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.Count > 0)
                        {
                            string TileArguments = LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.FirstOrDefault().Arguments;
                            return !string.IsNullOrWhiteSpace(TileArguments) && await ProcessArgumentsAsync(frame, TileArguments.Split(' ')).ConfigureAwait(false);
                        }
                    }
                    return false;
                case ActivationKind.Search when args is ISearchActivatedEventArgs SearchActivatedEventArgs:
                    return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(SearchActivatedEventArgs.QueryText, frame.Dispatcher)).ConfigureAwait(false);
                case ActivationKind.ShareTarget when args is IShareTargetActivatedEventArgs ShareTargetActivatedEventArgs:
                    return ShareTargetActivatedEventArgs.ShareOperation.Data != null && await ShowCreateFeedControlAsync(frame, ShareTargetActivatedEventArgs.ShareOperation.Data).ConfigureAwait(false);
                case ActivationKind.Protocol:
                case ActivationKind.ProtocolForResults:
                    IProtocolActivatedEventArgs ProtocolActivatedEventArgs = (IProtocolActivatedEventArgs)args;
                    switch (ProtocolActivatedEventArgs.Uri.Host.ToLowerInvariant())
                    {
                        case "www.coolapk.com":
                        case "coolapk.com":
                        case "www.coolmarket.com":
                        case "coolmarket.com":
                            _ = frame.ShowProgressBarAsync();
                            return await frame.OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsolutePath).ConfigureAwait(false);
                        case "http":
                        case "https":
                            _ = frame.ShowProgressBarAsync();
                            return await frame.OpenLinkAsync($"{ProtocolActivatedEventArgs.Uri.Host}:{ProtocolActivatedEventArgs.Uri.AbsolutePath}").ConfigureAwait(false);
                        case "me":
                            return await frame.NavigateAsync(typeof(ProfilePage)).ConfigureAwait(false);
                        case "home":
                            return await frame.NavigateAsync(typeof(IndexPage)).ConfigureAwait(false);
                        case "flags":
                            return await frame.NavigateAsync(typeof(TestPage)).ConfigureAwait(false);
                        case "circle":
                            return await frame.NavigateAsync(typeof(CirclePage)).ConfigureAwait(false);
                        case "create":
                            return await frame.ShowCreateFeedControlAsync().ConfigureAwait(false);
                        case "search":
                            return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(ProtocolActivatedEventArgs.Uri.AbsolutePath.Length > 1 ? ProtocolActivatedEventArgs.Uri.AbsolutePath.Substring(1, ProtocolActivatedEventArgs.Uri.AbsolutePath.Length - 1) : string.Empty, frame.Dispatcher)).ConfigureAwait(false);
                        case "history":
                            return await frame.NavigateAsync(typeof(HistoryPage)).ConfigureAwait(false);
                        case "settings":
                            return await frame.NavigateAsync(typeof(SettingsPage)).ConfigureAwait(false);
                        case "favorites":
                            return await frame.NavigateAsync(typeof(BookmarkPage)).ConfigureAwait(false);
                        case "extensions":
                            return await frame.NavigateAsync(typeof(ExtensionPage)).ConfigureAwait(false);
                        case "notifications":
                            return await frame.NavigateAsync(typeof(NotificationsPage)).ConfigureAwait(false);
                        default:
                            _ = frame.ShowProgressBarAsync();
                            return await frame.OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsoluteUri).ConfigureAwait(false);
                    }
                case ActivationKind.ToastNotification when args is IToastNotificationActivatedEventArgs ToastNotificationActivatedEventArgs:
                    ToastArguments arguments = ToastArguments.Parse(ToastNotificationActivatedEventArgs.Argument);
                    if (arguments.TryGetValue("action", out string action))
                    {
                        switch (action)
                        {
                            case "hasUpdate":
                                if (arguments.TryGetValue("url", out string url) && url.TryGetUri(out Uri uri))
                                {
                                    _ = frame.LaunchUriAsync(uri);
                                }
                                break;
                            case "hasNotification":
                                return await frame.NavigateAsync(typeof(NotificationsPage)).ConfigureAwait(false);
                        }
                    }
                    return false;
                case (ActivationKind)1021 when ApiInfoHelper.IsICommandLineActivatedEventArgsSupported
                                            && args is ICommandLineActivatedEventArgs CommandLineActivatedEventArgs:
                    return !string.IsNullOrWhiteSpace(CommandLineActivatedEventArgs.Operation.Arguments)
                            && await ProcessArgumentsAsync(frame, CommandLineActivatedEventArgs.Operation.Arguments.Split(' ')).ConfigureAwait(false);
                default:
                    return false;
            }
        }

        private static async Task<bool> ProcessArgumentsAsync(Frame frame, params string[] arguments)
        {
            if (arguments?.Length > 0)
            {
                switch (arguments[0].ToLowerInvariant())
                {
                    case "me":
                        return await frame.NavigateAsync(typeof(ProfilePage)).ConfigureAwait(false);
                    case "home":
                        return await frame.NavigateAsync(typeof(IndexPage)).ConfigureAwait(false);
                    case "flags":
                        return await frame.NavigateAsync(typeof(TestPage)).ConfigureAwait(false);
                    case "caches":
                        return await frame.NavigateAsync(typeof(CachesPage)).ConfigureAwait(false);
                    case "circle":
                        return await frame.NavigateAsync(typeof(CirclePage)).ConfigureAwait(false);
                    case "create":
                        return await frame.ShowCreateFeedControlAsync().ConfigureAwait(false);
                    case "search":
                        return await frame.NavigateAsync(typeof(SearchingPage), new SearchingViewModel(arguments.Length >= 2 ? arguments[1] : string.Empty, frame.Dispatcher)).ConfigureAwait(false);
                    case "history":
                        return await frame.NavigateAsync(typeof(HistoryPage)).ConfigureAwait(false);
                    case "settings":
                        return await frame.NavigateAsync(typeof(SettingsPage)).ConfigureAwait(false);
                    case "favorites":
                        return await frame.NavigateAsync(typeof(BookmarkPage)).ConfigureAwait(false);
                    case "extensions" when ExtensionManager.IsOSSUploaderSupported:
                        return await frame.NavigateAsync(typeof(ExtensionPage)).ConfigureAwait(false);
                    case "notifications":
                        return await frame.NavigateAsync(typeof(NotificationsPage)).ConfigureAwait(false);
                    default:
                        _ = frame.ShowProgressBarAsync();
                        return await frame.OpenLinkAsync(arguments[0]).ConfigureAwait(false);
                }
            }
            return false;
        }

        private static async Task<bool> ShowCreateFeedControlAsync(this UIElement element)
        {
            await element.Dispatcher.ResumeForegroundAsync();
            new CreateFeedControl
            {
                FeedType = CreateFeedType.Feed,
                PopupTransitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                }
            }.Show(element);
            return false;
        }

        private static async Task<bool> ShowCreateFeedControlAsync(this UIElement element, DataPackageView data)
        {
            await element.Dispatcher.ResumeForegroundAsync();
            CreateFeedControl control = new CreateFeedControl
            {
                FeedType = CreateFeedType.Feed,
                PopupTransitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                }
            };
            _ = control.Provider.DropFileAsync(data);
            control.Show(element);
            return false;
        }

        private static string Substring(this string str, int startIndex, string endString)
        {
            int end = str.IndexOf(endString);
            return end > startIndex ? str.Substring(startIndex, end - startIndex) : str.Substring(startIndex);
        }
    }
}
