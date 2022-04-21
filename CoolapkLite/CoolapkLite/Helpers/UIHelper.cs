﻿using CoolapkLite.Core.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using LiteDB;
using MicaForUWP.Media;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public static event EventHandler<ElementTheme> AppThemeChanged;

        public static Brush ApplicationPageBackgroundThemeWindowBrush()
        {
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
            {
                ResourceDictionary BlurBrushs = new ResourceDictionary();
                BlurBrushs.Source = new Uri("ms-appx:///Styles/MicaBrushs.xaml");
                return (BackdropMicaBrush)BlurBrushs["ApplicationPageBackgroundThemeMicaWallpaperBrush"];
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                ResourceDictionary AcrylicBrushs = new ResourceDictionary();
                AcrylicBrushs.Source = new Uri("ms-appx:///Styles/AcrylicBrushs.xaml");
                return (AcrylicBrush)AcrylicBrushs["ApplicationPageBackgroundThemeAcrylicWindowBrush"];
            }
            else if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
            {
                ResourceDictionary BlurBrushs = new ResourceDictionary();
                BlurBrushs.Source = new Uri("ms-appx:///Styles/BlurBrushs.xaml");
                return (BackdropBlurBrush)BlurBrushs["ApplicationPageBackgroundThemeBlurWindowBrush"];
            }
            else
            {
                return (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
            }
        }

        public static Brush ApplicationPageBackgroundThemeElementBrush()
        {
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
            {
                ResourceDictionary BlurBrushs = new ResourceDictionary();
                BlurBrushs.Source = new Uri("ms-appx:///Styles/MicaBrushs.xaml");
                return (BackdropMicaBrush)BlurBrushs["ApplicationPageBackgroundThemeMicaWallpaperBrush"];
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                ResourceDictionary AcrylicBrushs = new ResourceDictionary();
                AcrylicBrushs.Source = new Uri("ms-appx:///Styles/AcrylicBrushs.xaml");
                return (AcrylicBrush)AcrylicBrushs["ApplicationPageBackgroundThemeAcrylicElementBrush"];
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                ResourceDictionary BlurBrushs = new ResourceDictionary();
                BlurBrushs.Source = new Uri("ms-appx:///Styles/BlurBrushs.xaml");
                return (BackdropBlurBrush)BlurBrushs["ApplicationPageBackgroundThemeBlurElementBrush"];
            }
            else
            {
                return (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
            }
        }
    }

    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static double TitleBarHeight => 32;
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

        private static readonly ObservableCollection<string> MessageList = new ObservableCollection<string>();

        static UIHelper()
        {
            AppThemeChanged += (_, __) => CheckTheme();
            BsonMapper.Global.RegisterType
                (
                serialize: (pic) => pic.Uri,
                deserialize: (bson) => new ImageModel(bson.ToString(), ImageType.OriginImage)
                );
        }

        public static bool IsDarkTheme() => IsDarkTheme(SettingsHelper.Theme);

        public static bool IsDarkTheme(ElementTheme theme) => theme == ElementTheme.Default ? Application.Current.RequestedTheme == ApplicationTheme.Dark : theme == ElementTheme.Dark;

        public static async void ChangeTheme()
        {
            while (Window.Current?.Content is null)
            {
                await Task.Delay(100);
            }

            if (Window.Current?.Content is FrameworkElement frameworkElement)
            {
                foreach (CoreApplicationView item in CoreApplication.Views)
                {
                    await item.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = SettingsHelper.Theme;
                    });
                }
            }

            AppThemeChanged?.Invoke(Window.Current?.Content, SettingsHelper.Theme);
        }

        public static void CheckTheme()
        {
            bool IsDark = IsDarkTheme(SettingsHelper.Theme);
            CheckTheme(IsDark, false);
        }

        public static void CheckTheme(bool IsDark, bool IsInvoke = false)
        {
            Color ForegroundColor = IsDark ? Colors.White : Colors.Black;
            Color BackgroundColor = new AccessibilitySettings().HighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            if (HasStatusBar)
            {
                StatusBar StatusBar = StatusBar.GetForCurrentView();
                StatusBar.ForegroundColor = ForegroundColor;
                StatusBar.BackgroundColor = BackgroundColor;
                StatusBar.BackgroundOpacity = 0; // 透明度
            }
            else
            {
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = HasTitleBar ? BackgroundColor : Colors.Transparent;
            }

            if (IsInvoke) { AppThemeChanged?.Invoke(Window.Current?.Content, SettingsHelper.Theme); }
        }
    }

    internal static partial class UIHelper
    {
        public static IHaveTitleBar AppTitle;

        public static async void ShowProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                AppTitle?.ShowProgressBar();
            }
        }

        public static async void ShowProgressBar(double value = 0)
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
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
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            AppTitle?.PausedProgressBar();
        }

        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            AppTitle?.ErrorProgressBar();
        }

        public static async void HideProgressBar()
        {
            IsShowingProgressBar = false;
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
                    else if (AppTitle != null)
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
                IsShowingMessage = false;
            }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            bool r = false;
            DependencyObject DependencyObject = originalSource as DependencyObject;
            if (DependencyObject.FindAscendant<Button>() == null && DependencyObject.FindAscendant<AppBarButton>() == null && originalSource.GetType() != typeof(Button) && originalSource.GetType() != typeof(AppBarButton) && originalSource.GetType() != typeof(RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    r = source == DependencyObject.FindAscendantByName(FrameworkElement.Name);
                }
            }
            return source == originalSource || r;
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
            "/feed/",
            "/picture/",
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
                string uid = int.TryParse(u, out _) ? u : await NetworkHelper.GetUserIDByNameAsync(u);
                FeedListViewModel f = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (f != null)
                {
                    Navigate(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++) || str.IsFirst(i++))
            {
                if (str == "/feed/writer") { ShowMessage("暂不支持"); }
                else { Navigate(typeof(FeedShellPage), new FeedDetailViewModel(str.Replace(i - 1))); }
            }
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
}
