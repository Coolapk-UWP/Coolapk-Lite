using CoolapkLite.ViewModels;
using CoolapkLite.ViewModels.DataSource;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedShellListControl"/> class.
        /// </summary>
        public FeedShellListControl() => InitializeComponent();

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(FeedShellListControl),
                null);

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region ItemSource

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<ShyHeaderItem>),
                typeof(FeedShellListControl),
                null);

        public IEnumerable<ShyHeaderItem> ItemSource
        {
            get => (IEnumerable<ShyHeaderItem>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        #endregion

        #region HeaderMargin

        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(FeedShellListControl),
                null);

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        #endregion

        #region HeaderHeight

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(FeedShellListControl),
                null);

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        #endregion

        #region RefreshButtonVisibility

        public static readonly DependencyProperty RefreshButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RefreshButtonVisibility),
                typeof(Visibility),
                typeof(FeedShellListControl),
                new PropertyMetadata(Visibility.Visible));

        public Visibility RefreshButtonVisibility
        {
            get => (Visibility)GetValue(RefreshButtonVisibilityProperty);
            set => SetValue(RefreshButtonVisibilityProperty, value);
        }

        #endregion

        private void ShyHeaderPivotListView_ShyHeaderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShyHeaderPivotListView.ItemsSource is IToggleChangeSelectedIndex ToggleItemsSource)
            {
                CheckBox.Visibility = Visibility.Visible;
                CheckBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding
                {
                    Mode = BindingMode.TwoWay,
                    Source = ToggleItemsSource,
                    Path = new PropertyPath("ToggleIsOn")
                });
                ToggleSwitch.Visibility = Visibility.Visible;
                ToggleSwitch.SetBinding(ToggleSwitch.IsOnProperty, new Binding
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

            if (ShyHeaderPivotListView.ItemsSource is IComboBoxChangeSelectedIndex ComboBoxItemsSource)
            {
                ComboBox.Visibility = Visibility.Visible;
                ComboBox.ItemsSource = ComboBoxItemsSource.ItemSource;
                ComboBox.SetBinding(Selector.SelectedIndexProperty, new Binding
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

        private void ShyHeaderPivotListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((sender as ShyHeaderPivotListView).ActualWidth < 424 + RefreshButton.ActualWidth)
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

            ScrollViewer ScrollViewer = ShyHeaderPivotListView.FindDescendant<ScrollViewer>();

            if (ScrollViewer != null)
            {
                ScrollViewer.Margin = new Thickness(0, ScrollViewerMargin.Top, 0, Padding.Bottom);
                ScrollViewer.Padding = new Thickness(0, ScrollViewerPadding.Top, 0, -Padding.Bottom);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShyHeaderPivotListView.ItemsSource is EntityItemSource entities)
            {
                _ = entities.Refresh(true);
            }
        }
    }
}
