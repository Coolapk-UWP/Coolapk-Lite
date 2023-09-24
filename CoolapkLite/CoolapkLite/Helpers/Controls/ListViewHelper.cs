using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class ListViewHelper
    {
        #region Padding

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.RegisterAttached(
                "Padding",
                typeof(Thickness),
                typeof(ListViewHelper),
                new PropertyMetadata(null, OnPaddingChanged));

        public static Thickness GetPadding(ListViewBase element)
        {
            return (Thickness)element.GetValue(PaddingProperty);
        }

        public static void SetPadding(ListViewBase element, Thickness value)
        {
            element.SetValue(PaddingProperty, value);
        }

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListViewBase element = (ListViewBase)d;
            if (ApiInfoHelper.IsFrameworkElementIsLoadedSupported)
            {
                if (element.IsLoaded)
                {
                    Thickness padding = GetPadding(element);
                    UpdatePadding(element, padding);
                }
                else
                {
                    element.Loaded -= OnListViewLoaded;
                    element.Loaded += OnListViewLoaded;
                }
            }
            else
            {
                element.Loaded -= OnListViewLoaded;
                element.Loaded += OnListViewLoaded;
                Thickness padding = GetPadding(element);
                UpdatePadding(element, padding);
            }
        }

        private static void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            ListViewBase element = (ListViewBase)sender;
            Thickness padding = GetPadding(element);
            UpdatePadding(element, padding);
        }

        private static void UpdatePadding(ListViewBase element, Thickness padding)
        {
            if (element?.FindDescendant<ScrollViewer>() is ScrollViewer scrollViewer
                && scrollViewer?.FindDescendant<ItemsPresenter>()?.FindDescendant<Panel>() is Panel panel)
            {
                Thickness margin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);

                panel.Margin = padding;

                scrollViewer.Margin = padding;
                scrollViewer.Padding = margin;
            }
        }

        #endregion
    }
}
