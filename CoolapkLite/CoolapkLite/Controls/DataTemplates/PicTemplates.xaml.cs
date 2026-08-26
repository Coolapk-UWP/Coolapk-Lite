using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class PicTemplates : ResourceDictionary
    {
        public PicTemplates() => InitializeComponent();

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
                    _ = image.Refresh(element.Dispatcher);
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