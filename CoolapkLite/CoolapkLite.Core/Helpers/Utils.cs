using Html2Markdown;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.Core.Helpers
{
    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }

    public static partial class Utils
    {
        public static event EventHandler<(MessageType Type, string Message)> NeedShowInAppMessageEvent;

        internal static void ShowInAppMessage(MessageType type, string message = null)
        {
            NeedShowInAppMessageEvent?.Invoke(null, (type, message));
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { NeedShowInAppMessageEvent?.Invoke(null, (MessageType.Message, $"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}")); }
            else if (e.Message == "An error occurred while sending the request.") { NeedShowInAppMessageEvent?.Invoke(null, (MessageType.Message, "无法连接网络。")); }
            else { NeedShowInAppMessageEvent?.Invoke(null, (MessageType.Message, $"请检查网络连接。 {e.Message}")); }
        }

        public static string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                string r2 = BitConverter.ToString(r1).ToLowerInvariant();
                return r2.Replace("-", "");
            }
        }

        public static string GetBase64(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }
    }

    public static partial class Utils
    {
        public enum TimeIntervalType
        {
            MonthsAgo,
            DaysAgo,
            HoursAgo,
            MinutesAgo,
            JustNow,
        }

        private static readonly DateTime unixDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static (TimeIntervalType type, object time) ConvertUnixTimeStampToReadable(double time, DateTime baseTime)
        {
            TimeSpan ttime = new TimeSpan((long)time * 1000_0000);
            DateTime tdate = unixDateBase.Add(ttime);
            TimeSpan temp = baseTime.ToUniversalTime()
                                    .Subtract(tdate);

            if (temp.TotalDays > 30)
            {
                return (TimeIntervalType.MonthsAgo, tdate);
            }
            else
            {
                TimeIntervalType type = temp.Days > 0
                    ? TimeIntervalType.DaysAgo
                    : temp.Hours > 0 ? TimeIntervalType.HoursAgo : temp.Minutes > 0 ? TimeIntervalType.MinutesAgo : TimeIntervalType.JustNow;
                return (type, temp);
            }
        }

        public static DateTime ConvertUnixTimeStampToDateTime(this double time) => unixDateBase.Add(new TimeSpan((long)time * 1000_0000));

        public static double ConvertDateTimeToUnixTimeStamp(this DateTime time) => Math.Round(time.ToUniversalTime().Subtract(unixDateBase).TotalSeconds);
    }

    public static partial class Utils
    {
        public static string GetSizeString(this double size)
        {
            int index = 0;
            while (index <= 11)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                case 5: str = "PB"; break;
                case 6: str = "EB"; break;
                case 7: str = "ZB"; break;
                case 8: str = "YB"; break;
                case 9: str = "BB"; break;
                case 10: str = "NB"; break;
                case 11: str = "DB"; break;
                default:
                    break;
            }
            return $"{size:0.##}{str}";
        }

        public static string GetNumString(this double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }

        public static string CSStoMarkDown(this string text)
        {
            try
            {
                Converter converter = new Converter();
                return converter.Convert(text);
            }
            catch
            {
                Regex h1 = new Regex(@"<h1.*?>", RegexOptions.IgnoreCase);
                Regex h2 = new Regex(@"<h2.*?>", RegexOptions.IgnoreCase);
                Regex h3 = new Regex(@"<h3.*?>", RegexOptions.IgnoreCase);
                Regex h4 = new Regex(@"<h4.*?>\n", RegexOptions.IgnoreCase);
                Regex div = new Regex(@"<div.*?>", RegexOptions.IgnoreCase);
                Regex p = new Regex(@"<p.*?>", RegexOptions.IgnoreCase);
                Regex ul = new Regex(@"<ul.*?>", RegexOptions.IgnoreCase);
                Regex li = new Regex(@"<li.*?>", RegexOptions.IgnoreCase);
                Regex span = new Regex(@"<span.*?>", RegexOptions.IgnoreCase);

                text = text.Replace("</h1>", "");
                text = text.Replace("</h2>", "");
                text = text.Replace("</h3>", "");
                text = text.Replace("</h4>", "");
                text = text.Replace("</div>", "");
                text = text.Replace("<p>", "");
                text = text.Replace("</p>", "");
                text = text.Replace("</ul>", "");
                text = text.Replace("</li>", "");
                text = text.Replace("</span>", "**");
                text = text.Replace("</strong>", "**");

                text = h1.Replace(text, "#");
                text = h2.Replace(text, "##");
                text = h3.Replace(text, "###");
                text = h4.Replace(text, "####");
                text = text.Replace("<br/>", "  \n");
                text = text.Replace("<br />", "  \n");
                text = div.Replace(text, "");
                text = p.Replace(text, "");
                text = ul.Replace(text, "");
                text = li.Replace(text, " - ");
                text = span.Replace(text, "**");
                text = text.Replace("<strong>", "**");

                for (int i = 0; i < 20; i++) { text = text.Replace("(" + i.ToString() + ") ", " 1. "); }

                return text;
            }
        }

        public static string MarkDowntoCSS(this string text) => CommonMark.CommonMarkConverter.Convert(text);

        public static string CSStoString(this string str)
        {
            try
            {
                HtmlToText HtmlToText = new HtmlToText();
                return HtmlToText.Convert(str);
            }
            catch
            {
                //换行和段落
                string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
                //链接彻底删除！
                while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
                {
                    s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                    s = s.Replace("</a>", "");
                }
                return s;
            }
        }

        public static string ConvertJsonString(this string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = null;
            try { obj = serializer.Deserialize(jtr); } catch { }
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }

    public static partial class Utils
    {
        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (T cur in e)
            {
                yield return cur;
            }
            yield return value;
        }

        public static IEnumerable Append(this IEnumerable first, params object[] second)
        {
            return first.OfType<object>().Concat(second);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Concat(second);
        }

        public static IEnumerable Prepend(this IEnumerable first, params object[] second)
        {
            return second.Concat(first.OfType<object>());
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, params T[] second)
        {
            return second.Concat(first);
        }
    }

    public static partial class Utils
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

        public static async Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool isBackground, bool forceRefresh = false)
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
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
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
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult(json);
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return (DateTime.Now - ResponseCache[info.uri][info.page].date).TotalMinutes > 2; } }) && IsInternetAvailable)
            {
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
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

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, bool isBackground, bool forceRefresh = false)
        {
            string json = string.Empty;
            (int page, Uri uri) info = uri.GetPage();
            (bool isSucceed, string result) result;

            (bool isSucceed, string result) GetResult()
            {
                if (string.IsNullOrEmpty(json))
                {
                    ShowInAppMessage(MessageType.Message, "加载失败");
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
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
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
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
                result = GetResult();
                if (!result.isSucceed) { return result; }
                lock (locker)
                {
                    ResponseCache[info.uri].Add(info.page, (DateTime.Now, json));
                }
            }
            else if (await Task.Run(() => { lock (locker) { return (DateTime.Now - ResponseCache[info.uri][info.page].date).TotalMinutes > 2; } }) && IsInternetAvailable)
            {
                json = await NetworkHelper.GetSrtingAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
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
                ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, null);
            }
            if (!o.TryGetValue("data", out JToken token) && o.TryGetValue("message", out JToken message))
            {
                ShowInAppMessage(MessageType.Message, message.ToString());
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
    }
}
