using CoolapkLite.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models.Images
{
    public class ImageModel : INotifyPropertyChanged, IPic
    {
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
                }
                RaisePropertyChangedEvent();
            }
        }

        private bool isLongPic;
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

        private bool isWidePic;
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

        protected ImmutableArray<ImageModel> contextArray;
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

        public bool IsGif => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF");

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
                        _ = UIHelper.ShellDispatcher?.AwaitableRunAsync(() =>
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

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async void GetImage()
        {
            LoadStarted?.Invoke(this, null);
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = ImageCacheHelper.NoPic; }
            BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri);
            Pic = bitmapImage;
            IsLongPic =
                ((bitmapImage.PixelHeight * Window.Current.Bounds.Width) > bitmapImage.PixelWidth * Window.Current.Bounds.Height * 1.5)
                && bitmapImage.PixelHeight > bitmapImage.PixelWidth * 1.5;
            IsWidePic =
                ((bitmapImage.PixelWidth * Window.Current.Bounds.Height) > bitmapImage.PixelHeight * Window.Current.Bounds.Width * 1.5)
                && bitmapImage.PixelWidth > bitmapImage.PixelHeight * 1.5;
            LoadCompleted?.Invoke(this, null);
        }
    }
}
