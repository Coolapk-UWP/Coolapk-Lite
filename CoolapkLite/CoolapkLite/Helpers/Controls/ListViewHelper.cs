using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class ListViewHelper
    {
        #region ItemsPanelMargin

        public static Thickness GetItemsPanelMargin(FrameworkElement element)
        {
            return (Thickness)element.GetValue(ItemsPanelMarginProperty);
        }

        public static void SetItemsPanelMargin(FrameworkElement element, Thickness value)
        {
            element.SetValue(ItemsPanelMarginProperty, value);
        }

        public static readonly DependencyProperty ItemsPanelMarginProperty =
            DependencyProperty.RegisterAttached(
                "ItemsPanelMargin",
                typeof(Thickness),
                typeof(ListViewHelper),
                new PropertyMetadata(null, OnItemsPanelMarginChanged));

        private static void OnItemsPanelMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement Element = (FrameworkElement)d;
            if (Element.IsLoaded == false)
            {
                Element.Loaded += (sender, arg) =>
                {
                    Thickness Margin = GetItemsPanelMargin(Element);
                    ItemsStackPanel ItemsStackPanel = Element?.FindDescendant<ItemsStackPanel>();
                    if (ItemsStackPanel != null) { ItemsStackPanel.Margin = Margin; }
                };
            }
            else
            {
                Thickness Margin = GetItemsPanelMargin(Element);
                ItemsStackPanel ItemsStackPanel = Element?.FindDescendant<ItemsStackPanel>();
                if (ItemsStackPanel != null) { ItemsStackPanel.Margin = Margin; }
            }
        }

        #endregion

        #region ItemsPanelMargin

        public static HorizontalAlignment GetItemsPanelHorizontalAlignment(FrameworkElement element)
        {
            return (HorizontalAlignment)element.GetValue(ItemsPanelHorizontalAlignmentProperty);
        }

        public static void SetItemsPanelHorizontalAlignment(FrameworkElement element, HorizontalAlignment value)
        {
            element.SetValue(ItemsPanelHorizontalAlignmentProperty, value);
        }

        public static readonly DependencyProperty ItemsPanelHorizontalAlignmentProperty =
            DependencyProperty.RegisterAttached(
                "ItemsPanelHorizontalAlignment",
                typeof(HorizontalAlignment),
                typeof(ListViewHelper),
                new PropertyMetadata(null, OnItemsPanelHorizontalAlignmentChanged));

        private static void OnItemsPanelHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement Element = (FrameworkElement)d;
            if (Element.IsLoaded == false)
            {
                Element.Loaded += (sender, arg) =>
                {
                    HorizontalAlignment HorizontalAlignment = GetItemsPanelHorizontalAlignment(Element);
                    ItemsStackPanel ItemsStackPanel = Element?.FindDescendant<ItemsStackPanel>();
                    if (ItemsStackPanel != null) { ItemsStackPanel.HorizontalAlignment = HorizontalAlignment; }
                };
            }
            else
            {
                HorizontalAlignment HorizontalAlignment = GetItemsPanelHorizontalAlignment(Element);
                ItemsStackPanel ItemsStackPanel = Element?.FindDescendant<ItemsStackPanel>();
                if (ItemsStackPanel != null) { ItemsStackPanel.HorizontalAlignment = HorizontalAlignment; }
            }
        }

        #endregion
    }
}
