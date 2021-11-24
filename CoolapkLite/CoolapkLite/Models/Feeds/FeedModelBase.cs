using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkLite.Models.Feeds
{
    public class FeedModelBase : SourceFeedModel, INotifyPropertyChanged, ICanChangeLikeModel, ICanChangeReplyNum, ICanCopy
    {
        private string likeNum;
        public string LikeNum
        {
            get => likeNum;
            set
            {
                likeNum = value;
                RaisePropertyChangedEvent();
            }
        }

        private string replyNum;
        public string ReplyNum
        {
            get => replyNum;
            set
            {
                replyNum = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ID => EntityId;
        public bool Liked { get; set; }
        public string Info { get; private set; }
        public string DeviceTitle { get; private set; }
        public bool ShowUser { get; private set; } = true;
        public ImageModel UserAvatar { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken info) && !string.IsNullOrEmpty(info.ToString()))
            {
                Info = info.ToString();
            }
            else if (token.TryGetValue("feedTypeName", out JToken feedTypeName))
            {
                Info = feedTypeName.ToString();
            }

            if (token.TryGetValue("device_title", out JToken device_title))
            {
                DeviceTitle = device_title.ToString();
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar) && !string.IsNullOrEmpty(userAvatar.ToString()))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.SmallAvatar);
            }
        }
    }
}
