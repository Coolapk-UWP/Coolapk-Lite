using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
           nameof(Header),
           typeof(object),
           typeof(FeedShellListControl),
           null);

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
           nameof(ItemSource),
           typeof(IList<ShyHeaderItem>),
           typeof(FeedShellListControl),
           null);

        public static readonly DependencyProperty HeaderMarginProperty = DependencyProperty.Register(
           nameof(HeaderMargin),
           typeof(double),
           typeof(FeedShellListControl),
           null);

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
           nameof(HeaderHeight),
           typeof(double),
           typeof(FeedShellListControl),
           null);

        public static readonly DependencyProperty RefreshButtonVisibilityProperty = DependencyProperty.Register(
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
        }
    }
}
