﻿using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class OtherDataTemplates : ResourceDictionary
    {
        public OtherDataTemplates() => InitializeComponent();

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
    }
}
