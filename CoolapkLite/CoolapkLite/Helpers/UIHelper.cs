using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static double TitleBarHeight => 32;
        public static double PageTitleHeight => HasStatusBar ? 48 : 48 + TitleBarHeight;
        public static Thickness StackPanelMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness ScrollViewerPadding => new Thickness(0, -PageTitleHeight, 0, 0);

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

        private static readonly ObservableCollection<string> MessageList = new ObservableCollection<string>();

        public static bool IsDarkTheme(ElementTheme theme)
        {
            return theme == ElementTheme.Default ? Application.Current.RequestedTheme == ApplicationTheme.Dark : theme == ElementTheme.Dark;
        }

        public static async void CheckTheme()
        {
            while (Window.Current?.Content is null)
            {
                await Task.Delay(100);
            }

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                foreach (CoreApplicationView item in CoreApplication.Views)
                {
                    await item.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = SettingsHelper.Theme;
                    });
                }

                bool IsDark = IsDarkTheme(SettingsHelper.Theme);
                Color AccentColor = (Color)Application.Current.Resources["SystemChromeMediumLowColor"];

                if (HasStatusBar)
                {
                    if (IsDark)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.BackgroundColor = AccentColor;
                        statusBar.ForegroundColor = Colors.White;
                        statusBar.BackgroundOpacity = 0.8; // 透明度
                    }
                    else
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.BackgroundColor = AccentColor;
                        statusBar.ForegroundColor = Colors.Black;
                        statusBar.BackgroundOpacity = 0.8; // 透明度
                    }
                }
                else if (IsDark)
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.Black;
                }
            }
        }

        /// <summary>
        /// 显示进度条-正常
        /// </summary>
        public static async void ShowProgressBar(uint count = 0)
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                MainPage?.ShowProgressBar();
            }
        }

        /// <summary>
        /// 显示进度条-暂停
        /// </summary>
        public static async void PausedProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.PausedProgressBar();
        }

        /// <summary>
        /// 显示进度条-错误
        /// </summary>
        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.ErrorProgressBar();
        }

        /// <summary>
        /// 隐藏进度条
        /// </summary>
        public static async void HideProgressBar(int count = 0)
        {
            IsShowingProgressBar = false;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.HideProgressBar();
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
                            statusBar.ProgressIndicator.Text = $"[{MessageList.Count}]{MessageList[0].Replace("\n", " ")}";
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
                        if (!string.IsNullOrEmpty(MessageList[0]))
                        {
                            string messages = $"[{MessageList.Count}]{MessageList[0].Replace("\n", " ")}";
                            MainPage.AppTitle.Text = message;
                            await Task.Delay(3000);
                        }
                        MessageList.RemoveAt(0);
                        if (MessageList.Count == 0)
                        {
                            MainPage.AppTitle.Text = string.Empty;
                        }
                    }
                }
                IsShowingMessage = false;
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
                    _ = (MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new DrillInNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Entrance:
                    _ = (MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new EntranceNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Suppress:
                    _ = (MainPage?.HamburgerMenuFrame.Navigate(pageType, e, new SuppressNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Default:
                    _ = (MainPage?.HamburgerMenuFrame.Navigate(pageType, e));
                    break;
                default:
                    _ = (MainPage?.HamburgerMenuFrame.Navigate(pageType, e));
                    break;
            }
        }

        private static readonly ImmutableArray<string> routes = new string[]
        {
            "/u/",
        }.ToImmutableArray();

        private static bool IsFirst(this string str, int i) => str.IndexOf(routes[i], StringComparison.Ordinal) == 0;

        private static string Replace(this string str, int oldText)
        {
            return oldText == -1
                ? str.Replace("https://www.coolapk.com", string.Empty)
                : oldText == -2
                    ? str.Replace("http://www.coolapk.com", string.Empty)
                    : oldText == -3
                                    ? str.Replace("www.coolapk.com", string.Empty)
                                    : oldText < 0 ? throw new Exception($"i = {oldText}") : str.Replace(routes[oldText], string.Empty);
        }

        public static async void OpenLinkAsync(string str)
        {
            string rawstr = str;
            if (string.IsNullOrWhiteSpace(str)) { return; }
            int i = 0;
            if (str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                string uid = int.TryParse(u, out _) ? u : await Core.Helpers.NetworkHelper.GetUserIDByNameAsync(u);
                FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.UserPageList, uid);
                if (f != null)
                {
                    Navigate(typeof(FeedListPage), f);
                }
            }
        }
    }
}
