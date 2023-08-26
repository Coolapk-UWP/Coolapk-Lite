using CoolapkLite.Controls;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
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
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(BookmarkViewModel),
                typeof(BookmarkPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public BookmarkViewModel Provider
        {
            get => (BookmarkViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public BookmarkPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is BookmarkViewModel ViewModel
                && Provider == null)
            {
                Provider = ViewModel;
                await Refresh(true);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "AddBookmark":
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
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            if (e != null) { e.Handled = true; }

            _ = this.OpenLinkAsync(element.Tag.ToString());
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness StackPanelMargin = (Thickness)Application.Current.Resources["StackPanelMargin"];
            Thickness ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            Thickness ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            ItemsStackPanel StackPanel = ListView.FindDescendant<ItemsStackPanel>();
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();

            if (StackPanel != null)
            {
                StackPanel.Margin = StackPanelMargin;
            }
            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = ScrollViewerMargin;
                ScrollViewer.Padding = ScrollViewerPadding;
            }
        }
    }
}
