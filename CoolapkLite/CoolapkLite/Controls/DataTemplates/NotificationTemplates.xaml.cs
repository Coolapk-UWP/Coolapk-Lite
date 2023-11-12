using CoolapkLite.Helpers;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class NotificationTemplates : ResourceDictionary
    {
        public NotificationTemplates() => InitializeComponent();

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.Tag is MessageNotificationModel messageNotification
                ? element.NavigateAsync(typeof(ChatPage), new ChatViewModel(messageNotification.UKey, $"{messageNotification.UserName}的私信", element.Dispatcher))
                : element.OpenLinkAsync(element.Tag?.ToString());
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => FrameworkElement_Tapped(sender, null);
    }
}
