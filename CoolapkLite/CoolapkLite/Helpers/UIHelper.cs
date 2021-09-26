using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

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
                        statusBar.BackgroundOpacity = 0; // 透明度
                    }
                    else
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.BackgroundColor = AccentColor;
                        statusBar.ForegroundColor = Colors.Black;
                        statusBar.BackgroundOpacity = 0; // 透明度
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
    }
}
