﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Feeds
{
    public class SourceFeedModel : Entity, ISourceFeedModel, INotifyPropertyChanged
    {
        private bool showUser = true;
        public bool ShowUser
        {
            get => showUser;
            set => SetProperty(ref showUser, value);
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set => SetProperty(ref isCopyEnabled, value);
        }

        public int ID { get; private set; }
        public int RatingStar { get; private set; }

        public bool IsVoteFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsCoolPicture { get; private set; }
        public bool IsQuestionFeed { get; private set; }

        public string Url { get; private set; }
        public string Message { get; private set; }
        public string ShareUrl { get; private set; }
        public string MessageTitle { get; private set; }
        public string FeedType { get; private set; } = "feed";

        public ImageModel Pic { get; private set; }
        public UserModel UserInfo { get; private set; }
        public UserAction UserAction { get; private set; }
        public DateTimeOffset Dateline { get; private set; }

        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

        ISourceUserModel ISourceFeedModel.UserInfo => UserInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public SourceFeedModel(JObject token) : base(token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else
            {
                Url = $"/feed/{ID}";
            }

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                UserInfo = new UserModel(userInfo);
            }
            else
            {
                UserInfo = new UserModel(null);
            }

            if (token.TryGetValue("userAction", out JToken v2))
            {
                JObject userAction = (JObject)v2;
                UserAction = new UserAction(userAction);
            }
            else
            {
                UserAction = new UserAction(null);
            }

            ShareUrl = token.TryGetValue("shareUrl", out JToken shareUrl) && !string.IsNullOrEmpty(shareUrl.ToString())
                ? shareUrl.ToString()
                : $"https://www.coolapk.com{(Url != null ? Url.Replace("/question/", "/feed/") : string.Empty)}";

            if (token.TryGetValue("message", out JToken message))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                Message = message.ToString().Replace("<a href=\"\">查看更多</a>", $"<a href=\"{Url}\">{loader.GetString("ReadMore")}</a>");
            }

            if (token.TryGetValue("message_title", out JToken message_title))
            {
                MessageTitle = message_title.ToString();
            }

            if (token.TryGetValue("feedType", out JToken feedType))
            {
                FeedType = feedType.ToString();
                switch (FeedType)
                {
                    case "vote":
                        IsVoteFeed = true;
                        Url = Url.Replace("/feed/", "/vote/");
                        break;
                    case "rating":
                        IsRatingFeed = true;
                        if (token.TryGetValue("star", out JToken star))
                        {
                            RatingStar = star.ToObject<int>();
                        }
                        break;
                    case "question":
                        IsQuestionFeed = true;
                        Url = Url.Replace("/feed/", "/question/");
                        break;
                }
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0)
            {
                PicArr = picArr.Where(x => !string.IsNullOrEmpty(x?.ToString()))
                               .Select(x => new ImageModel(x.ToString(), ImageType.SmallImage))
                               .ToImmutableArray();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.SmallImage);
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}的动态", UserInfo.UserName)
                                                                .Append(Message.HtmlToString())
                                                                .ToString();
    }
}
