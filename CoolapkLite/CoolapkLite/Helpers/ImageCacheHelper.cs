using CoolapkLite.Common;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Helpers
{
    [Flags]
    public enum ImageType
    {
        Origin = 0x00,
        Small = 0x01,

        Image = 0x02,
        Avatar = 0x04,
        Icon = 0x08,
        Captcha = 0x16,
        Message = 0x32,

        OriginImage = Image | Origin,
        BigAvatar = Avatar | Origin,
        OriginMessage = Message | Origin,

        SmallImage = Image | Small,
        SmallAvatar = Avatar | Small,
        SmallMessage = Message | Small,
    }

    public static partial class ImageCacheHelper
    {
        private static readonly Uri DarkNoPicUri = new Uri("ms-appx:/Assets/NoPic/img_placeholder_night.png");
        private static readonly Uri WhiteNoPicUri = new Uri("ms-appx:/Assets/NoPic/img_placeholder.png");

        private static Dictionary<CoreDispatcher, BitmapImage> DarkNoPicMode { get; set; } = new Dictionary<CoreDispatcher, BitmapImage>();
        private static Dictionary<CoreDispatcher, BitmapImage> WhiteNoPicMode { get; set; } = new Dictionary<CoreDispatcher, BitmapImage>();

        static ImageCacheHelper() => ImageCache.Instance.CacheDuration = TimeSpan.FromHours(8);

        public static async Task<BitmapImage> GetImageAsync(ImageType type, string url, CoreDispatcher dispatcher, bool isForce = false)
        {
            Uri uri = type.HasFlag(ImageType.Message)
                ? await GetMessageImageUriAsync(type, url).ConfigureAwait(false)
                : url.TryGetUri();

            if (uri == null) { return null; }

            if (url.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase) || uri.IsFile)
            {
                await dispatcher.ResumeForegroundAsync();
                return new BitmapImage(uri);
            }
            else if (!isForce && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return null;
            }
            else
            {
                if (!type.HasFlag(ImageType.Message) && type.HasFlag(ImageType.Small))
                {
                    if (url.Contains("image.coolapk.com", StringComparison.OrdinalIgnoreCase)) { url += ".s.jpg"; }
                    uri = url.TryGetUri();
                }

                if (ImageCache.Instance.Dispatcher != dispatcher)
                {
                    ImageCache.Instance.Dispatcher = dispatcher;
                }

                try
                {
                    BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                    return image;
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        await ImageCache.Instance.RemoveAsync(new[] { uri });
                        BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        return image;
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        _ = dispatcher.ShowMessageAsync(str);
                        return null;
                    }
                }
                catch (Exception)
                {
                    string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    _ = dispatcher.ShowMessageAsync(str);
                    return null;
                }
            }
        }

        public static async Task<StorageFile> GetImageFileAsync(ImageType type, string url)
        {
            Uri uri = type.HasFlag(ImageType.Message)
                ? await GetMessageImageUriAsync(type, url).ConfigureAwait(false)
                : url.TryGetUri();

            if (uri == null) { return null; }

            if (url.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
            {
                return await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            else if (uri.IsFile)
            {
                return await StorageFile.GetFileFromPathAsync(url);
            }
            else
            {
                if (!type.HasFlag(ImageType.Message) && type.HasFlag(ImageType.Small))
                {
                    if (url.Contains("image.coolapk.com", StringComparison.OrdinalIgnoreCase)) { url += ".s.jpg"; }
                    uri = url.TryGetUri();
                }

                try
                {
                    StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    if (image == null)
                    {
                        _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    }
                    return image;
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        await ImageCache.Instance.RemoveAsync(new[] { uri });
                        _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        return image;
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        _ = UIHelper.ShowMessageAsync(str);
                        return null;
                    }
                }
                catch (Exception)
                {
                    string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    _ = UIHelper.ShowMessageAsync(str);
                    return null;
                }
            }
        }

        public static Task CleanCacheAsync() => ImageCache.Instance.ClearAsync();

        public static BitmapImage GetNoPic(CoreDispatcher dispatcher)
        {
            if (dispatcher == null) { return null; }

            if (!DarkNoPicMode.ContainsKey(dispatcher))
            {
                if (!dispatcher.HasThreadAccess) { return null; }
                DarkNoPicMode[dispatcher] = new BitmapImage(DarkNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
                WhiteNoPicMode[dispatcher] = new BitmapImage(WhiteNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
            }

            return ThemeHelper.IsDarkTheme()
                ? DarkNoPicMode[dispatcher]
                : WhiteNoPicMode[dispatcher];
        }

        public static async Task<BitmapImage> GetNoPicAsync(CoreDispatcher dispatcher)
        {
            await dispatcher.ResumeForegroundAsync();

            if (!DarkNoPicMode.ContainsKey(dispatcher))
            {
                DarkNoPicMode[dispatcher] = new BitmapImage(DarkNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
                WhiteNoPicMode[dispatcher] = new BitmapImage(WhiteNoPicUri) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
            }

            return await ThemeHelper.IsDarkThemeAsync().ConfigureAwait(false)
                ? DarkNoPicMode[dispatcher]
                : WhiteNoPicMode[dispatcher];
        }

        private static async Task<Uri> GetMessageImageUriAsync(ImageType type, string url)
        {
            url += type.HasFlag(ImageType.Small) ? "&type=s" : "&type=n";
            if (!url.TryGetUri(out Uri uri)) { return null; }
            using (HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            })
            using (HttpClient client = new HttpClient(handler))
            {
                TokenCreator token = new TokenCreator(SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion));

                NetworkHelper.SetRequestHeaders(client);
                client.DefaultRequestHeaders.Add("X-App-Token", token.GetToken());
                client.DefaultRequestHeaders.Add("X-Requested-With", NetworkHelper.XMLHttpRequest);

                foreach ((string name, string value) in NetworkHelper.GetCoolapkCookies(uri))
                {
                    handler.CookieContainer.SetCookies(NetworkHelper.GetHost(uri), $"{name}={value}");
                }

                HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);
                return response?.Headers.Location;
            }
        }
    }
}
