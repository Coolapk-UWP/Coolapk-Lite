using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkLite.Models.Pages
{
    public abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(JObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase
    {
        public ImageModel UserAvatar { get; private set; }

        public string Bio { get; private set; }
        public string UserName { get; private set; }

        internal UserDetail(JObject token) : base(token)
        {
            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }
        }
    }
}
