using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Helpers
{
    public enum ImageType
    {
        Icon,
        Captcha,
        BigAvatar,
        SmallImage,
        OriginImage,
        SmallAvatar,
    }

    internal static partial class ImageCacheHelper
    {
        private static readonly BitmapImage WhiteNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        private static readonly BitmapImage DarkNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder_night.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        internal static BitmapImage NoPic { get => SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode) ? DarkNoPicMode : WhiteNoPicMode; }

        static ImageCacheHelper()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromHours(8);
        }

        internal static async Task<BitmapImage> GetImageAsync(ImageType type, string url, Pages.ShowImageModel model = null)
        {
            try { new Uri(url); } catch { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return new BitmapImage(new Uri(url));
            }
            else if (model == null && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    url += ".s.jpg";
                }
                Uri uri = new Uri(url);

                try
                {
                    if (model != null)
                    {
                        model.IsProgressRingActived = true;
                    }
                    BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                    return image;
                }
                catch
                {
                    string str = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    UIHelper.ShowMessage(str);
                    return NoPic;
                }
                finally
                {
                    if (model != null)
                    {
                        model.IsProgressRingActived = false;
                    }
                }
            }
        }

        internal static async Task<StorageFile> GetImageFileAsync(ImageType type, string url)
        {
            try { new Uri(url); } catch { return null; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
            }
            else if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return null;
            }
            else
            {
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    url += ".s.jpg";
                }
                Uri uri = new Uri(url);
                return await ImageCache.Instance.GetFileFromCacheAsync(uri);
            }
        }

        internal static Task CleanCacheAsync()
        {
            return ImageCache.Instance.ClearAsync();
        }
    }

    internal static partial class ImageCacheHelper
    {
        [Obsolete]
        private static readonly Dictionary<ImageType, StorageFolder> folders = new Dictionary<ImageType, StorageFolder>();

        [Obsolete]
        internal static async Task<StorageFolder> GetFolderAsync(ImageType type)
        {
            StorageFolder folder;
            if (folders.ContainsKey(type))
            {
                folder = folders[type];
            }
            else
            {
                folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(type.ToString()) as StorageFolder;
                if (folder is null)
                {
                    folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString(), CreationCollisionOption.OpenIfExists);
                }
                if (!folders.ContainsKey(type))
                {
                    folders.Add(type, folder);
                }
            }
            return folder;
        }

        [Obsolete]
        internal static async Task<BitmapImage> GetImageAsyncOld(ImageType type, string url, Pages.ShowImageModel model = null)
        {
            try { new Uri(url); } catch { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return new BitmapImage(new Uri(url));
            }
            else if (model == null && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                string fileName = Core.Helpers.Utils.GetMD5(url);
                StorageFolder folder = await GetFolderAsync(type);
                IStorageItem item = await folder.TryGetItemAsync(fileName);
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    url += ".s.jpg";
                }
                bool forceGetPic = model != null;
                if (item is null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    return await DownloadImageAsync(file, url, model);
                }
                else
                {
                    return item is StorageFile file ? GetLocalImageAsync(file.Path, forceGetPic) : null;
                }
            }
        }

        [Obsolete]
        private static BitmapImage GetLocalImageAsync(string filename, bool forceGetPic)
        {
            try
            {
                return (filename is null || (!forceGetPic && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))) ? NoPic : new BitmapImage(new Uri(filename));
            }
            catch
            {
                return NoPic;
            }
        }

        [Obsolete]
        private static async Task<BitmapImage> DownloadImageAsync(StorageFile file, string url, Pages.ShowImageModel model)
        {
            try
            {
                if (model != null)
                {
                    model.IsProgressRingActived = true;
                }
                using (HttpClient hc = new HttpClient())
                using (Stream stream = await hc.GetStreamAsync(new Uri(url)))
                using (Stream fs = await file.OpenStreamForWriteAsync())
                {
                    await stream.CopyToAsync(fs);
                }
                return new BitmapImage(new Uri(file.Path));
            }
            catch (FileLoadException) { return NoPic; }
            catch (HttpRequestException)
            {
                string str = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return NoPic;
            }
            finally
            {
                if (model != null)
                {
                    model.IsProgressRingActived = false;
                }
            }
        }

        [Obsolete]
        internal static async Task CleanOldVersionImageCacheAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                ImageType type = (ImageType)i;
                await (await GetFolderAsync(type)).DeleteAsync();
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString());
            }
        }

        internal static async Task CleanCaptchaCacheAsync()
        {
#pragma warning disable 0612
            await (await GetFolderAsync(ImageType.Captcha)).DeleteAsync();
#pragma warning restore 0612
            await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Captcha");
        }
    }
}
