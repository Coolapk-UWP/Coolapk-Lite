using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class PicTemplates : ResourceDictionary
    {
        public PicTemplates() => InitializeComponent();

        public void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (!(sender is ImageControl element)) { return; }
            _ = element.ShowImageAsync(element.Source);
            if (e != null) { e.Handled = true; }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element && element.Tag is ImageModel image)) { return; }
            switch (element.Name)
            {
                case "CopyButton":
                    image.CopyPic();
                    break;
                case "SaveButton":
                    image.SavePic();
                    break;
                case "ShareButton":
                    image.SharePic();
                    break;
                case "RefreshButton":
                    _ = image.Refresh();
                    break;
                case "ShowImageButton":
                    _ = element.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type &= (ImageType)0xFE;
                    break;
            }
        }
    }
}