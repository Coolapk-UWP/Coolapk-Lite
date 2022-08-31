// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenu is based on a SplitView control. By default it contains a HamburgerButton and a ListView to display menu items.
    /// </summary>
    public partial class HamburgerMenu
    {
        /// <summary>
        /// Event raised when an item is clicked
        /// </summary>
        public event ItemClickEventHandler ItemClick;

        /// <summary>
        /// Event raised when an options' item is clicked
        /// </summary>
        public event ItemClickEventHandler OptionsItemClick;

        /// <summary>
        /// Event raised when an item is invoked
        /// </summary>
        public event EventHandler<HamburgerMenuItemInvokedEventArgs> ItemInvoked;

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width >= ExpandedModeThresholdWidth)
            {
                VisualStateManager.GoToState(this, "ExpandedSize", true);
            }
            else if (Window.Current.Bounds.Width >= CompactModeThresholdWidth)
            {
                VisualStateManager.GoToState(this, "CompactSize", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "OverlaySize", true);
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            IsPaneOpen = !IsPaneOpen;
        }

        private void ButtonsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_optionsListView != null)
            {
                _optionsListView.SelectedIndex = -1;
            }

            ItemClick?.Invoke(this, e);
            ItemInvoked?.Invoke(this, new HamburgerMenuItemInvokedEventArgs() { InvokedItem = e.ClickedItem, IsItemOptions = false });
        }

        private void OptionsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_buttonsListView != null)
            {
                _buttonsListView.SelectedIndex = -1;
            }

            OptionsItemClick?.Invoke(this, e);
            ItemInvoked?.Invoke(this, new HamburgerMenuItemInvokedEventArgs() { InvokedItem = e.ClickedItem, IsItemOptions = true });
        }

        private void HamburgerListView_Loading(FrameworkElement sender, object args)
        {
            //ResourceDictionary dict = new ResourceDictionary();
            //dict.Source = new Uri("ms-appx:///Controls/HamburgerMenu/HamburgerMenuTemplate.xaml");
            //Style Style = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.RevealBrush") ? (Style)dict["HamburgerMenuItemRevealStyle"] : (Style)dict["HamburgerMenuItemStyle"];
            //if (sender is ListView ListView)
            //{
            //    ListView.ItemContainerStyle = Style;
            //}
            //else if (sender is ListViewItem ListViewItem)
            //{
            //    ListViewItem.Style = Style;
            //}
        }

        private void PaneAutoSuggestButton_Tapped(object sender, TappedRoutedEventArgs e) => IsPaneOpen = true;
    }
}
