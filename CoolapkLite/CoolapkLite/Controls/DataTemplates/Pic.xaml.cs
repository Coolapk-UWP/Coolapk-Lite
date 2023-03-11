using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class Pic : ResourceDictionary
    {
        public Pic() => InitializeComponent();

        public void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = UIHelper.ShowImageAsync((sender as FrameworkElement).Tag as ImageModel);
        }

        public void Image_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                Image_Tapped(sender, null);
            }
        }
    }
}