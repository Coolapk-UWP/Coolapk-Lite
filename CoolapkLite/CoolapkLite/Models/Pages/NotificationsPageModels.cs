﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Pages
{
    public abstract class NotificationModel : Entity, IHasUrl
    {
        public int ID { get; protected set; }

        public bool IsNew { get; protected set; }

        public string Url { get; protected set; }
        public string Status { get; protected set; }
        public string UserUrl { get; protected set; }
        public string UserName { get; protected set; }
        public string BlockStatus { get; protected set; }

        public ImageModel UserAvatar { get; protected set; }
        public DateTimeOffset Dateline { get; protected set; }

        protected NotificationModel(JObject token) : base(token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("isnew", out JToken isnew))
            {
                IsNew = isnew.ToObject<bool>();
            }
        }

        public override string ToString() => string.Join(" - ", UserName, Dateline);
    }

    public class SimpleNotificationModel : NotificationModel
    {
        public string Note { get; private set; }

        public SimpleNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("url", out JToken url))
            {
                UserUrl = url.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("note", out JToken _note))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_note.ToString());
                HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
                HtmlNode node = nodes.Last();
                Note = doc.DocumentNode.InnerText;
                Url = node.GetAttributeValue("href", string.Empty);
            }

            if (token.TryGetValue("fromUserAvatar", out JToken fromUserAvatar))
            {
                UserAvatar = new ImageModel(fromUserAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("fromUserInfo", out JToken v1))
            {
                JObject fromUserInfo = (JObject)v1;
                BlockStatus = fromUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : fromUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : fromUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("fromusername", out JToken fromusername))
            {
                UserName = string.Join(" ", new[] { fromusername.ToString(), BlockStatus }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            string[] strings = new string[2];

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                strings[0] = "[已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                strings[1] += " [仅自己可见]";
            }

            Status = string.Join(" ", strings.Where(string.IsNullOrWhiteSpace));
        }

        public override string ToString() => new StringBuilder().TryAppendLine(UserName)
                                                                .AppendLine(Dateline.ConvertDateTimeOffsetToReadable())
                                                                .Append(Note.HtmlToString())
                                                                .ToString();
    }

    public class AtCommentMeNotificationModel : NotificationModel
    {
        public string FeedUrl { get; private set; }
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }

        public AtCommentMeNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("extra_url", out JToken extra_url))
            {
                FeedUrl = extra_url.ToString();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UserUrl = $"/u/{uid}";
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("extra_title", out JToken extra_title))
            {
                FeedMessage = extra_title.ToString();
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            Message = $"{(string.IsNullOrEmpty(token.Value<string>("rusername")) ? string.Empty : $"回复<a href=\"/u/{token.Value<string>("ruid")}\">{token.Value<string>("rusername")}</a>: ")}{token.Value<string>("message")}";

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                BlockStatus = userInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : userInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : userInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = string.Join(" ", new[] { username.ToString(), BlockStatus }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            string[] strings = new string[2];

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                strings[0] = "[已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                strings[1] = "[仅自己可见]";
            }

            Status = string.Join(" ", strings.Where(string.IsNullOrWhiteSpace));
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}提及", UserName)
                                                                .AppendLine(Dateline.ConvertDateTimeOffsetToReadable())
                                                                .Append(Message.HtmlToString())
                                                                .ToString();
    }

    public class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }

        public SourceFeedModel FeedDetail { get; private set; }

        public LikeNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("feedTypeName", out JToken feedTypeName))
            {
                Title = $"赞了你的{feedTypeName}";
            }
            else if (token.TryGetValue("infoHtml", out JToken infoHtml))
            {
                Title = $"赞了你的{infoHtml}";
            }

            if (token.TryGetValue("likeUid", out JToken likeUid))
            {
                UserUrl = $"/u/{likeUid}";
            }

            if (token.TryGetValue("likeTime", out JToken likeTime))
            {
                Dateline = likeTime.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("likeAvatar", out JToken likeAvatar))
            {
                UserAvatar = new ImageModel(likeAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("likeUserInfo", out JToken v1))
            {
                JObject likeUserInfo = (JObject)v1;
                BlockStatus = likeUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : likeUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : likeUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("likeUsername", out JToken likeUsername))
            {
                UserName = string.Join(" ", new[] { likeUsername.ToString(), BlockStatus }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            string[] strings = new string[2];

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                strings[0] = " [已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                strings[1] = " [仅自己可见]";
            }

            Status = string.Join(" ", strings.Where(string.IsNullOrWhiteSpace));

            FeedDetail = new SourceFeedModel(token);
        }

        public override string ToString() => new StringBuilder().Append(UserName)
                                                                .AppendLine(Title)
                                                                .TryAppendLine(Dateline.ConvertDateTimeOffsetToReadable())
                                                                .AppendLine("点赞动态：")
                                                                .Append(FeedDetail)
                                                                .ToString();
    }

    public class MessageNotificationModel : NotificationModel
    {
        public int UnreadNum { get; private set; }
        public bool IsTop { get; private set; }
        public string UKey { get; private set; }
        public string FeedMessage { get; private set; }

        public MessageNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("unreadNum", out JToken unreadNum))
            {
                UnreadNum = unreadNum.ToObject<int>();
            }

            if (token.TryGetValue("ukey", out JToken ukey))
            {
                UKey = ukey.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("message", out JToken message))
            {
                FeedMessage = message.ToString();
            }

            if (token.TryGetValue("messageUserInfo", out JToken v1))
            {
                JObject messageUserInfo = (JObject)v1;

                if (messageUserInfo.TryGetValue("uid", out JToken uid))
                {
                    UserUrl = $"/u/{uid}";
                }

                if (messageUserInfo.TryGetValue("userAvatar", out JToken userAvatar))
                {
                    UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
                }

                BlockStatus = messageUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : messageUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : messageUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;

                if (messageUserInfo.TryGetValue("username", out JToken username))
                {
                    UserName = string.Join(" ", new[] { username.ToString(), BlockStatus }.Where(x => !string.IsNullOrWhiteSpace(x)));
                }
            }

            if (token.TryGetValue("is_top", out JToken is_top))
            {
                IsTop = is_top.ToObject<bool>();
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}私信", UserName)
                                                                .TryAppendLine(Dateline.ConvertDateTimeOffsetToReadable())
                                                                .Append(FeedMessage.HtmlToString())
                                                                .ToString();
    }
}
