using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class ProfileCardTemplates : ResourceDictionary
    {
        public ProfileCardTemplates() => InitializeComponent();

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if (e != null) { e.Handled = true; }

            OnTapped(element, element.Tag);
        }

        public void FrameworkElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Space:
                    if (!(sender is FrameworkElement element)) { return; }
                    OnTapped(element, element.Tag);
                    e.Handled = true;
                    break;
            }
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            OnTapped(element, element.Tag);
        }

        private void OnTapped(FrameworkElement element, object tag)
        {
            string url = tag is string str ? str : tag is IHasTitle model ? model.Url : null;
            if (string.IsNullOrEmpty(url) || url == "/topic/quickList?quickType=list")
            {
                return;
            }
            else if (url == "Login")
            {
                _ = element.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri, element.Dispatcher));
            }
            else if (url.StartsWith("/page", StringComparison.Ordinal))
            {
                url = $"{url.Replace("/page", "/page/dataList")}&title={(tag as IHasTitle)?.Title ?? string.Empty}";
                _ = element.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url, element.Dispatcher));
            }
            else if (url.StartsWith("#"))
            {
                _ = element.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel($"{url}&title={(tag as IHasTitle)?.Title ?? string.Empty}", element.Dispatcher));
            }
            else if (url.Contains("我的常去"))
            {
                _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去", element.Dispatcher));
            }
            else if (url.Contains("浏览历史"))
            {
                _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史", element.Dispatcher));
            }
            else if (url.Contains("我关注的话题"))
            {
                _ = element.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList", element.Dispatcher));
            }
            else if (url.Contains("我的收藏单"))
            {
                string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (uid != null) { _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserCollectionListProvider(uid, element.Dispatcher)); }
            }
            else if (url.Contains("我的问答"))
            {
                string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (uid != null) { _ = element.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer", element.Dispatcher)); }
            }
            else
            {
                _ = element.OpenLinkAsync(url);
            }
        }
    }
}
