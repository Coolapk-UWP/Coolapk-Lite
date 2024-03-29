﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Upload;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

#if !NETCORE463
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace CoolapkLite.ViewModels.FeedPages
{
    public class CreateFeedViewModel : IViewModel
    {
        public static string[] ImageTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".heif", ".heic" };

        public readonly CreateUserItemSource CreateUserItemSource = new CreateUserItemSource();
        public readonly CreateTopicItemSource CreateTopicItemSource = new CreateTopicItemSource();

        public readonly ObservableCollection<WriteableBitmap> Pictures = new ObservableCollection<WriteableBitmap>();

        public CoreDispatcher Dispatcher { get; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
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

        public CreateFeedViewModel(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        public async Task Refresh(bool reset)
        {
            await CreateUserItemSource.Refresh(reset).ConfigureAwait(false);
            await CreateTopicItemSource.Refresh(reset).ConfigureAwait(false);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is CreateFeedViewModel model && Equals(model);

        public async Task ReadFileAsync(IStorageFile file)
        {
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                await ReadStreamAsync(stream).ConfigureAwait(false);
            }
        }

        public async Task ReadStreamAsync(IRandomAccessStream stream)
        {
            BitmapDecoder ImageDecoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap SoftwareImage = await ImageDecoder.GetSoftwareBitmapAsync();
            try
            {
                WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                await WriteableImage.SetSourceAsync(stream);
                Pictures.Add(WriteableImage);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(CreateFeedViewModel)).Warn(ex.ExceptionToMessage(), ex);
                try
                {
                    using (InMemoryRandomAccessStream random = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random);
                        encoder.SetSoftwareBitmap(SoftwareImage);
                        await encoder.FlushAsync();
                        WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                        await WriteableImage.SetSourceAsync(random);
                        Pictures.Add(WriteableImage);
                    }
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(CreateFeedViewModel)).Error(e.ExceptionToMessage(), e);
                }
            }
        }

        public async Task<bool> CheckDataAsync(DataPackageView data)
        {
            if (Pictures.Count >= 9)
            {
                return false;
            }
            else if (data.Contains(StandardDataFormats.Bitmap))
            {
                return true;
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                if (items.FirstOrDefault() is StorageFolder storageFolder)
                {
                    IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
                    IEnumerable<StorageFile> images = files.Where(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    return images.Any();
                }
                else
                {
                    IEnumerable<StorageFile> images = items.OfType<StorageFile>(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    return images.Any();
                }
            }
            else
            {
                return false;
            }
        }

        public async void PickImage()
        {
            FileOpenPicker FileOpen = new FileOpenPicker();
            FileOpen.FileTypeFilter.Add(".jpg");
            FileOpen.FileTypeFilter.Add(".jpeg");
            FileOpen.FileTypeFilter.Add(".png");
            FileOpen.FileTypeFilter.Add(".bmp");
            FileOpen.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            foreach (StorageFile file in await FileOpen.PickMultipleFilesAsync())
            {
                if (file != null) { await ReadFileAsync(file).ConfigureAwait(false); }
            }
        }

        public async Task<IList<string>> UploadPicAsync()
        {
            IList<string> results = null;
            if (Pictures.Count <= 0) { return Array.Empty<string>(); }
            _ = Dispatcher.ShowMessageAsync("上传图片");
#if NETCORE463
            List<UploadFileFragment> fragments = new List<UploadFileFragment>();
            foreach (WriteableBitmap pic in Pictures)
            {
                await UploadFileFragment.FromWriteableBitmapAsync(pic).ContinueWith(x => fragments.Add(x.Result));
            }
            results = await RequestHelper.UploadImages(fragments, "image", "feed", string.Empty);
            _ = Dispatcher.ShowMessageAsync($"上传了 {results.Count} 张图片");
#else
            if (ExtensionManager.IsOSSUploaderSupported)
            {
                ExtensionManager manager = new ExtensionManager(ExtensionManager.OSSUploader);
                await manager.InitializeAsync(Dispatcher);
                if (manager.Extensions.Count > 0)
                {
                    List<UploadFileFragment> fragments = new List<UploadFileFragment>();
                    foreach (WriteableBitmap pic in Pictures)
                    {
                        await UploadFileFragment.FromWriteableBitmapAsync(pic).ContinueWith(x => fragments.Add(x.Result));
                    }
                    results = await RequestHelper.UploadImagesAsync(manager.Extensions.FirstOrDefault(), fragments, "image", "feed", string.Empty);
                    _ = Dispatcher.ShowMessageAsync($"上传了 {results.Count} 张图片");
                    return results;
                }
            }
            else
            {
                int i = 0;
                results = new List<string>(Pictures.Count);
                foreach (WriteableBitmap pic in Pictures)
                {
                    i++;
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        using (Stream pixelStream = pic.PixelBuffer.AsStream())
                        {
                            byte[] pixels = new byte[pixelStream.Length];
                            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                (uint)pic.PixelWidth,
                                (uint)pic.PixelHeight,
                                96.0,
                                96.0,
                                pixels);

                            await encoder.FlushAsync();

                            byte[] bytes = await stream.GetBytesAsync();
                            (bool isSucceed, string result) = await RequestHelper.UploadImageAsync(bytes, "pic");
                            if (isSucceed) { results.Add(result); }
                        }
                    }
                    _ = Dispatcher.ShowMessageAsync($"已上传 ({i}/{Pictures.Count})");
                }
            }
#endif
            return results ?? Array.Empty<string>();
        }

        public async Task DropFileAsync(DataPackageView data)
        {
            if (Pictures.Count >= 9)
            {
                return;
            }
            else if (data.Contains(StandardDataFormats.Bitmap))
            {
                RandomAccessStreamReference bitmap = await data.GetBitmapAsync();
                using (IRandomAccessStreamWithContentType random = await bitmap.OpenReadAsync())
                {
                    await ReadStreamAsync(random).ConfigureAwait(false);
                }
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                if (items.FirstOrDefault() is StorageFolder storageFolder)
                {
                    IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
                    IEnumerable<StorageFile> images = files.Take(9 - Pictures.Count, i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    if (images.Any()) { images.ForEach(async image => await ReadFileAsync(image).ConfigureAwait(false)); }
                }
                else
                {
                    IEnumerable<StorageFile> images = items.OfType<StorageFile>(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    if (images.Any()) { images.Take(9 - Pictures.Count).ForEach(async image => await ReadFileAsync(image).ConfigureAwait(false)); }
                }
            }
        }
    }

    public class CreateUserItemSource : EntityItemSource
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider(value);
                }
            }
        }

        public CreateUserItemSource(string keyword = " ")
        {
            Keyword = keyword;
        }

        private void UpdateProvider(string keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.SearchCreateUsers,
                        keyword,
                        p,
                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    GetEntities,
                    "uid");
            }
            else if (SettingsHelper.Get<string>(SettingsHelper.Uid) is string uid && !string.IsNullOrEmpty(uid))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            "followList",
                            uid,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    o => new UserModel((JObject)o["fUserInfo"]).AsEnumerable(),
                    "fuid");
            }
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new UserModel(jo);
        }
    }

    public class CreateTopicItemSource : EntityItemSource
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider(value);
                }
            }
        }

        public CreateTopicItemSource(string keyword = " ")
        {
            Keyword = keyword;
        }

        private void UpdateProvider(string keyword)
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchCreateTags,
                    keyword,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new TopicModel(jo);
        }
    }
}
