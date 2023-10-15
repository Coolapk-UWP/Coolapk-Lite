using CoolapkLite.Common;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkLite.Helpers
{
    public static partial class NetworkHelper
    {
        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        public static TokenCreator token;

        static NetworkHelper()
        {
            ThemeHelper.UISettingChanged.Add(arg => Client?.DefaultRequestHeaders?.ReplaceDarkMode());
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
                SettingsHelper.InvokeLoginChanged(Uid, true);
            }
        }

        public static void SetRequestHeaders()
        {
            token = new TokenCreator(SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion));
            SetRequestHeaders(Client);
        }

        public static async void SetRequestHeaders(HttpClient client)
        {
            string Culture = LanguageHelper.GetPrimaryLanguage();
            EasClientDeviceInformation DeviceInfo = new EasClientDeviceInformation();
            APIVersions APIVersion = SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture);
            client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            client.DefaultRequestHeaders.Add("X-App-Device", TokenCreator.DeviceCode);
            client.DefaultRequestHeaders.Add("X-Dark-Mode", await ThemeHelper.IsDarkThemeAsync() ? "1" : "0");

            if (SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA).ToString());
            }
            else
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalvik/2.1.0 (Windows NT {SystemInformation.Instance.OperatingSystemVersion.Major}.{SystemInformation.Instance.OperatingSystemVersion.Minor}; Win{(SystemInformation.Instance.OperatingSystemArchitecture.ToString().Contains("64") ? "64" : "32")}; {SystemInformation.Instance.OperatingSystemArchitecture.ToString().ToLower()}; WebView/3.0) (#Build; {DeviceInfo.SystemManufacturer}; {DeviceInfo.SystemProductName}; {DeviceInfo.SystemProductName}_{DeviceInfo.SystemSku}; {SystemInformation.Instance.OperatingSystemVersion})");
            }

            switch (APIVersion)
            {
                case APIVersions.V6:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case APIVersions.V7:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case APIVersions.V8:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.7-1809041-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "8.7");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1809041");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case APIVersions.V9:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.6.3-1910291-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "9.6.3");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1910291");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersions.小程序:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersions.V10:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case APIVersions.V11:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.4.7-2112231-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "11.4.7");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2112231");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                case APIVersions.V12:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/12.5.4-2212261-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "12.5.4");
                    client.DefaultRequestHeaders.Add("X-Api-Supported", "2212261");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2212261");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "12");
                    break;
                case APIVersions.V13:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/13.3.4-2309211-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "13.3.4");
                    client.DefaultRequestHeaders.Add("X-Api-Supported", "2309211");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2309211");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "13");
                    break;
                case APIVersions.Custom:
                    APIVersion CustomAPI = SettingsHelper.Get<APIVersion>(SettingsHelper.CustomAPI);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd($" {CustomAPI}");
                    client.DefaultRequestHeaders.Add("X-App-Version", CustomAPI.Version);
                    client.DefaultRequestHeaders.Add("X-Api-Supported", CustomAPI.VersionCode);
                    client.DefaultRequestHeaders.Add("X-App-Code", CustomAPI.VersionCode);
                    client.DefaultRequestHeaders.Add("X-Api-Version", CustomAPI.Version.Split('.').FirstOrDefault());
                    break;
                default:
                    break;
            }
        }

        public static IEnumerable<(string name, string value)> GetCoolapkCookies(Uri uri)
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                if (uri.Host.Contains("coolapk"))
                {
                    foreach (HttpCookie item in cookieManager.GetCookies(GetHost(uri)))
                    {
                        if (item.Name == "uid" ||
                            item.Name == "username" ||
                            item.Name == "token")
                        {
                            yield return (item.Name, item.Value);
                        }
                    }
                }
                else
                {
                    foreach (HttpCookie item in cookieManager.GetCookies(GetHost(uri)))
                    {
                        yield return (item.Name, item.Value);
                    }
                }
            }
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
                //await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                HttpResponseMessage response;
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
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

        public static async Task<HttpResponseMessage> GetAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                //await semaphoreSlim.WaitAsync().ConfigureAwait(false);
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

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                //await semaphoreSlim.WaitAsync().ConfigureAwait(false);
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

        public static async Task<string> GetStringAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                //await semaphoreSlim.WaitAsync().ConfigureAwait(false);
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
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                if (string.IsNullOrWhiteSpace(str)) { throw ex; }
                JObject token = JObject.Parse(str);
                if (token == null) { throw ex; }
                else { throw new CoolapkMessageException(token, ex); }
            }
        }

        public static Uri GetHost(Uri uri) => new Uri("https://" + uri.Host);

        public static string ExpandShortUrl(this Uri ShortUrl)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ShortUrl);
            try { _ = req.HaveResponse; }
            catch (WebException ex)
            {
                HttpWebResponse res = ex.Response as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.Found)
                { return res.Headers["Location"] ?? ShortUrl.ToString(); }
            }
            return ShortUrl.ToString();
        }

        public static async Task<Uri> ExpandShortUrlAsync(this Uri ShortUrl)
        {
            if (ShortUrl.Host == "s.click.taobao.com")
            {
                using (HttpClient request = new HttpClient())
                {
                    HttpResponseMessage response = await request.GetAsync(ShortUrl).ConfigureAwait(false);
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
                HttpResponseMessage res = await Client.GetAsync(ShortUrl).ConfigureAwait(false);
                return res.RequestMessage.RequestUri ?? ShortUrl;
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
                return url.Contains(":")
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
