using CoolapkLite.Core.Exceptions;
using CoolapkLite.Models;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.System.Profile;
using Windows.System.UserProfile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Helpers
{
    public static partial class NetworkHelper
    {
        public static readonly HttpClientHandler ClientHandler = new HttpClientHandler();
        public static readonly HttpClient Client = new HttpClient(ClientHandler);
        private static readonly string Guid = System.Guid.NewGuid().ToString();

        static NetworkHelper()
        {
            SetRequestHeaders();
        }

        public static void SetRequestHeaders()
        {
            CultureInfo Culture = null;
            ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            string Version = SettingsHelper.Get<string>(SettingsHelper.APIVersion);
            try { Culture = GlobalizationPreferences.Languages.Count > 0 ? new CultureInfo(GlobalizationPreferences.Languages.First()) : null; } catch { }
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            Client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            Client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            Client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            Client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            Client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture == null ? "zh-CN" : Culture.ToString());
            Client.DefaultRequestHeaders.Add("X-Dark-Mode", Application.Current.RequestedTheme.ToString() == "Dark" ? "1" : "0");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd("Dalvik/2.1.0 (Windows NT " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + (Package.Current.Id.Architecture.ToString().Contains("64") ? "; Win64; " : "; Win32; ") + Package.Current.Id.Architecture.ToString().Replace("X", "x") + "; WebView/3.0) (#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; CoolapkUWP; " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + "." + (ushort)((version & 0x00000000FFFF0000L) >> 16) + "." + (ushort)(version & 0x000000000000FFFFL) + ")");
            switch (Version)
            {
                case "V6":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case "V7":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case "V8":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.4.1-1806141-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "8.4.1");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1806141");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case "V9":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "小程序":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "V10":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case "V11":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.4.7-2112231-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "11.4.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2112231");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                case "V12":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/12.4-2207192-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "12.4");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2207192");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2207192");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "12");
                    break;
                default:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
            }
            Client.DefaultRequestHeaders.Add("X-App-Device", GetCoolapkDeviceID());
        }

        public static IEnumerable<(string name, string value)> GetCoolapkCookies(Uri uri)
        {
            using (Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                Windows.Web.Http.HttpCookieManager cookieManager = filter.CookieManager;
                foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(GetHost(uri)))
                {
                    if (item.Name == "uid" ||
                        item.Name == "username" ||
                        item.Name == "token")
                    {
                        yield return (item.Name, item.Value);
                    }
                }
            }
        }

        private static string GetCoolapkDeviceID()
        {
            string token = SettingsHelper.Get<string>(SettingsHelper.DeviceID);
            if (string.IsNullOrEmpty(token))
            {
                Guid easId = new EasClientDeviceInformation().Id;
                string md5_easID = DataHelper.GetMD5(easId.ToString());
                string base64 = md5_easID;
                for (int i = 0; i < 5; i++)
                {
                    base64 = DataHelper.GetBase64(base64);
                }
                token = base64.Replace("=", "");
                SettingsHelper.Set(SettingsHelper.DeviceID, token);
            }
            return token;
        }

        private static string GetCoolapkAppToken()
        {
            double t = DateTime.Now.ConvertDateTimeToUnixTimeStamp();
            string hex_t = "0x" + Convert.ToString((int)t, 16);
            // 时间戳加密
            string md5_t = DataHelper.GetMD5($"{t}");
            string a = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_t}${Guid}&com.coolapk.market";
            string md5_a = DataHelper.GetMD5(DataHelper.GetBase64(a));
            string token = md5_a + Guid + hex_t;
            return token;
        }

        private static void ReplaceAppToken(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            _ = headers.Remove(name);
            headers.Add(name, GetCoolapkAppToken());
        }

        private static void ReplaceRequested(this System.Net.Http.Headers.HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies, Uri uri)
        {
            if (cookies == null) { return; }

            //var c = container.GetCookies(UriHelper.CoolapkUri);
            foreach ((string name, string value) in cookies)
            {
                container.SetCookies(GetHost(uri), $"{name}={value}");
            }
        }

        private static void BeforeGetOrPost(IEnumerable<(string name, string value)> coolapkCookies, Uri uri, string request)
        {
            ClientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies, uri);
            Client.DefaultRequestHeaders.ReplaceAppToken();
            Client.DefaultRequestHeaders.ReplaceRequested(request);
        }

    }

    public static partial class NetworkHelper
    {
        public static async Task<string> PostAsync(Uri uri, HttpContent content, IEnumerable<(string name, string value)> coolapkCookies, bool isBackground)
        {
            try
            {
                HttpResponseMessage response;
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
                response = await Client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStreamAsync(uri);
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetSrtingAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStringAsync(uri);
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public static partial class NetworkHelper
    {
        private static bool IsInternetAvailable => Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;
        private static readonly Dictionary<Uri, Dictionary<int, (DateTime date, string data)>> ResponseCache = new Dictionary<Uri, Dictionary<int, (DateTime, string)>>();
        private static readonly object locker = new object();

        internal static readonly Timer CleanCacheTimer = new Timer(o =>
        {
            if (IsInternetAvailable)
            {
                DateTime now = DateTime.Now;
                lock (locker)
                {
                    foreach (KeyValuePair<Uri, Dictionary<int, (DateTime date, string data)>> i in ResponseCache)
                    {
                        int[] needDelete = (from j in i.Value
                                            where (now - j.Value.date).TotalMinutes > 2
                                            select j.Key).ToArray();
                        foreach (int item in needDelete)
                        {
                            _ = ResponseCache[i.Key].Remove(item);
                        }
                    }
                }
            }
        }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

        public static async Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool isBackground=false, bool forceRefresh = false)
        {
            string json = string.Empty;
            (int page, Uri uri) info = uri.GetPage();
            (bool isSucceed, JToken result) result;

            void ReadCache()
            {
                lock (locker)
                {
                    json = ResponseCache[info.uri][info.page].data;
                }
                result = GetResult(json);
            }

            if (forceRefresh && IsInternetAvailable)
            {
                lock (locker)
                {
                    ResponseCache.Remove(info.uri);
                }
            }

            if (await Task.Run(() => { lock (locker) { return !ResponseCache.ContainsKey(info.uri); } }))
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult(json);
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache.Add(info.uri, new Dictionary<int, (DateTime date, string data)>());
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return !ResponseCache[info.uri].ContainsKey(info.page); } }))
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult(json);
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return (DateTime.Now - ResponseCache[info.uri][info.page].date).TotalMinutes > 2; } }) && IsInternetAvailable)
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult(json);
                if (!result.isSucceed) { ReadCache(); }
                lock (locker)
                {
                    ResponseCache[info.uri][info.page] = (DateTime.Now, json);
                }
            }
            else
            {
                ReadCache();
            }
            return result;
        }

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, bool isBackground=false, bool forceRefresh = false)
        {
            string json = string.Empty;
            (int page, Uri uri) info = uri.GetPage();
            (bool isSucceed, string result) result;

            (bool isSucceed, string result) GetResult()
            {
                if (string.IsNullOrEmpty(json))
                {
                    UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                    return (false, null);
                }
                else { return (true, json); }
            }

            if (forceRefresh)
            {
                lock (locker)
                {
                    ResponseCache.Remove(info.uri);
                }
            }

            if (await Task.Run(() => { lock (locker) { return !ResponseCache.ContainsKey(info.uri); } }))
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult();
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache.Add(info.uri, new Dictionary<int, (DateTime date, string data)>());
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return !ResponseCache[info.uri].ContainsKey(info.page); } }))
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult();
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return (DateTime.Now - ResponseCache[info.uri][info.page].date).TotalMinutes > 2; } }) && IsInternetAvailable)
            {
                json = await GetSrtingAsync(uri, GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult();
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache[info.uri][info.page] = (DateTime.Now, json);
                }
            }
            else
            {
                lock (locker)
                {
                    json = ResponseCache[info.uri][info.page].data;
                }
                result = GetResult();
                if (!result.isSucceed) { return result; }
            }
            return result;
        }

        public static async Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, HttpContent content=null, bool isBackground=false)
        {
            string json = string.Empty;
            (bool isSucceed, JToken result) result;

            json = await PostAsync(uri, content, GetCoolapkCookies(uri), isBackground);
            result = GetResult(json);

            return result;
        }

        public static async Task<(bool isSucceed, string result)> PostStringAsync(Uri uri, HttpContent content=null, bool isBackground=false, bool forceRefresh = false)
        {
            string json = string.Empty;
            (bool isSucceed, string result) result;

            (bool isSucceed, string result) GetResult()
            {
                if (string.IsNullOrEmpty(json))
                {
                    UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                    return (false, null);
                }
                else { return (true, json); }
            }

            json = await PostAsync(uri, content, GetCoolapkCookies(uri), isBackground);
            result = GetResult();

            return result;
        }

        private static (int page, Uri uri) GetPage(this Uri uri)
        {
            Regex pageregex = new Regex(@"([&|?])page=(\d+)(\??)");
            if (pageregex.IsMatch(uri.ToString()))
            {
                int pagenum = Convert.ToInt32(pageregex.Match(uri.ToString()).Groups[2].Value);
                Uri baseuri = new Uri(pageregex.Match(uri.ToString()).Groups[3].Value == "?" ? pageregex.Replace(uri.ToString(), pageregex.Match(uri.ToString()).Groups[1].Value) : pageregex.Replace(uri.ToString(), string.Empty));
                return (pagenum, baseuri);
            }
            else
            {
                return (0, uri);
            }
        }

        private static (bool isSucceed, JToken result) GetResult(string json)
        {
            if (string.IsNullOrEmpty(json)) { return (false, null); }
            JObject o;
            try { o = JObject.Parse(json); }
            catch
            {
                UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, null);
            }
            if (!o.TryGetValue("data", out JToken token) && o.TryGetValue("message", out JToken message))
            {
                UIHelper.ShowInAppMessage(MessageType.Message, message.ToString());
                return (false, null);
            }
            else { return (!string.IsNullOrEmpty(token.ToString()), token); }
        }

        public static string GetId(JToken token, string _idName)
        {
            return token == null
                ? string.Empty
                : (token as JObject).TryGetValue(_idName, out JToken jToken)
                    ? jToken.ToString()
                    : (token as JObject).TryGetValue("entityId", out JToken v1)
                                    ? v1.ToString()
                                    : (token as JObject).TryGetValue("id", out JToken v2) ? v2.ToString() : throw new ArgumentException(nameof(_idName));
        }

