﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using Windows.System;
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

            if (element.Tag is MessageNotificationModel messageNotification)
            {
                _ = element.NavigateAsync(typeof(ChatPage), new ChatViewModel(messageNotification.UKey, $"{messageNotification.UserName}的私信", element.Dispatcher));
            }
            else
            {
                _ = element.OpenLinkAsync(element.Tag?.ToString());
            }
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

        private void Button_Click(object sender, RoutedEventArgs e) => FrameworkElement_Tapped(sender, null);
    }
}
