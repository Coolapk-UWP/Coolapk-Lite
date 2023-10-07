using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class ChatCardTemplates : ResourceDictionary
    {
        public ChatCardTemplates() => InitializeComponent();

        private void FrameworkElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }

            if (!(sender is FrameworkElement element)) { return; }

            if (e != null) { e.Handled = true; }

            _ = element.OpenLinkAsync(element.Tag?.ToString());
        }

        public void FrameworkElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Space:
                    FrameworkElement_Tapped(sender, null);
                    e.Handled = true;
                    break;
            }
        }

        public void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (!(sender is FrameworkElement element)) { return; }
            _ = element.ShowImageAsync(element.Tag as ImageModel);
            if (e != null) { e.Handled = true; }
        }

        public void Image_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            switch (e.Key)
            {
                case VirtualKey.Enter:
                case VirtualKey.Space:
                    Image_Tapped(sender, null);
                    e.Handled = true;
                    break;
            }
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

        private async void Border_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            DragOperationDeferral deferral = args.GetDeferral();
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await ((sender as FrameworkElement)?.Tag as ImageModel)?.GetImageDataPackageAsync(args.Data, "拖拽图片");
            deferral.Complete();
        }
    }
}