#pragma warning disable 0612
        public static async Task<BitmapImage> GetImageAsync(string uri, bool isBackground = false)
        {
            StorageFolder folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            StorageFile file = await folder.CreateFileAsync(DataHelper.GetMD5(uri));

            Stream s = await GetStreamAsync(new Uri(uri), GetCoolapkCookies(new Uri(uri)), "XMLHttpRequest", isBackground);

            using (Stream ss = await file.OpenStreamForWriteAsync())
            {
                await s.CopyToAsync(ss);
            }

            return new BitmapImage(new Uri(file.Path));
        }
#pragma warning restore 0612

        public static async Task ChangeLikeAsync(this ICanChangeLikeModel model, CoreDispatcher dispatcher)
        {
            if (model == null) { return; }

            bool isReply = model is FeedReplyModel;
            Uri u = UriHelper.GetUri(
                model.Liked ? UriType.OperateUnlike : UriType.OperateLike,
                isReply ? "Reply" : string.Empty,
                model.ID);
            (bool isSucceed, JToken result) = await PostDataAsync(u, null, true);
            if (!isSucceed) { return; }

            JObject o = result as JObject;
            await dispatcher.AwaitableRunAsync(() =>
            {
                model.Liked = !model.Liked;
                if (isReply)
                {
                    model.LikeNum = Convert.ToInt32(o.ToString().Replace("\"", string.Empty));
                }
                else if (o != null)
                {
                    model.LikeNum = o.Value<int>("count");
                }
            });
        }

        public static async void ChangeFollow(this ICanFollowModel model, CoreDispatcher dispatcher)
        {
            UriType type = model.Followed ? UriType.OperateUnfollow : UriType.OperateFollow;

            (bool isSucceed, _) = await PostDataAsync(UriHelper.GetUri(type, model.UID), null, true);
            if (!isSucceed) { return; }

            await dispatcher.AwaitableRunAsync(() => model.Followed = !model.Followed);
        }
    }

    public static partial class NetworkHelper
    {
        /// <summary> 通过用户名获取UID。 </summary>
        /// <param name="name"> 要获取UID的用户名。 </param>
        public static async Task<string> GetUserIDByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new UserNameErrorException();
            }

            string str = string.Empty;
            try
            {
                str = await Client.GetStringAsync(new Uri("https://www.coolapk.com/n/" + name));
                return $"{JObject.Parse(str)["dataRow"].Value<int>("uid")}";
            }
            catch
            {
                JObject o = JObject.Parse(str);
                if (o == null) { throw; }
                else
                {
                    throw new CoolapkMessageException(o);
                }
            }
        }

        public static Uri GetHost(Uri uri)
        {
            return new Uri("https://" + uri.Host);
        }

        public static string ExpandShortUrl(this Uri ShortUrl)
        {
            string NativeUrl = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ShortUrl);
            try { _ = req.HaveResponse; }
            catch (WebException ex)
            {
                HttpWebResponse res = ex.Response as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.Found)
                { NativeUrl = res.Headers["Location"]; }
            }
            return NativeUrl ?? ShortUrl.ToString();
        }

        public static Uri ValidateAndGetUri(this string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
            }
            return uri;
        }
    }
}
