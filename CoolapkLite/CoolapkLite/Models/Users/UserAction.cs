using CoolapkLite.Core.Models;
using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Users
{
    public class UserAction : Entity, INotifyPropertyChanged
    {
        private bool like;
        public bool Like
        {
            get => like;
            set
            {
                if (like != value)
                {
                    like = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool favorite;
        public bool Favorite
        {
            get => favorite;
            set
            {
                if (favorite != value)
                {
                    favorite = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool follow;
        public bool Follow
        {
            get => follow;
            set
            {
                if (follow != value)
                {
                    follow = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool collect;
        public bool Collect
        {
            get => collect;
            set
            {
                if (collect != value)
                {
                    collect = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool followAuthor;
        public bool FollowAuthor
        {
            get => followAuthor;
            set
            {
                if (followAuthor != value)
                {
                    OnFollowChanged();
                    followAuthor = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool authorFollowYou;
        public bool AuthorFollowYou
        {
            get => authorFollowYou;
            set
            {
                if (authorFollowYou != value)
                {
                    OnFollowChanged();
                    authorFollowYou = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string followGlyph;
        public string FollowGlyph
        {
            get => followGlyph;
            set
            {
                if (followGlyph != value)
                {
                    followGlyph = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string followStatus;
        public string FollowStatus
        {
            get => followStatus;
            set
            {
                if (followStatus != value)
                {
                    followStatus = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public UserAction(JObject token) : base(token)
        {
            if (token.TryGetValue("like", out JToken like))
            {
                Like = like.ToObject<int>() != 0;
            }

            if (token.TryGetValue("favorite", out JToken favorite))
            {
                Favorite = favorite.ToObject<int>() != 0;
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                Follow = follow.ToObject<int>() != 0;
            }

            if (token.TryGetValue("collect", out JToken collect))
            {
                Collect = collect.ToObject<int>() != 0;
            }

            if (token.TryGetValue("followAuthor", out JToken followAuthor))
            {
                FollowAuthor = followAuthor.ToObject<int>() != 0;
            }

            if (token.TryGetValue("authorFollowYou", out JToken authorFollowYou))
            {
                AuthorFollowYou = authorFollowYou.ToObject<int>() != 0;
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowGlyph = FollowAuthor ? AuthorFollowYou ? loader.GetString("unfollowFanGlyph") : loader.GetString("unfollowGlyph")
                        : AuthorFollowYou ? loader.GetString("followFanGlyph") : loader.GetString("followGlyph");
            FollowStatus = FollowAuthor ? AuthorFollowYou ? loader.GetString("unfollowFan") : loader.GetString("unfollow")
                        : AuthorFollowYou ? loader.GetString("followFan") : loader.GetString("follow");
        }
    }
}
