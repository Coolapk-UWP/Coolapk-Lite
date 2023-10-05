using CoolapkLite.Common;
using CoolapkLite.Models.Upload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;


#if NETCORE463
using System.Linq;
#else
using System.Net.Http.Headers;
using CoolapkLite.Models.Network;
using Windows.Foundation.Collections;
#endif

namespace CoolapkLite.Helpers
{
    public static class RequestHelper
    {
        private static bool IsInternetAvailable => mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;
        private static readonly object locker = new object();

        public static async Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool isBackground = false)
        {
            if (uri == null) { return (false, null); }
            string results = await NetworkHelper.GetStringAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground).ConfigureAwait(false);
            if (string.IsNullOrEmpty(results)) { return (false, null); }
            JObject token;
            try { token = JObject.Parse(results); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                UIHelper.ShowMessage(message.ToString());
                return (false, null);
            }
            else { return (data != null && !string.IsNullOrWhiteSpace(data.ToString()), data); }
        }

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, string request = "com.coolapk.market", bool isBackground = false)
        {
            if (uri == null) { return (false, null); }
            string results = await NetworkHelper.GetStringAsync(uri, NetworkHelper.GetCoolapkCookies(uri), request, isBackground).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(results))
            {
                UIHelper.ShowMessage("加载失败");
                return (false, results);
            }
            else { return (true, results); }
        }

        public static async Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            if (uri == null) { return (false, null); }
            string json = await NetworkHelper.PostAsync(uri, content, NetworkHelper.GetCoolapkCookies(uri), isBackground).ConfigureAwait(false);
            if (string.IsNullOrEmpty(json)) { return (false, null); }
            JObject token;
            try { token = JObject.Parse(json); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                bool _isSucceed = token.TryGetValue("error", out JToken error) && error.ToObject<int>() == 0;
                UIHelper.ShowMessage(message.ToString());
                return (_isSucceed, token);
            }
            else
            {
                return data != null && !string.IsNullOrWhiteSpace(data.ToString())
                    ? (true, data)
                    : (token != null && !string.IsNullOrEmpty(token.ToString()), token);
            }
        }

        public static async Task<(bool isSucceed, string result)> PostStringAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            if (uri == null) { return (false, null); }
            string json = await NetworkHelper.PostAsync(uri, content, NetworkHelper.GetCoolapkCookies(uri), isBackground).ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            else { return (true, json); }
        }

        private static (int page, Uri uri) GetPage(this Uri uri)
        {
            Regex pageRegex = new Regex(@"([&?])page=(\\d+)(\\??)");
            Match match = pageRegex.Match(uri.ToString());
            if (match.Success)
            {
                int pageNum = Convert.ToInt32(match.Groups[2].Value);
                Uri baseUri = new Uri(match.Groups[3].Value == "?" ? pageRegex.Replace(uri.ToString(), match.Groups[1].Value) : pageRegex.Replace(uri.ToString(), string.Empty));
                return (pageNum, baseUri);
            }
            else
            {
                return (0, uri);
            }
        }

#pragma warning disable 0612
        public static async Task<BitmapImage> GetImageAsync(string uri, CoreDispatcher dispatcher, bool isBackground = false)
        {
            StorageFolder folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha).ConfigureAwait(false);
            StorageFile file = await folder.CreateFileAsync(DataHelper.GetMD5(uri));

            using (Stream stream = await NetworkHelper.GetStreamAsync(new Uri(uri), NetworkHelper.GetCoolapkCookies(new Uri(uri)), "XMLHttpRequest", isBackground).ConfigureAwait(false))
            using (Stream fileStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            if (dispatcher?.HasThreadAccess == false)
            {
                await dispatcher.ResumeForegroundAsync();
            }

            return new BitmapImage(new Uri(file.Path));
        }
#pragma warning restore 0612

#if NETCORE463
        public static async Task<List<string>> UploadImages(IEnumerable<UploadFileFragment> images)
        {
            List<string> responses = new List<string>(images.Count());
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                string json = JsonConvert.SerializeObject(images, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                using (StringContent uploadBucket = new StringContent("image"))
                using (StringContent uploadDir = new StringContent("feed"))
                using (StringContent is_anonymous = new StringContent("0"))
                using (StringContent uploadFileList = new StringContent(json))
                {
                    content.Add(uploadBucket, "uploadBucket");
                    content.Add(uploadDir, "uploadDir");
                    content.Add(is_anonymous, "is_anonymous");
                    content.Add(uploadFileList, "uploadFileList");
                    (bool isSucceed, JToken result) = await PostDataAsync(UriHelper.GetUri(UriType.OOSUploadPrepare), content);
                    if (isSucceed)
                    {
                        UploadPicturePrepareResult data = result.ToObject<UploadPicturePrepareResult>();
                        foreach (UploadFileInfo info in data.FileInfo)
                        {
                            UploadFileFragment image = images.FirstOrDefault(x => x.MD5 == info.MD5);
                            if (image == null) { continue; }
                            using (Stream stream = image.Bytes.GetStream())
                            {
                                string response = await Task.Run(() => OSSUploadHelper.OssUpload(data.UploadPrepareInfo, info, stream, "image/png"));
                                if (!string.IsNullOrEmpty(response))
                                {
                                    try
                                    {
                                        JObject token = JObject.Parse(response);
                                        if (token.TryGetValue("data", out JToken value)
                                            && ((JObject)value).TryGetValue("url", out JToken url)
                                            && !string.IsNullOrEmpty(url.ToString()))
                                        {
                                            responses.Add(url.ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                                        UIHelper.ShowMessage("上传失败");
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return responses;
        }
#else
        public static async Task<string[]> UploadImagesAsync(IEnumerable<UploadFileFragment> fragments, Extension extension)
        {
            ValueSet message = new ValueSet
            {
                ["UID"] = SettingsHelper.Get<string>(SettingsHelper.Uid),
                ["UserName"] = SettingsHelper.Get<string>(SettingsHelper.UserName),
                ["Token"] = SettingsHelper.Get<string>(SettingsHelper.Token),
                ["TokenVersion"] = (int)SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion),
                ["UserAgent"] = JsonConvert.SerializeObject(UserAgent.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString())),
                ["APIVersion"] = JsonConvert.SerializeObject(APIVersion.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString())),
                ["Images"] = JsonConvert.SerializeObject(fragments, new JsonSerializerSettings { ContractResolver = new IgnoreIgnoredContractResolver() })
            };
            return await extension.InvokeAsync(message) as string[];
        }

        public static async Task<(bool isSucceed, string result)> UploadImageAsync(byte[] image, string name)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (ByteArrayContent picFile = new ByteArrayContent(image))
                {
                    picFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"picFile\"",
                        FileName = $"\"{name}\""
                    };
                    picFile.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    picFile.Headers.ContentLength = image.Length;

                    content.Add(picFile);

                    (bool isSucceed, JToken result) = await PostDataAsync(UriHelper.GetV1Uri(UriType.UploadImage, "feed"), content).ConfigureAwait(false);

                    if (isSucceed) { return (isSucceed, result.ToString()); }
                }
            }
            return (false, null);
        }

        private class IgnoreIgnoredContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
                if (list != null)
                {
                    foreach (JsonProperty item in list)
                    {
                        if (item.Ignored)
                        {
                            item.Ignored = false;
                        }
                    }
                }
                return list;
            }
        }
#endif

        public static async Task<bool> CheckLoginAsync()
        {
            (bool isSucceed, _) = await GetDataAsync(UriHelper.GetUri(UriType.CheckLoginInfo), true).ConfigureAwait(false);
            return isSucceed;
        }
    }
}
