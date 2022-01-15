using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
           "Header",
           typeof(object),
           typeof(FeedShellListControl),
           null);

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public FeedShellListControl()
        {
            InitializeComponent();
        }
    }
}
