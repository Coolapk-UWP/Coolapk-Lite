using CoolapkLite.Common;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models.Images
{
    public class ImageModel : INotifyPropertyChanged, IPic
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));

        private readonly Action<UISettingChangedType> UISettingChanged;

        public static bool IsAutoPlaySupported { get; } = ApiInfoHelper.IsBitmapImageAutoPlaySupported;

        public CoreDispatcher Dispatcher { get; private set; }

        public string Title => GetTitle();

        public bool IsSmallImage => Type.HasFlag(ImageType.Small);

        private bool isLongPic = false;
        public bool IsLongPic
        {
            get => isLongPic;
            private set => SetProperty(ref isLongPic, value);
        }

        private bool isWidePic = false;
        public bool IsWidePic
        {
            get => isWidePic;
            private set => SetProperty(ref isWidePic, value);
        }

        private bool isGif = false;
        public bool IsGif
        {
            get => isGif;
            private set => SetProperty(ref isGif, value);
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set => SetProperty(ref isLoading, value);
        }

        private string uri;
        public string Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImageAsync();
                    }
                    RaisePropertyChangedEvent();
                    RaisePropertyChangedEvent(nameof(Title));
                }
            }
        }

        private ImageType type;
        public ImageType Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImageAsync();
                    }
                    RaisePropertyChangedEvent();
                    RaisePropertyChangedEvent(nameof(IsSmallImage));
                }
            }
        }

        protected WeakReference<BitmapImage> pic;
        public BitmapImage Pic
        {
            get
            {
                if (Dispatcher == null)
                {
                    Dispatcher = UIHelper.TryGetForCurrentCoreDispatcher();
                }

                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                else
                {
                    _ = GetImageAsync();
                    return ImageCacheHelper.GetNoPic(Dispatcher);
                }
            }
            protected set
            {
                if (pic == null)
                {
                    pic = new WeakReference<BitmapImage>(value);
                }
                else
                {
                    pic.SetTarget(value);
                }
                RaisePropertyChangedEvent();
            }
        }

        protected ImmutableArray<ImageModel> contextArray = ImmutableArray<ImageModel>.Empty;
        public ImmutableArray<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray.IsDefaultOrEmpty)
                {
                    contextArray = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
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

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
            UISettingChanged = async (mode) =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
                        {
                            if (pic != null && pic.TryGetTarget(out BitmapImage _))
                            {
                                if (Dispatcher == null) { return; }
                                Pic = await ImageCacheHelper.GetNoPicAsync(Dispatcher);
                            }
                        }
                        break;

                    case UISettingChangedType.NoPicChanged when pic != null && pic.TryGetTarget(out BitmapImage _):
                        _ = GetImageAsync();
                        break;
                }
            };
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
        }

        public ImageModel(string uri, ImageType type, CoreDispatcher dispatcher) : this(uri, type)
        {
            Dispatcher = dispatcher;
        }

        ~ImageModel()
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public static void SetSemaphoreSlim(int initialCount)
        {
            semaphoreSlim.Dispose();
            semaphoreSlim = new SemaphoreSlim(initialCount);
        }

        private async Task GetImageAsync()
        {
            if (Dispatcher == null) { return; }
            await ThreadSwitcher.ResumeBackgroundAsync();
            try
            {
                IsLoading = true;
                LoadStarted?.Invoke(this, null);
                await semaphoreSlim.WaitAsync();
                if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = await ImageCacheHelper.GetNoPicAsync(Dispatcher); }
                BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri, Dispatcher);
                if (bitmapImage.Dispatcher != Dispatcher)
                {
                    StorageFile file = await ImageCacheHelper.GetImageFileAsync(Type, Uri);
                    using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                    {
                        bitmapImage = await Dispatcher.AwaitableRunAsync(async () =>
                        {
                            BitmapImage image = new BitmapImage();
                            await image.SetSourceAsync(stream);
                            return image;
                        });
                    }
                }
                if (bitmapImage != null)
                {
                    Pic = bitmapImage;
                    if (!bitmapImage.Dispatcher.HasThreadAccess)
                    {
                        await bitmapImage.Dispatcher.ResumeForegroundAsync();
                    }
                    double PixelWidth = bitmapImage.PixelWidth;
                    double PixelHeight = bitmapImage.PixelHeight;
                    Rect Bounds = Window.Current is Window window
                        ? await window.Dispatcher.AwaitableRunAsync(() => window.Bounds)
                        : await CoreApplication.MainView.Dispatcher.AwaitableRunAsync(() => CoreApplication.MainView.CoreWindow.Bounds);
                    IsLongPic = ((PixelHeight * Bounds.Width) > PixelWidth * Bounds.Height * 1.5)
                                && PixelHeight > PixelWidth * 1.5;
                    IsWidePic = ((PixelWidth * Bounds.Height) > PixelHeight * Bounds.Width * 1.5)
                                && PixelWidth > PixelHeight * 1.5;
                    IsGif = IsAutoPlaySupported && !Type.HasFlag(ImageType.Small) ? bitmapImage.IsAnimatedBitmap : Uri.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    Pic = null;
                    IsLongPic = false;
                    IsWidePic = false;
                    IsGif = Uri.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
                }
            }
            finally
            {
                LoadCompleted?.Invoke(this, null);
                semaphoreSlim.Release();
                IsLoading = false;
            }
        }

        public async void CopyPic()
        {
            DataPackage dataPackage = await GetImageDataPackageAsync("复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic()
        {
            DataPackage dataPackage = await GetImageDataPackageAsync("分享图片");
            if (dataPackage != null)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
                DataTransferManager.ShowShareUI();
            }
        }

        public async void SavePic()
        {
            string url = Uri;
            ImageType type = Type & (ImageType)0xFE;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(type, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }

            string fileName = Title;
            int index = fileName.LastIndexOf('.');
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = index != -1 ? fileName.Substring(0, index++) : fileName,
            };

            if (index != -1)
            {
                string fileEx = fileName.Substring(index);
                if (!string.IsNullOrEmpty(fileEx))
                {
                    index = fileEx.IndexOfAny(new char[] { '?', '%', '&' });
                    fileEx = fileEx.Substring(0, index == -1 ? fileEx.Length : index);
                    fileSavePicker.FileTypeChoices.Add($"{fileEx}文件", new string[] { $".{fileEx}" });
                }
            }

            if (fileSavePicker.FileTypeChoices.Count < 1)
            {
                fileSavePicker.FileTypeChoices.Add("png 文件", new string[] { ".png" });
            }

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

        public async Task<DataPackage> GetImageDataPackageAsync(string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(Type & (ImageType)0xFE, Uri);
            if (file == null) { return null; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = Title;

            return dataPackage;
        }

        public async Task GetImageDataPackageAsync(DataPackage dataPackage, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(Type & (ImageType)0xFE, Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                Dispatcher.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = Title;
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        public ImageModel Clone(CoreDispatcher dispatcher)
        {
            if (contextArray.Any())
            {
                ImmutableArray<ImageModel> array = contextArray.Select(x => new ImageModel(x.uri, x.type, dispatcher)).ToImmutableArray();
                ImageModel image = array.FirstOrDefault(x => x.uri == uri);
                if (image != null)
                {
                    foreach (ImageModel item in array)
                    {
                        item.ContextArray = array;
                    }
                }
                else
                {
                    image.ContextArray = array;
                }
                return image;
            }
            else
            {
                ImageModel image = new ImageModel(uri, type, dispatcher)
                {
                    ContextArray = contextArray.Select(x => new ImageModel(x.uri, x.type, dispatcher)).ToImmutableArray()
                };
                return image;
            }
        }

        public void ChangeDispatcher(CoreDispatcher dispatcher)
        {
            if (Dispatcher != dispatcher)
            {
                Dispatcher = dispatcher;
                IsLongPic = false;
                IsWidePic = false;
                pic = null;
                RaisePropertyChangedEvent(nameof(Pic));
            }
            ContextArray.ForEach((x) => x.ChangeDispatcher(dispatcher));
        }

        private string GetTitle()
        {
            Match match = Regex.Match(Uri, @"[^/]+(?!.*/)");
            return match.Success ? match.Value : $"CoolapkLite_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
        }

        public async Task Refresh() => await GetImageAsync();

        public override string ToString() => string.Join(" - ", Title, Uri);
    }
}
