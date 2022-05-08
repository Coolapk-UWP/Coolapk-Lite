using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CoolapkLite.Helpers
{
    public static class ScrollBarHelper
    {
        #region VerticalScrollBarMargin

        public static Thickness GetVerticalScrollBarMargin(FrameworkElement element)
        {
            return (Thickness)element.GetValue(VerticalScrollBarMarginProperty);
        }

        public static void SetVerticalScrollBarMargin(FrameworkElement element, Thickness value)
        {
            element.SetValue(VerticalScrollBarMarginProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollBarMarginProperty =
            DependencyProperty.RegisterAttached(
                "VerticalScrollBarMargin",
                typeof(Thickness),
                typeof(ScrollBarHelper),
                new PropertyMetadata(null, OnVerticalScrollBarMarginChanged));

        private static void OnVerticalScrollBarMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement Element = (FrameworkElement)d;
            if (Element.IsLoaded == false)
            {
                Element.Loaded += (sender, arg) =>
                {
                    Thickness Margin = GetVerticalScrollBarMargin((FrameworkElement)d);
                    ScrollViewer ScrollViewer = Element is ScrollViewer ? (ScrollViewer)Element : Element?.FindDescendant<ScrollViewer>();
                    ScrollBar VerticalScrollBar = Element is ScrollBar ? (ScrollBar)Element : (ScrollBar)ScrollViewer?.FindDescendantByName("VerticalScrollBar");
                    if (VerticalScrollBar != null) { VerticalScrollBar.Margin = Margin; }
                };
            }
            else
            {
                Thickness Margin = GetVerticalScrollBarMargin((FrameworkElement)d);
                ScrollViewer ScrollViewer = Element is ScrollViewer ? (ScrollViewer)Element : Element?.FindDescendant<ScrollViewer>();
                ScrollBar VerticalScrollBar = Element is ScrollBar ? (ScrollBar)Element : (ScrollBar)ScrollViewer?.FindDescendantByName("VerticalScrollBar");
                if (VerticalScrollBar != null) { VerticalScrollBar.Margin = Margin; }
            }
        }

        #endregion
    }
}
