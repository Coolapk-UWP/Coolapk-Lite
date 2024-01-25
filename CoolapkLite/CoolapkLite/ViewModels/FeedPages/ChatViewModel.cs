using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Message;
using CoolapkLite.Models.Upload;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class ChatViewModel : EntityItemSource, IViewModel
    {
        public static string[] ImageTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".heif", ".heic" };

        public string ID { get; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private bool isTextEmpty = true;
        public bool IsTextEmpty
        {
            get => isTextEmpty;
            set => SetProperty(ref isTextEmpty, value);
        }

        public ChatViewModel(string id, string title, CoreDispatcher dispatcher) : base(dispatcher)
        {
            Title = title;
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.GetMessageChat,
                        id,
                        p,
                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "dateline");
        }

        bool IViewModel.IsEqual(IViewModel other) => other is ChatViewModel model && IsEqual(model);
        public bool IsEqual(ChatViewModel other) => ID == other.ID;

        protected override async Task<uint> LoadItemsAsync(uint count)
        {
            if (Provider != null)
            {
                List<Entity> models = new List<Entity>((int)count);
                if (IsFullLoad)
                {
                    uint loaded = 0;
                    while (loaded < count)
                    {
                        uint temp = loaded;
                        if (loaded > 0) { _currentPage++; models.Clear(); }
                        await Provider.GetEntityAsync(models, _currentPage).ConfigureAwait(false);
                        models.Reverse();
                        loaded += await AddItemsAsync(models);
                        if (loaded <= 0 || loaded <= temp) { return loaded; }
                    }
                    return loaded;
                }
                else
                {
                    await Provider.GetEntityAsync(models, _currentPage).ConfigureAwait(false);
                    return await AddItemsAsync(models).ConfigureAwait(false);
                }
            }
            return 0;
        }

        public override async Task<bool> AddItemAsync(Entity item)
        {
            if (item != null && !(item is NullEntity))
            {
                await AddAsync(item).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public override async Task AddAsync(Entity item)
        {
            await Dispatcher.ResumeForegroundAsync();
            InsertItem(0, item);
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            switch (json.Value<string>("entityType"))
            {
                case "message":
                    yield return new MessageModel(json);
                    break;
                case "messageExtra":
                    yield return new MessageExtraModel(json);
                    break;
            }
            yield break;
        }

        public async Task<bool> CheckDataAsync(DataPackageView data)
        {
            if (data.Contains(StandardDataFormats.Bitmap))
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

        public async Task<WriteableBitmap> PickImageAsync()
        {
            FileOpenPicker FileOpen = new FileOpenPicker();
            FileOpen.FileTypeFilter.Add(".jpg");
            FileOpen.FileTypeFilter.Add(".jpeg");
            FileOpen.FileTypeFilter.Add(".png");
            FileOpen.FileTypeFilter.Add(".bmp");
            FileOpen.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            StorageFile file = await FileOpen.PickSingleFileAsync();
            return file == null ? null : await ReadFileAsync(file).ConfigureAwait(false);
        }

        public async Task<WriteableBitmap> ReadFileAsync(IStorageFile file)
        {
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                return await ReadStreamAsync(stream).ConfigureAwait(false);
            }
        }

        public async Task<WriteableBitmap> ReadStreamAsync(IRandomAccessStream stream)
        {
            BitmapDecoder ImageDecoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap SoftwareImage = await ImageDecoder.GetSoftwareBitmapAsync();
            try
            {
                WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                await WriteableImage.SetSourceAsync(stream);
                return WriteableImage;
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
                        return WriteableImage;
                    }
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(CreateFeedViewModel)).Error(e.ExceptionToMessage(), e);
                    return null;
                }
            }
        }

        public async Task<WriteableBitmap> DropFileAsync(DataPackageView data)
        {
            if (data.Contains(StandardDataFormats.Bitmap))
            {
                RandomAccessStreamReference bitmap = await data.GetBitmapAsync();
                using (IRandomAccessStreamWithContentType random = await bitmap.OpenReadAsync())
                {
                    return await ReadStreamAsync(random).ConfigureAwait(false);
                }
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                if (items.FirstOrDefault() is StorageFolder storageFolder)
                {
                    IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
                    StorageFile image = files.FirstOrDefault(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    return await ReadFileAsync(image).ConfigureAwait(false);
                }
                else
                {
                    IEnumerable<StorageFile> images = items.OfType<StorageFile>(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    StorageFile image = images.FirstOrDefault(i => ImageTypes.Any(x => i.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                    return await ReadFileAsync(image).ConfigureAwait(false);
                }
            }
            return null;
        }

        public async Task<string> UploadPicAsync(WriteableBitmap writeableBitmap)
        {
            if (writeableBitmap == null) { return string.Empty; }
            string id = ID.Substring(ID.IndexOf('_') + 1);
            if (string.IsNullOrWhiteSpace(id)) { return string.Empty; }
            _ = Dispatcher.ShowMessageAsync("上传图片");
#if NETCORE463
            List<UploadFileFragment> fragments = new List<UploadFileFragment>
            {
                await UploadFileFragment.FromWriteableBitmapAsync(writeableBitmap)
            };
            List<string> results = await RequestHelper.UploadImages(fragments, "message", "message", id);
            _ = Dispatcher.ShowMessageAsync($"上传了 {results.Count} 张图片");
#else
            string[] results = null;
            if (ExtensionManager.IsOSSUploaderSupported)
            {
                ExtensionManager manager = new ExtensionManager(ExtensionManager.OSSUploader);
                await manager.InitializeAsync(Dispatcher);
                if (manager.Extensions.Count > 0)
                {
                    List<UploadFileFragment> fragments = new List<UploadFileFragment>
                    {
                        await UploadFileFragment.FromWriteableBitmapAsync(writeableBitmap)
                    };
                    results = await RequestHelper.UploadImagesAsync(manager.Extensions.FirstOrDefault(), fragments, "message", "message", id);
                    _ = Dispatcher.ShowMessageAsync($"上传了 {results.Length} 张图片");
                }
            }
#endif
            return results == null ? string.Empty : results.FirstOrDefault();
        }
    }
}
