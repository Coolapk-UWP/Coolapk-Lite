using CoolapkLite.Common;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkLite.Helpers
{
    public static partial class NetworkHelper
    {
        public const string XMLHttpRequest = "XMLHttpRequest";

        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        public static TokenCreator token;

        static NetworkHelper()
        {
            ThemeHelper.UISettingChanged += arg => Client?.DefaultRequestHeaders?.ReplaceDarkMode();
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);
            SetRequestHeaders();
            SetLoginCookie();
        }

        public static void SetLoginCookie()
        {
            string Uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
            string UserName = SettingsHelper.Get<string>(SettingsHelper.UserName);
            string Token = SettingsHelper.Get<string>(SettingsHelper.Token);

            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = Uid;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
                SettingsHelper.InvokeLoginChanged(true);
            }
        }

        public static void SetRequestHeaders()
        {
            token = new TokenCreator(SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion));
            SetRequestHeaders(Client);
        }

        public static async void SetRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "33");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", LanguageHelper.GetPrimaryLanguage());
            client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            client.DefaultRequestHeaders.Add("X-App-Device", TokenCreator.DeviceCode);
            client.DefaultRequestHeaders.Add("X-Dark-Mode", await ThemeHelper.IsDarkThemeAsync() ? "1" : "0");

            if (!SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA))
            {
                SettingsHelper.Set(SettingsHelper.CustomUA, UserAgent.Default);
            }
            client.DefaultRequestHeaders.UserAgent.ParseAdd(SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA).ToString());

            APIVersion version = APIVersion.Create(SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion));
            client.DefaultRequestHeaders.UserAgent.ParseAdd($" {version}");
            client.DefaultRequestHeaders.Add("X-App-Version", version.Version);
            client.DefaultRequestHeaders.Add("X-Api-Supported", version.VersionCode);
            client.DefaultRequestHeaders.Add("X-App-Code", version.VersionCode);
            client.DefaultRequestHeaders.Add("X-Api-Version", version.MajorVersion);
        }

        private static void ReplaceDarkMode(this HttpRequestHeaders headers)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, ThemeHelper.IsDarkTheme() ? "1" : "0");
        }

        private static void ReplaceAppToken(this HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            _ = headers.Remove(name);
            headers.Add(name, token.GetToken());
        }

        private static void ReplaceRequested(this HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies, Uri uri)
        {
            if (cookies == null) { return; }

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
                BeforeGetOrPost(coolapkCookies, uri, XMLHttpRequest);
                response = await Client.PostAsync(uri, content).ConfigureAwait(false);
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

        public static async Task<HttpResponseMessage> GetAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetAsync(uri).ConfigureAwait(false);
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

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStreamAsync(uri).ConfigureAwait(false);
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

        public static async Task<string> GetStringAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStringAsync(uri).ConfigureAwait(false);
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
                str = await Client.GetStringAsync(new Uri($"https://www.coolapk.com/n/{name}")).ConfigureAwait(false);
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

        public static Uri GetHost(Uri uri) => new Uri("https://" + uri.Host);

        public static string ExpandShortUrl(this Uri shortUrl)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(shortUrl);
            try { _ = req.HaveResponse; }
            catch (WebException ex)
            {
                HttpWebResponse res = ex.Response as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.Found)
                { return res.Headers["Location"] ?? shortUrl.ToString(); }
            }
            return shortUrl.ToString();
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
                using (HttpClientHandler handler = new HttpClientHandler()
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
                    : url[0] == '/'
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
