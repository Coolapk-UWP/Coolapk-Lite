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
        Coolapk
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

        private DateTimeOffset dateline;
        public DateTimeOffset Dateline
        {
            get => dateline;
            set
            {
                dateline = value;
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
            if (url.ContainsAny(new[] { "b23.tv", "t.cn" }, StringComparison.OrdinalIgnoreCase))
            {
                url = await url.TryGetUri().ExpandShortUrlAsync().ContinueWith(x => x.Result.ToString());
            }

            if (url.ContainsAll(new[] { "coolapk", "/feed/" }, StringComparison.OrdinalIgnoreCase))
            {
                GetJson(url.TryGetUri(), LinkType.Coolapk);
            }
            else if (url.ContainsAll(new[] { "ithome", "content" }, StringComparison.OrdinalIgnoreCase))
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
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, NetworkHelper.XMLHttpRequest);
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json, type);
            }
        }

        private void ReadJson(JObject json, LinkType type)
        {
            switch (type)
            {
                case LinkType.Coolapk:
                    ParseCoolapk(json);
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
                if (data.TryGetValue("message", out JToken message))
                {
                    MessageRawOutput = message.ToString();
                    int length = MessageRawOutput.Contains("</a>", StringComparison.OrdinalIgnoreCase) ? 200 : 120;
                    if (MessageRawOutput.Length - length >= 7)
                    {
                        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                        Message = $"{MessageRawOutput.Substring(0, length)}...<a href=\"{Url}\">{loader.GetString("ReadMore")}</a>";
                    }
                }
                if (data.TryGetValue("dateline", out JToken dateline))
                {
                    Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
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
                    foreach (JObject v in contents.OfType<JObject>())
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
                        builder.AppendFormat("...<a href=\"{0}\">{1}</a>", Url, loader.GetString("ReadMore"));
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
                if (data.TryGetValue("createTime", out JToken createTime) && DateTimeOffset.TryParse(createTime.ToString(), out DateTimeOffset dateTimeOffset))
                {
                    Dateline = dateTimeOffset;
                }
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}的动态", UserInfo.UserName)
                                                                .Append(Message.HtmlToString())
                                                                .ToString();
    }
}
