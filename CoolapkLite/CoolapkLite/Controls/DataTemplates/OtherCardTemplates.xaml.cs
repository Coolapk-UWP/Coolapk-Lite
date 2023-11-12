using CoolapkLite.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class OtherCardTemplates : ResourceDictionary
    {
        public OtherCardTemplates() => InitializeComponent();

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag?.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case "NewWindow":
                    _ = element.Dispatcher.OpenLinkOutsideAsync(element.Tag?.ToString());
                    break;
                default:
                    FrameworkElement_Tapped(sender, null);
                    break;
            }
        }
    }
}
