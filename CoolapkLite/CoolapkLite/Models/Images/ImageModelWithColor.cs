using ColorThiefDotNet;
using CoolapkLite.Helpers;
using System;
using System.ComponentModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models.Images
{
    public class ImageModelWithColor : ImageModel, INotifyPropertyChanged
    {
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

        new public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ImageModelWithColor(string uri, ImageType type) : base(uri, type)
        {

        }

        new protected void SetBrush(BitmapImage value)
        {
            if (value.UriSource is null)
            {
                SetBrush();
            }
            else { BackgroundColor = fallbackColor; }
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
