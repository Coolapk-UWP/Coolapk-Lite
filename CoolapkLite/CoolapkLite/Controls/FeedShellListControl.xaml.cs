using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.DataSource;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(FeedShellListControl),
                null);

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IList<ShyHeaderItem>),
                typeof(FeedShellListControl),
                null);

        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(FeedShellListControl),
                null);

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(FeedShellListControl),
                null);

        public static readonly DependencyProperty RefreshButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RefreshButtonVisibility),
                typeof(Visibility),
                typeof(FeedShellListControl),
                new PropertyMetadata(Visibility.Visible));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public IList<ShyHeaderItem> ItemSource
        {
            get => (IList<ShyHeaderItem>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public Visibility RefreshButtonVisibility
        {
            get => (Visibility)GetValue(RefreshButtonVisibilityProperty);
            set => SetValue(RefreshButtonVisibilityProperty, value);
        }

        public FeedShellListControl()
        {
            InitializeComponent();

            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "FocusVisualMargin"))
            {
                RefreshButton.FocusVisualMargin = new Thickness(0);
            }
        }

        private void ShyHeaderListView_ShyHeaderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShyHeaderListView.ItemsSource is ICanToggleChangeSelectedIndex ToggleItemsSource)
            {
                CheckBox.Visibility = Visibility.Visible;
                CheckBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding()
                {
                    Mode = BindingMode.TwoWay,
                    Source = ToggleItemsSource,
                    Path = new PropertyPath("ToggleIsOn")
                });
                ToggleSwitch.Visibility = Visibility.Visible;
                ToggleSwitch.SetBinding(ToggleSwitch.IsOnProperty, new Binding()
                {
                    Mode = BindingMode.TwoWay,
                    Source = ToggleItemsSource,
                    Path = new PropertyPath("ToggleIsOn")
                });
            }
            else
            {
                ToggleSwitch.Visibility = CheckBox.Visibility = Visibility.Collapsed;
            }

            if (ShyHeaderListView.ItemsSource is ICanComboBoxChangeSelectedIndex ComboBoxItemsSource)
            {
                ComboBox.Visibility = Visibility.Visible;
                ComboBox.ItemsSource = ComboBoxItemsSource.ItemSource;
                ComboBox.SetBinding(Selector.SelectedIndexProperty, new Binding()
                {
                    Mode = BindingMode.TwoWay,
                    Source = ComboBoxItemsSource,
                    Path = new PropertyPath("ComboBoxSelectedIndex")
                });
            }
            else
            {
                ComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShyHeaderListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((sender as ShyHeaderListView).ActualWidth < 424 + RefreshButton.ActualWidth)
            {
                ToggleSwitchBorder.Visibility = Visibility.Collapsed;
                CheckBoxBorder.Visibility = Visibility.Visible;
            }
            else
            {
                ToggleSwitchBorder.Visibility = Visibility.Visible;
                CheckBoxBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Thickness ScrollViewerMargin = (Thickness)Application.Current.Resources["ScrollViewerMargin"];
            Thickness ScrollViewerPadding = (Thickness)Application.Current.Resources["ScrollViewerPadding"];

            ScrollViewer ScrollViewer = ShyHeaderListView.FindDescendant<ScrollViewer>();

            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = new Thickness(0, ScrollViewerMargin.Top, 0, Padding.Bottom);
                ScrollViewer.Padding = new Thickness(0, ScrollViewerPadding.Top, 0, -Padding.Bottom);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShyHeaderListView.ItemsSource is EntityItemSource entities)
            {
                _ = entities.Refresh(true);
            }
        }
    }
}
