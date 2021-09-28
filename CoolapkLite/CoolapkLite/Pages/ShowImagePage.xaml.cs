using CoolapkLite.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using InAppNotify = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowImagePage : Page
    {
        public ShowImagePage()
        {
            this.InitializeComponent();
        }
    }

    internal class ImageModel : INotifyPropertyChanged
    {
        private bool isProgressRingActived;
        private WeakReference<BitmapImage> pic;
        private readonly InAppNotify inAppNotify;
        private readonly CoreDispatcher dispatcher;

        public string Uri { get; }
        public ImageType Type { get; private set; }
        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF"); }

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
            private set
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

        public bool IsProgressRingActived
        {
            get => isProgressRingActived;
            set
            {
                isProgressRingActived = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (name != null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            });
        }

        public ImageModel(string uri, ImageType type, InAppNotify notify, CoreDispatcher dispatcher)
        {
            Uri = uri;
            Type = IsGif || SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture) ? ChangeType(type) : type;
            inAppNotify = notify;
            this.dispatcher = dispatcher;
        }

        //public ImageModel(Models.ImageModel model, InAppNotify notify, CoreDispatcher dispatcher) : this(model.Uri, model.Type, notify, dispatcher)
        //{
        //}

        private async void GetImage()
        {
            BitmapImage bitmapImage = null;
            while (bitmapImage is null)
            {
#pragma warning disable 0612
                bitmapImage = await ImageCacheHelper.GetImageAsyncOld(Type, Uri, this, inAppNotify);
#pragma warning restore 0612
            }
            await Task.Delay(20);
            Pic = bitmapImage;
        }

        private static ImageType ChangeType(ImageType type)
        {
            switch (type)
            {
                case ImageType.SmallImage:
                    return ImageType.OriginImage;

                case ImageType.SmallAvatar:
                    return ImageType.BigAvatar;

                default:
                    return type;
            }
        }

        public void ChangeType()
        {
            Type = ChangeType(Type);
            GetImage();
        }
    }
}
