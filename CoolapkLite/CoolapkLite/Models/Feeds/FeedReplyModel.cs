using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CoolapkLite.Models.Feeds
{
    public class FeedReplyModel : SourceFeedReplyModel, INotifyPropertyChanged, ICanChangeLikeModel, ICanChangeReplyNum, ICanCopy
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

        public bool Liked { get; set; }
        public bool ShowReplyRows { get; set; }
        public ImageModel Pic { get; private set; }
        public string Dateline { get; private set; }
        public new string Message { get; private set; }
        public double ReplyRowsMore { get; private set; }
        public double ReplyRowsCount { get; private set; }
        public ImageModel UserAvatar { get; private set; }
        public bool ShowReplyRowsMore => ReplyRowsMore > 0;
        public List<SourceFeedReplyModel> ReplyRows { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedReplyModel(JObject token, bool ShowReplyRow = true) : base(token)
        {
            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = DataHelper.ConvertUnixTimeStampToReadable(Convert.ToDouble(dateline.ToString()));
            }

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("likenum", out JToken likenum))
            {
                LikeNum = Convert.ToInt32(likenum.ToString());
            }

            if (token.TryGetValue("replynum", out JToken replynum))
            {
                ReplyNum = Convert.ToInt32(replynum.ToString());
            }

            if (token.TryGetValue("replyRowsMore", out JToken replyRowsMore))
            {
                ReplyRowsMore = Convert.ToInt32(replyRowsMore.ToString());
            }

            if (token.TryGetValue("replyRowsCount", out JToken replyRowsCount))
            {
                ReplyRowsCount = Convert.ToInt32(replyRowsCount.ToString());
            }

            ShowReplyRows = ShowReplyRow && ReplyRowsCount > 0;

            if (ShowReplyRows)
            {
                ReplyRows = token["replyRows"].Select(item => new SourceFeedReplyModel((JObject)item)).ToList();
                ReplyRowsMore = token.Value<int>("replyRowsMore");
            }

            Liked = token.TryGetValue("userAction", out JToken userAction) ? userAction.Value<int>("like") == 1 : false;

            if (!string.IsNullOrEmpty(PicUri))
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }
    }
}
