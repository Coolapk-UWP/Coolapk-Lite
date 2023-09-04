using CoolapkLite.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class RelationRowsTemplates : ResourceDictionary
    {
        public RelationRowsTemplates() => InitializeComponent();

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null) { e.Handled = true; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            _ = element.OpenLinkAsync(element.Tag?.ToString());
        }
    }
}
