﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
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

        public static bool IsAutoPlaySupported => ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay");

        public CoreDispatcher Dispatcher { get; private set; }

        protected WeakReference<BitmapImage> pic;
        public BitmapImage Pic
        {
            get
            {
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

        private string uri;
        public string Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    RaisePropertyChangedEvent(nameof(IsGif));
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImageAsync();
                    }
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
                }
            }
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set => SetProperty(ref isLoading, value);
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

        public ImageModel(string uri, ImageType type) : this(uri, type, UIHelper.TryGetForCurrentCoreDispatcher())
        {
        }

        public ImageModel(string uri, ImageType type, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
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
                                Pic = await ImageCacheHelper.GetNoPicAsync(Dispatcher);
                            }
                        }
                        break;

                    case UISettingChangedType.NoPicChanged:
                        if (pic != null && pic.TryGetTarget(out BitmapImage _))
                        {
                            _ = GetImageAsync();
                        }
                        break;
                }
            };
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
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

        public async Task Refresh() => await GetImageAsync();

        public override string ToString() => Uri;
    }
}
