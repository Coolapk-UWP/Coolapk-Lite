﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels
{
    public class ShowImageViewModel : IViewModel
    {
        private string ImageName = string.Empty;

        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set => SetProperty(ref title, value);
        }

        private int index = -1;
        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    if (index != -1) { RegisterImage(Images[index], Images[value]); }
                    index = value;
                    RaisePropertyChangedEvent();
                    Title = GetTitle(Images[value].Uri);
                    ShowOrigin = Images[value].Type.HasFlag(ImageType.Small);
                }
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            protected set => SetProperty(ref isLoading, value);
        }

        private IList<ImageModel> images;
        public IList<ImageModel> Images
        {
            get => images;
            private set => SetProperty(ref images, value);
        }

        private bool showOrigin = false;
        public bool ShowOrigin
        {
            get => showOrigin;
            set => SetProperty(ref showOrigin, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public ShowImageViewModel(ImageModel image)
        {
            if (image.ContextArray.Any())
            {
                Images = image.ContextArray;
                Index = Images.IndexOf(image);
            }
            else
            {
                Images = new List<ImageModel> { image };
                Index = 0;
            }
            if (SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture))
            {
                foreach (ImageModel Image in Images)
                {
                    Image.Type &= (ImageType)0xFE;
                }
            }
        }

        ~ShowImageViewModel()
        {
            foreach (ImageModel image in images)
            {
                image.LoadStarted -= OnLoadStarted;
                image.LoadCompleted -= OnLoadCompleted;
            }
        }

        public async Task Refresh(bool reset = false) => await Images[Index].Refresh();

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);
        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;

        private string GetTitle(string url)
        {
            Match match = Regex.Match(url, @"[^/]+(?!.*/)");
            ImageName = match.Success ? match.Value : "查看图片";
            return $"{ImageName} ({Index + 1}/{Images.Count})";
        }

        public async void CopyPic()
        {
            DataPackage dataPackage = await GetImageDataPackage("复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic()
        {
            DataPackage dataPackage = await GetImageDataPackage("分享图片");
            if (dataPackage != null)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
                DataTransferManager.ShowShareUI();
            }
        }

        public async Task<DataPackage> GetImageDataPackage(string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return null;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = ImageName;

            return dataPackage;
        }

        public async Task GetImageDataPackage(DataPackage dataPackage, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = ImageName;
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        public async void SavePic()
        {
            string url = Images[Index].Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }

            string fileName = ImageName;
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

        private void RegisterImage(ImageModel oldValue, ImageModel newValue)
        {
            oldValue.LoadStarted -= OnLoadStarted;
            oldValue.LoadCompleted -= OnLoadCompleted;
            newValue.LoadStarted += OnLoadStarted;
            newValue.LoadCompleted += OnLoadCompleted;
        }

        private void OnLoadStarted(ImageModel sender, object args) => IsLoading = true;

        private void OnLoadCompleted(ImageModel sender, object args) => IsLoading = false;
    }
}
