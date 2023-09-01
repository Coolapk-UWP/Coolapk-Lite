﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.ViewModels;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Controls.DataTemplates
{
    public partial class PicTemplates : ResourceDictionary, ISharePic
    {
        public PicTemplates() => InitializeComponent();

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
                    CopyPic(image);
                    break;
                case "SaveButton":
                    SavePic(image);
                    break;
                case "ShareButton":
                    SharePic(image);
                    break;
                case "RefreshButton":
                    _ = image.Refresh();
                    break;
                case "ShowImageButton":
                    _ = element.ShowImageAsync(image);
                    break;
                case "OriginButton":
                    image.Type = ImageType.OriginImage;
                    break;
            }
        }

        private async void Border_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await GetImageDataPackageAsync(args.Data, (sender as FrameworkElement).Tag as ImageModel, "拖拽图片");
        }

        public async void CopyPic(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackageAsync(image, "复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackageAsync(image, "分享图片");
            if (dataPackage != null)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
                DataTransferManager.ShowShareUI();
            }
        }

        public async void SavePic(ImageModel imageModel)
        {
            string url = imageModel.Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }

            string fileName = GetPicTitle(url);
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };

            string fileEx = fileName.Substring(fileName.LastIndexOf('.') + 1);
            int index = fileEx.IndexOfAny(new char[] { '?', '%', '&' });
            fileEx = fileEx.Substring(0, index == -1 ? fileEx.Length : index);
            fileSavePicker.FileTypeChoices.Add($"{fileEx}文件", new string[] { "." + fileEx });

            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (Stream FolderStream = await file.OpenStreamForWriteAsync())
                {
                    using (IRandomAccessStreamWithContentType RandomAccessStream = await image.OpenReadAsync())
                    {
                        using (Stream ImageStream = RandomAccessStream.AsStreamForRead())
                        {
                            await ImageStream.CopyToAsync(FolderStream);
                        }
                    }
                }
            }
        }

        public async Task<DataPackage> GetImageDataPackageAsync(ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null) { return null; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetPicTitle(image.Uri);

            return dataPackage;
        }

        public async Task GetImageDataPackageAsync(DataPackage dataPackage, ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetPicTitle(image.Uri);
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        private string GetPicTitle(string url)
        {
            Match match = Regex.Match(url, @"[^/]+(?!.*/)");
            return match.Success ? match.Value : "图片";
        }
    }
}