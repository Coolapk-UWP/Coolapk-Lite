using CoolapkLite.Common;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

namespace CoolapkLite.Helpers
{
    public static partial class NetworkHelper
    {
        private static readonly object appTokenLock = new object();
        private static readonly TimeSpan timeout = TimeSpan.FromTicks(863970000000 / 2);

        private static DateTimeOffset lastUpdate;

        public const string XMLHttpRequest = "XMLHttpRequest";

        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        public static TokenCreator TokenCreator;

        static NetworkHelper()
        {
            ClientHandler = new HttpClientHandler { MaxConnectionsPerServer = 20 };
            Client = new HttpClient(ClientHandler);
            ThemeHelper.UISettingChanged += arg => Client.DefaultRequestHeaders.ReplaceDarkMode(arg);
            SettingsHelper.LoginChanged += arg => UpdateCoolapkCookie();
            SetRequestHeaders();
        }

        public static void SetRequestHeaders()
        {
            TokenCreator = new TokenCreator(SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion));
            SetRequestHeaders(Client, ClientHandler);
            Client.DefaultRequestHeaders.ReplaceAppToken(true);
        }

        public static void SetRequestHeaders(HttpClient client, HttpClientHandler handler = null)
        {
            HttpRequestHeaders headers = client.DefaultRequestHeaders;

            headers.Clear();
            headers.Add("X-Sdk-Int", "36");
            headers.Add("X-Sdk-Locale", LanguageHelper.GetPrimaryLanguage());
            headers.Add("X-App-Mode", "universal");
            headers.Add("X-App-Channel", "coolapk");
            headers.Add("X-App-Id", "com.coolapk.market");
            headers.Add("X-App-Device", TokenCreator.DeviceCode);
            if (Window.Current != null)
            {
                headers.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            }

            bool isCustomUA = SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            headers.UserAgent.ParseAdd((isCustomUA ? SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA) : UserAgent.Default).ToString());

            APIVersion version = TokenCreator.APIVersion;
            headers.UserAgent.ParseAdd($" {version}");
            headers.Add("X-App-Version", version.Version);
            headers.Add("X-Api-Supported", version.VersionCode.ToString());
            headers.Add("X-App-Code", version.VersionCode.ToString());
            headers.Add("X-Api-Version", version.MajorVersion);

