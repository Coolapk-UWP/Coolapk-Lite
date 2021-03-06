using ColorThiefDotNet;
using CoolapkLite.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models.Images
{
    public class ImageModel : INotifyPropertyChanged, IPic
    {
        private bool isLongPic;
        private bool isWidePic;
        protected WeakReference<BitmapImage> pic;
        protected ImmutableArray<ImageModel> contextArray;
        private static readonly ColorThief thief = new ColorThief();
        private static readonly Windows.UI.Color fallbackColor = Windows.UI.Color.FromArgb(0x99, 0, 0, 0);
        private Windows.UI.Color backgroundColor = fallbackColor;

        public Windows.UI.Color BackgroundColor
        {
            get => backgroundColor;
            private set
            {
                backgroundColor = value;
                RaisePropertyChangedEvent();
            }
        }

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
                    GetImage();
                    return ImageCacheHelper.NoPic;
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
                    if (value.UriSource is null)
                    {
                        SetBrush();
                    }
                    else { BackgroundColor = fallbackColor; }
                }
                RaisePropertyChangedEvent();
            }
        }

        public bool IsLongPic
        {
            get => isLongPic;
            private set
            {
                if (isLongPic != value)
                {
                    isLongPic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsWidePic
        {
            get => isWidePic;
            private set
            {
                if (isWidePic != value)
                {
                    isWidePic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ImmutableArray<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray.IsDefaultOrEmpty)
                {
                    contextArray = value;
                }
            }
        }

        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF"); }

        public string Uri { get; }

        public ImageType Type { get; }

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
            SettingsHelper.UISettingChanged.Add(mode =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        _ = UIHelper.ShellDispatcher?.RunAsync(
                            Windows.UI.Core.CoreDispatcherPriority.Normal,
                            () =>
                            {
                                if (pic == null)
                                {
                                    GetImage();
                                }
                                else if (pic.TryGetTarget(out BitmapImage image) && image.UriSource != null)
                                {
                                    Pic = ImageCacheHelper.NoPic;
                                }
                            });

                        break;

                    case UISettingChangedType.NoPicChanged:
                        GetImage();
                        break;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async void GetImage()
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = ImageCacheHelper.NoPic; }
            BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri);
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { return; }
            Pic = bitmapImage;
            IsLongPic =
                ((bitmapImage.PixelHeight * Window.Current.Bounds.Width) > bitmapImage.PixelWidth * Window.Current.Bounds.Height * 1.5)
                && bitmapImage.PixelHeight > bitmapImage.PixelWidth * 1.5;
            IsWidePic =
                ((bitmapImage.PixelWidth * Window.Current.Bounds.Height) > bitmapImage.PixelHeight * Window.Current.Bounds.Width * 1.5)
                && bitmapImage.PixelWidth > bitmapImage.PixelHeight * 1.5;
        }

        private async void SetBrush()
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(Type, Uri);
            if (file is null) { return; }
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                QuantizedColor color = await thief.GetColor(decoder);
                BackgroundColor =
                    Windows.UI.Color.FromArgb(
                        color.Color.A,
                        color.Color.R,
                        color.Color.G,
                        color.Color.B);
            }
        }
    }
}
