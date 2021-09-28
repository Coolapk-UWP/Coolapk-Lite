using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedModelBase(JObject token) : base(token)
        {

        }
    }
}
