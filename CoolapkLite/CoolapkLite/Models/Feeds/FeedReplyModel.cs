﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models.Feeds
{
    public class FeedReplyModel : SourceFeedReplyModel, INotifyPropertyChanged, ICanLike, ICanReply, ICanCopy
    {
        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set
            {
                if (likeNum != value)
                {
                    likeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int replyNum;
        public int ReplyNum
        {
            get => replyNum;
            set
            {
                if (replyNum != value)
                {
                    replyNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
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

        public int ReplyRowsMore { get; private set; }
        public int ReplyRowsCount { get; private set; }

        public string Dateline { get; private set; }

        public DateTime DateTime { get; private set; }
        public ImageModel Pic { get; private set; }

        public ImmutableArray<SourceFeedReplyModel> ReplyRows { get; private set; } = ImmutableArray<SourceFeedReplyModel>.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedReplyModel(JObject token, bool ShowReplyRow = true) : base(token)
        {
            if (token.TryGetValue("dateline", out JToken dateline))
            {
                DateTimeOffset dateTimeOffset = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
                Dateline = dateTimeOffset.ConvertDateTimeOffsetToReadable();
                DateTime = dateTimeOffset.LocalDateTime;
            }

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("likenum", out JToken likenum))
            {
                LikeNum = likenum.ToObject<int>();
            }

            if (token.TryGetValue("replynum", out JToken replynum))
            {
                ReplyNum = replynum.ToObject<int>();
            }

            if (token.TryGetValue("replyRowsMore", out JToken replyRowsMore))
            {
                ReplyRowsMore = replyRowsMore.ToObject<int>();
            }

            if (token.TryGetValue("replyRowsCount", out JToken replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount.ToObject<int>();
            }

            if (token.TryGetValue("replyRows", out JToken replyRows))
            {
                ReplyRows = replyRows.OfType<JObject>().Select(item => new SourceFeedReplyModel(item)).ToImmutableArray();
            }

            if (!string.IsNullOrEmpty(PicUri))
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }

        public async Task ChangeLikeAsync()
        {
            UriType type = Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetV1Uri(type, "Reply", ID), null, true);
            if (!isSucceed) { return; }
            Liked = !Liked;
            if (result.ToObject<int>() is int likenum && likenum >= 0)
            {
                LikeNum = likenum;
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}的回复", UserInfo?.UserName)
                                                                .TryAppendLine(Message.HtmlToString())
                                                                .AppendFormat("{0}点赞 {1}回复", LikeNum, ReplyNum)
                                                                .ToString();
    }
}
