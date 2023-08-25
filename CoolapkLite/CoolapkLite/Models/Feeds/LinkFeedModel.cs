using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Feeds
{
    public enum LinkType
    {
        ITHome,
        Coolapk,
        BiliBili
    }

    public class LinkFeedModel : ISourceFeedModel, INotifyPropertyChanged
    {
        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool isSucceed = false;
        public bool IsSucceed
        {
            get => isSucceed;
            set
            {
                isSucceed = value;
                RaisePropertyChangedEvent();
            }
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                RaisePropertyChangedEvent();
            }
        }

        private string messageTitle;
        public string MessageTitle
        {
            get => messageTitle;
            set
            {
                messageTitle = value;
                RaisePropertyChangedEvent();
            }
        }

        private string messageRawOutput;
        public string MessageRawOutput
        {
            get => messageRawOutput;
            set
            {
                messageRawOutput = value;
                RaisePropertyChangedEvent();
            }
        }

        private string dateline;
        public string Dateline
        {
            get => dateline;
            set
            {
                dateline = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool showUser = true;
        public bool ShowUser
        {
            get => showUser;
            set
            {
                if (showUser != value)
                {
                    showUser = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private DateTime dateTime;
        public DateTime DateTime
        {
            get => dateTime;
            set
            {
                if (dateTime != value)
                {
                    dateTime = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private LinkUserModel userInfo;
        public LinkUserModel UserInfo
        {
            get => userInfo;
            set
            {
                userInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        private ImmutableArray<ImageModel> picArr = ImmutableArray<ImageModel>.Empty;
        public ImmutableArray<ImageModel> PicArr
        {
            get => picArr;
            set
            {
                picArr = value;
                RaisePropertyChangedEvent();
            }
        }

        ISourceUserModel ISourceFeedModel.UserInfo => UserInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public LinkFeedModel(string url)
        {
            if (!string.IsNullOrEmpty(url)) { ParseLink(url); }
        }

        private async void ParseLink(string url)
        {
            if (url.Contains("b23.tv") || url.Contains("t.cn"))
            {
                url = (await url.TryGetUri().ExpandShortUrlAsync()).ToString();
            }

            if (url.Contains("coolapk") && url.Contains("/feed/"))
            {
                GetJson(url.TryGetUri(), LinkType.Coolapk);
            }
            //else if (url.Contains("bilibili") && url.Contains("t.bilibili"))
            //{
            //    Match match = Regex.Match(url, @"/t.*?/(\w+)");
            //    if (match.Success && match.Groups.Count >= 2)
            //    {
            //        Uri uri = new Uri("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail");
            //        MultipartFormDataContent content = new MultipartFormDataContent { { new StringContent(match.Groups[1].Value), "dynamic_id" } };
            //        GetJson(uri, LinkType.BiliBili, content);
            //    }
            //}
            else if (url.Contains("ithome") && url.Contains("content"))
            {
                Match match = Regex.Match(url, @"(\?|%3F|&|%26)id(=|%3D)([\w]+)");
                if (match.Success && match.Groups.Count >= 4)
                {
                    Uri uri = new Uri($"https://qapi.ithome.com/api/content/getcontentdetail?id={match.Groups[3].Value}");
                    GetJson(uri, LinkType.ITHome);
                }
            }
        }

        private async void GetJson(Uri uri, LinkType type)
        {
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest"); 
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json, type);
            }
        }

        //private async void GetJson(Uri uri, LinkType type, MultipartFormDataContent content)
        //{
        //    (bool isSucceed, string result) = await RequestHelper.PostStringAsync(uri, content);
        //    if (isSucceed && !string.IsNullOrEmpty(result))
        //    {
        //        JObject json = JObject.Parse(result);
        //        ReadJson(json, type);
        //    }
        //}

        private void ReadJson(JObject json, LinkType type)
        {
            switch (type)
            {
                case LinkType.Coolapk:
                    ParseCoolapk(json);
                    IsSucceed = true;
                    break;
                case LinkType.BiliBili:
                    ParseBiliBili(json);
                    IsSucceed = true;
                    break;
                case LinkType.ITHome:
                    ParseITHome(json);
                    IsSucceed = true;
                    break;
                default:
                    break;
            }
        }

        private void ParseCoolapk(JObject json)
        {
            if (json.TryGetValue("data", out JToken v1))
            {
                JObject data = (JObject)v1;
                if (data.TryGetValue("userInfo", out JToken v2))
                {
                    JObject userInfo = (JObject)v2;
                    LinkUserModel UserModel = new LinkUserModel();
                    if (userInfo.TryGetValue("url", out JToken uurl))
                    {
                        UserModel.Url = uurl.ToString();
                    }
                    if (userInfo.TryGetValue("username", out JToken username))
                    {
                        UserModel.UserName = username.ToString();
                    }
                    UserInfo = UserModel;
                }
                if (data.TryGetValue("url", out JToken url))
                {
                    Url = url.ToString();
                }
                if (data.TryGetValue("feedType", out JToken feedType) && feedType.ToString() == "feedArticle")
                {
                    if (data.TryGetValue("message", out JToken message))
                    {
                        Message = message.ToString();
                        if (Message.Contains("</a>") ? Message.Length - 200 >= 7 : Message.Length - 120 >= 7)
                        {
                            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                            Message = message.ToString().Substring(0, 120);
                            Message = Message.Contains("</a>") ? message.ToString().Substring(0, 200) + "...<a href=\"" + Url + "\">" + loader.GetString("ReadMore") + "</a>" : Message + "...<a href=\"" + Url + "\">" + loader.GetString("ReadMore") + "</a>";
                        }
                    }
                }
                else
                {
                    if (data.TryGetValue("message", out JToken message))
                    {
                        Message = message.ToString();
                    }
                }
                if (data.TryGetValue("dateline", out JToken dateline))
                {
                    DateTime dateTime = dateline.ToObject<long>().ConvertUnixTimeStampToDateTime();
                    Dateline = dateTime.ConvertDateTimeToReadable();
                    DateTime = dateTime.ToLocalTime();
                }
                if (data.TryGetValue("message_title", out JToken message_title))
                {
                    MessageTitle = message_title.ToString();
                }
                if (data.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0)
                {
                    PicArr = picArr.Where(x => !string.IsNullOrEmpty(x?.ToString()))
                        .Select(x => new ImageModel(x.ToString(), ImageType.SmallImage))
                        .ToImmutableArray();

                    foreach (ImageModel item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
            }
        }

        private void ParseBiliBili(JObject json)
        {
            if (json.TryGetValue("data", out JToken v1))
            {
                JObject data = (JObject)v1;
                if (data.TryGetValue("card", out JToken v2))
                {
                    JObject card = (JObject)v2;
                    if (card.TryGetValue("card", out JToken v3))
                    {
                        JObject card1 = JObject.Parse(v3.ToString());
                        if (card1.TryGetValue("item", out JToken v4))
                        {
                            JObject item = (JObject)v4;
                            if (item.TryGetValue("description", out JToken description))
                            {
                                Message = description.ToString();
                            }
                            if (item.TryGetValue("title", out JToken title))
                            {
                                MessageTitle = title.ToString();
                            }
                            if (item.TryGetValue("upload_time", out JToken upload_time))
                            {
                                Dateline = upload_time.ToObject<long>().ConvertUnixTimeStampToReadable();
                            }
                            if (item.TryGetValue("pictures", out JToken pictures) && (pictures as JArray).Count > 0)
                            {
                                PicArr = pictures.Select(x => (x as JObject).TryGetValue("img_src", out JToken img_src) ? img_src.ToString() : null)
                                    .Where(x => !string.IsNullOrEmpty(x))
                                    .Select(x => new ImageModel(x, ImageType.OriginImage))
                                    .ToImmutableArray();
                                foreach (ImageModel items in PicArr)
                                {
                                    items.ContextArray = PicArr;
                                }
                            }
                        }
                        if (card1.TryGetValue("user", out JToken v5))
                        {
                            JObject user = (JObject)v5;
                            LinkUserModel UserModel = new LinkUserModel();
                            if (user.TryGetValue("name", out JToken name))
                            {
                                UserModel.UserName = name.ToString();
                            }
                            if (user.TryGetValue("uid", out JToken uid))
                            {
                                UserModel.Url = "https://space.bilibili.com/" + uid.ToString();
                            }
                            UserInfo = UserModel;
                        }
                    }
                }
                if (data.TryGetValue("desc", out JToken v6))
                {
                    JObject desc = (JObject)v6;
                    if (data.TryGetValue("dynamic_id_str", out JToken dynamic_id_str))
                    {
                        Url = "https://t.bilibili.com/" + dynamic_id_str;
                    }
                }
                if (Message.Length - 120 >= 7)
                {
                    Message = message.ToString().Substring(0, 120) + "...<a href=\"" + Url + "\">";
                }
            }
        }

        private void ParseITHome(JObject json)
        {
            if (json.TryGetValue("data", out JToken v1))
            {
                JObject data = (JObject)v1;
                if (data.TryGetValue("id", out JToken id))
                {
                    Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty)}";
                }
                if (data.TryGetValue("contents", out JToken contents))
                {
                    bool hasTag = false;
                    StringBuilder builder = new StringBuilder();
                    foreach (JObject v in contents)
                    {
                        if (v.TryGetValue("content", out JToken content) && v.TryGetValue("type", out JToken type))
                        {
                            switch (type.ToString())
                            {
                                case "0":
                                    _ = builder.Append(content);
                                    break;
                                case "2":
                                    _ = v.TryGetValue("link", out JToken link) && !string.IsNullOrEmpty(link.ToString())
                                        ? builder.AppendFormat("<a class=\"feed-link-url\" href=\"{0}\" target=\"_blank\" rel=\"nofollow\">{1}</a>", link.ToString(), string.IsNullOrEmpty(content.ToString()) ? "查看链接" : content)
                                        : builder.Append(content);
                                    hasTag = true;
                                    break;
                                case "3":
                                    _ = v.TryGetValue("topicId", out JToken topicId) && !string.IsNullOrEmpty(topicId.ToString())
                                        ? builder.AppendFormat("<a class=\"feed-link-tag\" href=\"ithome://qtopic?id={0}\">{1}</a>", topicId.ToString(), content.ToString())
                                        : builder.Append(content);
                                    hasTag = true;
                                    break;
                                default:
                                    _ = builder.Append(content);
                                    break;
                            }
                        }
                    }
                    MessageRawOutput = builder.ToString();
                    int length = hasTag ? 200 : 120;
                    if (builder.Length - length >= 7)
                    {
                        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                        builder.Remove(length, builder.Length - length);
                        builder.Append("...<a href=\"" + Url + "\">" + loader.GetString("ReadMore") + "</a>");
                    }
                    Message = builder.ToString();
                }
                if (data.TryGetValue("user", out JToken v2))
                {
                    JObject user = (JObject)v2;
                    LinkUserModel UserModel = new LinkUserModel();
                    if (user.TryGetValue("userNick", out JToken userNick))
                    {
                        UserModel.UserName = userNick.ToString();
                    }
                    if (user.TryGetValue("userAvatar", out JToken userAvatar))
                    {
                        UserModel.UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
                    }
                    if (user.TryGetValue("link", out JToken link))
                    {
                        UserModel.Url = link.ToString();
                    }
                    UserInfo = UserModel;
                }
                if (data.TryGetValue("pictures", out JToken pictures) && (pictures as JArray).Count > 0)
                {
                    PicArr = pictures.Select(x => (x as JObject).TryGetValue("src", out JToken src) ? src.ToString() : null)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => new ImageModel(x, ImageType.OriginImage))
                        .ToImmutableArray();
                    foreach (ImageModel item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
                if (data.TryGetValue("createTime", out JToken createTime))
                {
                    DateTime dateTime = Convert.ToDateTime(createTime.ToString());
                    Dateline = dateTime.ConvertDateTimeToReadable();
                    DateTime = dateTime.ToLocalTime();
                }
            }
        }
    }
}