            handler?.CookieContainer.ReplaceCoolapkCookie();
        }

        public static void SetRequestHeaders(Windows.Web.Http.HttpClient client)
        {
            HttpRequestHeaderCollection headers = client.DefaultRequestHeaders;

            headers.Clear();
            headers.Add("X-Sdk-Int", "33");
            headers.Add("X-Sdk-Locale", LanguageHelper.GetPrimaryLanguage());
            headers.Add("X-App-Mode", "universal");
            headers.Add("X-App-Channel", "coolapk");
            headers.Add("X-App-Id", "com.coolapk.market");
            headers.Add("X-App-Device", TokenCreator.DeviceCode);
            if (Window.Current != null)
            {
                headers.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            }

            bool isCustomUA = SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            headers.UserAgent.ParseAdd((isCustomUA ? SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA) : UserAgent.Default).ToString());

            APIVersion version = TokenCreator.APIVersion;
            headers.UserAgent.ParseAdd($" {version}");
            headers.Add("X-App-Version", version.Version);
            headers.Add("X-Api-Supported", version.VersionCode.ToString());
            headers.Add("X-App-Code", version.VersionCode.ToString());
            headers.Add("X-Api-Version", version.MajorVersion);
        }

        public static void UpdateDeviceInfo(DeviceInfo deviceInfo)
        {
            SettingsHelper.Set(SettingsHelper.DeviceInfo, deviceInfo);
            TokenCreator.UpdateDeviceInfo(deviceInfo);
            SetRequestHeaders();
        }

        public static void UpdateAPIVersion(APIVersions version)
        {
            SettingsHelper.Set(SettingsHelper.APIVersion, version);
            TokenCreator.UpdateAPIVersion(version);
            SetRequestHeaders();
        }

        public static void UpdateCoolapkCookie() => ClientHandler.CookieContainer.ReplaceCoolapkCookie();

        private static HttpCookieCollection GetCoolapkCookies(Uri uri)
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                return cookieManager.GetCookies(uri);
            }
        }

        private static void ReplaceDarkMode(this HttpRequestHeaders headers, ApplicationTheme theme)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, theme == ApplicationTheme.Dark ? "1" : "0");
        }

        private static void ReplaceAppToken(this HttpRequestHeaders headers, bool forces = false)
        {
            lock (appTokenLock)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (forces || now - lastUpdate > timeout)
                {
                    lastUpdate = now;
                    const string name = "X-App-Token";
                    _ = headers.Remove(name);
                    headers.Add(name, TokenCreator.GetToken());
                }
            }
        }

        private static void ReplaceRequested(this HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container)
        {
            Uri host = new Uri("http://coolapk.com");
            container.GetCookies(host).OfType<Cookie>().ForEach(x => x.Expired = true);
            HttpCookieCollection cookies = GetCoolapkCookies(host);
            foreach (HttpCookie cookie in cookies)
            {
                container.Add(
#if !NETCORE463
                    host,
#endif
                    new Cookie(
                        cookie.Name,
                        cookie.Value,
                        cookie.Path,
                        cookie.Domain));
            }
        }
    }

    public static partial class NetworkHelper
    {
        private static readonly object requestedLock = new object();

        public static async Task<string> PostAsync(Uri uri, HttpContent content, bool isBackground)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<HttpResponseMessage> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(XMLHttpRequest);
                    task = Client.PostAsync(uri, content);
                }
                HttpResponseMessage response = await task.ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { _ = UIHelper.ShowHttpExceptionMessageAsync(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<HttpResponseMessage> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetAsync(uri);
                }
                return await task.ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { _ = UIHelper.ShowHttpExceptionMessageAsync(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<Stream> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetStreamAsync(uri);
                }
                return await task.ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { _ = UIHelper.ShowHttpExceptionMessageAsync(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }

        public static async Task<string> GetStringAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<string> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetStringAsync(uri);
                }
                return await task.ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { _ = UIHelper.ShowHttpExceptionMessageAsync(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }
    }

    public static partial class NetworkHelper
    {
        /// <summary>
        /// 通过用户名或 UID 获取用户信息。
        /// </summary>
        /// <param name="name">要获取信息的用户名或 UID 。</param>
        /// <param name="isBackground">是否通知错误。</param>
        /// <returns>用户信息</returns>
        public static async Task<UserInfoModel> GetUserInfoByNameAsync(string name, bool isBackground = false)
        {
            string str = string.Empty;
            try
            {
                str = await GetStringAsync(new Uri($"https://www.coolapk.com/n/{name}"), XMLHttpRequest, isBackground).ConfigureAwait(false);
                JObject token = JObject.Parse(str);
                if (token.TryGetValue("dataRow", out JToken v1))
                {
                    JObject dataRow = (JObject)v1;
                    return new UserInfoModel(dataRow);
                }
                return null;
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { _ = UIHelper.ShowHttpExceptionMessageAsync(e); }
                return null;
            }
        }

        public static async Task<Uri> ExpandShortUrlAsync(this Uri shortUrl)
        {
            if (shortUrl.Host == "s.click.taobao.com")
            {
                using (HttpClient request = new HttpClient())
                {
                    HttpResponseMessage response = await request.GetAsync(shortUrl).ConfigureAwait(false);
                    string urlA = response.RequestMessage.RequestUri.ToString();
                    string urlB = WebUtility.UrlDecode(urlA);
                    string urlC = urlB.Remove(0, 35);
                    request.DefaultRequestHeaders.Add("referer", urlB);
                    response = await request.GetAsync(urlC).ConfigureAwait(false);
                    return response.RequestMessage.RequestUri;
                }
            }
            else
            {
                using (HttpClientHandler handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false
                })
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage response = await client.GetAsync(shortUrl).ConfigureAwait(false);
                    return response?.Headers.Location ?? shortUrl;
                }
            }
        }

        public static Uri TryGetUri(this string url)
        {
            url.TryGetUri(out Uri uri);
            return uri;
        }

        public static bool TryGetUri(this string url, out Uri uri)
        {
            uri = default;
            if (string.IsNullOrWhiteSpace(url)) { return false; }
            try
            {
                return url.Contains(':')
                    ? Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)
                    : url.FirstOrDefault() == '/'
                        ? Uri.TryCreate(UriHelper.CoolapkUri, url, out uri)
                        : Uri.TryCreate($"https://{url}", UriKind.RelativeOrAbsolute, out uri);
            }
            catch (FormatException ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Warn(ex.ExceptionToMessage(), ex);
            }
            return false;
        }
    }
}
