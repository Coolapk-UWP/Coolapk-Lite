using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Users
{
    public class UserAction : Entity, INotifyPropertyChanged
    {
        private bool like;
        public bool Like
        {
            get => like;
            set => SetProperty(ref like, value);
        }

        private bool favorite;
        public bool Favorite
        {
            get => favorite;
            set => SetProperty(ref favorite, value);
        }

        private bool follow;
        public bool Follow
        {
            get => follow;
            set => SetProperty(ref follow, value);
        }

        private bool collect;
        public bool Collect
        {
            get => collect;
            set => SetProperty(ref collect, value);
        }

        private bool followAuthor;
        public bool FollowAuthor
        {
            get => followAuthor;
            set
            {
                if (followAuthor != value)
                {
                    followAuthor = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
        }

        private bool authorFollowYou;
        public bool AuthorFollowYou
        {
            get => authorFollowYou;
            set => SetProperty(ref authorFollowYou, value);
        }

        private string followGlyph;
        public string FollowGlyph
        {
            get => followGlyph;
            set => SetProperty(ref followGlyph, value);
        }

        private string followStatus;
        public string FollowStatus
        {
            get => followStatus;
            set => SetProperty(ref followStatus, value);
        }

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

        public UserAction(JObject token) : base(token)
        {
            if (token == null) { return; }

            if (token.TryGetValue("like", out JToken like))
            {
                Like = like.ToObject<bool>();
            }

            if (token.TryGetValue("favorite", out JToken favorite))
            {
                Favorite = favorite.ToObject<bool>();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                Follow = follow.ToObject<bool>();
            }

            if (token.TryGetValue("collect", out JToken collect))
            {
                Collect = collect.ToObject<bool>();
            }

            if (token.TryGetValue("followAuthor", out JToken followAuthor))
            {
                FollowAuthor = followAuthor.ToObject<bool>();
            }

            if (token.TryGetValue("authorFollowYou", out JToken authorFollowYou))
            {
                AuthorFollowYou = authorFollowYou.ToObject<bool>();
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowGlyph = FollowAuthor ? AuthorFollowYou ? "\uE8EE" : "\uE8FB"
                        : AuthorFollowYou ? "\uE97A" : "\uE710";
            FollowStatus = FollowAuthor ? AuthorFollowYou ? loader.GetString("UnfollowFan") : loader.GetString("Unfollow")
                        : AuthorFollowYou ? loader.GetString("FollowFan") : loader.GetString("Follow");
        }
    }
}
