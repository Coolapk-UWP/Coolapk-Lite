using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BookmarkPage : Page
    {
        public readonly BookmarkViewModel Provider;

        public BookmarkPage()
        {
            InitializeComponent();
            Provider = BookmarkViewModel.Caches.TryGetValue(Dispatcher, out BookmarkViewModel provider) ? provider : new BookmarkViewModel(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Provider.Bookmarks == null)
            {
                _ = Refresh(true);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case nameof(AddBookmark):
                    BookmarkDialog dialog = new BookmarkDialog();
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        Provider.Bookmarks.Add(new Bookmark(dialog.BookmarkURL, dialog.BookmarkTitle));
                        _ = Refresh();
                    }
                    break;
                case "RemoveBookmark":
                    _ = Provider.Bookmarks.Remove(element.Tag as Bookmark);
                    _ = Refresh();
                    break;
                case "NewWindow":
                    _ = Dispatcher.OpenLinkOutsideAsync(element.Tag?.ToString());
                    break;
            }
        }

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag?.ToString());
        }

        public void FrameworkElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Space:
                    FrameworkElement_Tapped(sender, null);
                    e.Handled = true;
                    break;
            }
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void FrameworkElement_RefreshEvent() => _ = Refresh(true);
    }
}
