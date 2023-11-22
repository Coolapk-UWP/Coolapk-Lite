﻿using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Pages
{
    public abstract class FeedListDetailBase : Entity, ICanCopy, INotifyPropertyChanged
    {
        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set => SetProperty(ref isCopyEnabled, value);
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

        protected FeedListDetailBase(JObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

    public class UserDetail : FeedListDetailBase, IUserModel, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
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

        int ICanFollow.ID => UID;

        public int UID { get; private set; }
        public int FeedNum { get; private set; }
        public int LikeNum { get; private set; }
        public int FansNum { get; private set; }
        public int LevelNum { get; private set; }
        public int FollowNum { get; private set; }

        public bool IsMe { get; private set; }
        public bool IsFans { get; private set; }
        public bool IsBlackList { get; private set; }

        public string Bio { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Gender { get; private set; }
        public string UserName { get; private set; }
        public string LoginText { get; private set; }
        public string BlockStatus { get; private set; }
        public string VerifyTitle { get; private set; }

        public long Experience { get; private set; }
        public long NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public string Url => $"/u/{UID}";

        public UserDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
                IsMe = uid.ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid);
            }

            if (token.TryGetValue("feed", out JToken feed))
            {
                FeedNum = feed.ToObject<int>();
            }

            if (token.TryGetValue("be_like_num", out JToken be_like_num))
            {
                LikeNum = be_like_num.ToObject<int>();
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                FansNum = fans.ToObject<int>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                LevelNum = level.ToObject<int>();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<int>();
            }

            if (token.TryGetValue("isFans", out JToken isFans))
            {
                IsFans = isFans.ToObject<bool>();
            }

            if (token.TryGetValue("isBlackList", out JToken isBlackList))
            {
                IsBlackList = isBlackList.ToObject<bool>();
            }

            if (token.TryGetValue("isFollow", out JToken isFollow))
            {
                Followed = isFollow.ToObject<bool>();
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("province", out JToken province) && token.TryGetValue("city", out JToken city))
            {
                City = province.ToString() == city.ToString() ? city.ToString() : $"{province} {city}";
            }

            if (token.TryGetValue("astro", out JToken astro))
            {
                Astro = astro.ToString();
            }

            if (token.TryGetValue("gender", out JToken gender))
            {
                Gender = gender.ToObject<int>() == 1 ? "♂"
                    : gender.ToObject<int>() == 0 ? "♀"
                    : string.Empty;
            }

            if (token.TryGetValue("displayUsername", out JToken displayUsername))
            {
                UserName = displayUsername.ToString();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginText = $"{logintime.ToObject<long>().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>() == -1 ? loader.GetString("BlockStatus-1")
                    : block_status.ToObject<int>() == 2 ? loader.GetString("BlockStatus2") : "\0\0";
                BlockStatus = BlockStatus.Substring(1, BlockStatus.Length - 2);
            }

            if (token.TryGetValue("verify_title", out JToken verify_title))
            {
                VerifyTitle = verify_title.ToString();
            }

            if (token.TryGetValue("experience", out JToken experience))
            {
                Experience = experience.ToObject<long>();
            }

            if (token.TryGetValue("next_level_experience", out JToken next_level_experience))
            {
                NextLevelExperience = next_level_experience.ToObject<long>();
            }

            if (token.TryGetValue("next_level_percentage", out JToken next_level_percentage))
            {
                NextLevelPercentage = next_level_percentage.ToObject<double>();
            }

            if (token.TryGetValue("cover", out JToken cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            if (!IsMe)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowStatus = IsBlackList ? loader.GetString("InBlackList")
                    : Followed ? IsFans ? loader.GetString("UnfollowFan") : loader.GetString("UnFollow")
                    : IsFans ? loader.GetString("FollowFan") : loader.GetString("Follow");
                FollowGlyph = IsBlackList ? "\uE8F8"
                    : Followed ? IsFans ? "\uE8EE" : "\uE8FB"
                    : IsFans ? "\uE97A" : "\uE710";
            }
        }

        public async Task ChangeFollowAsync()
        {
            UriType type = Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, UID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("用户：{0}", UserName)
                                                                .TryAppendLine(LoginText)
                                                                .AppendLineFormat("{0}点赞 {1}关注 {2}粉丝", LikeNum, FollowNum, FansNum)
                                                                .TryAppendLine(Bio)
                                                                .TryAppendJoin(" ", Gender, City, Astro, BlockStatus)
                                                                .ToString();
    }

    public class TopicDetail : FeedListDetailBase, IHasSubtitle, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
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

        public int ID { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string SubTitle { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImmutableArray<UserModel> FollowUsers { get; private set; } = ImmutableArray<UserModel>.Empty;

        public ImageModel Pic => Logo;

        public TopicDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
            {
                Followed = follow.ToObject<bool>();
            }

            if (token.TryGetValue("hot_num_txt", out JToken hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetValue("follownum_txt", out JToken follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetValue("commentnum_txt", out JToken commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("intro", out JToken intro) && Description != intro.ToString())
            {
                SubTitle = intro.ToString();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("recent_follow_list", out JToken recent_follow_list) && (recent_follow_list as JArray).Count > 0)
            {
                FollowUsers = recent_follow_list
                    .OfType<JObject>()
                    .Select(x => x.TryGetValue("userInfo", out JToken userInfo)
                        ? new UserModel((JObject)userInfo) : null)
                    .OfType<UserModel>()
                    .ToImmutableArray();
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("UnFollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollowAsync()
        {
            UriType type = Followed ? UriType.PostTopicUnfollow : UriType.PostTopicFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, Title), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("话题：{0}", Title)
                                                                .TryAppendLineJoin(" ", HotNum, CommentNum, FollowNum)
                                                                .TryAppendLine(Description)
                                                                .Append(SubTitle)
                                                                .ToString();
    }

    public class DyhDetail : FeedListDetailBase, IHasDescription, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
        }

        private string followNum;
        public string FollowNum
        {
            get => followNum;
            set => SetProperty(ref followNum, value);
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

        public int ID { get; private set; }

        public string Uurl { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ImageModel Pic => Logo;

        public string Url => $"/dyh/{ID}";

        public DyhDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
            {
                Followed = follow.ToObject<bool>();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("author", out JToken author))
            {
                UserName = author.ToString();
            }

            if (token.TryGetValue("follownum", out JToken follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollowAsync()
        {
            UriType type = Followed ? UriType.PostDyhUnfollow : UriType.PostDyhFollow;

            (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, ID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
            if (result.ToObject<int>() is int follownum && follownum >= 0)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat($"看看号：{0}", Title)
                                                                .AppendLineFormat("作者：{0} {1}", UserName, FollowNum)
                                                                .Append(Description)
                                                                .ToString();
    }

    public class ProductDetail : FeedListDetailBase, IHasDescription, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
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

        public int ID { get; private set; }
        public int Star1Count { get; private set; }
        public int Star2Count { get; private set; }
        public int Star3Count { get; private set; }
        public int Star4Count { get; private set; }
        public int Star5Count { get; private set; }
        public int OwnerStar1Count { get; private set; }
        public int OwnerStar2Count { get; private set; }
        public int OwnerStar3Count { get; private set; }
        public int OwnerStar4Count { get; private set; }
        public int OwnerStar5Count { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string StarCount { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string RatingCount { get; private set; }
        public string Description { get; private set; }

        public double OwnerScore { get; private set; }
        public double RatingScore { get; private set; }
        public double Star1Percent { get; private set; }
        public double Star2Percent { get; private set; }
        public double Star3Percent { get; private set; }
        public double Star4Percent { get; private set; }
        public double Star5Percent { get; private set; }
        public double OwnerStar1Percent { get; private set; }
        public double OwnerStar2Percent { get; private set; }
        public double OwnerStar3Percent { get; private set; }
        public double OwnerStar4Percent { get; private set; }
        public double OwnerStar5Percent { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImmutableArray<string> TagArr { get; private set; } = ImmutableArray<string>.Empty;

        public ImmutableArray<UserModel> FollowUsers { get; private set; } = ImmutableArray<UserModel>.Empty;

        public ImmutableArray<ImageModel> CoverArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

        public ImageModel Pic => Logo;

        public ProductDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            double MaxStarCount = 0, MaxOwnerStarCount = 0;

            if (token.TryGetValue("star_1_count", out JToken star_1_count))
            {
                Star1Count = star_1_count.ToObject<int>();
                MaxStarCount = Math.Max(Star1Count, MaxStarCount);
            }

            if (token.TryGetValue("star_2_count", out JToken star_2_count))
            {
                Star2Count = star_2_count.ToObject<int>();
                MaxStarCount = Math.Max(Star2Count, MaxStarCount);
            }

            if (token.TryGetValue("star_3_count", out JToken star_3_count))
            {
                Star3Count = star_3_count.ToObject<int>();
                MaxStarCount = Math.Max(Star3Count, MaxStarCount);
            }

            if (token.TryGetValue("star_4_count", out JToken star_4_count))
            {
                Star4Count = star_4_count.ToObject<int>();
                MaxStarCount = Math.Max(Star4Count, MaxStarCount);
            }

            if (token.TryGetValue("star_5_count", out JToken star_5_count))
            {
                Star5Count = star_5_count.ToObject<int>();
                MaxStarCount = Math.Max(Star5Count, MaxStarCount);
            }

            if (token.TryGetValue("owner_star_1_count", out JToken owner_star_1_count))
            {
                OwnerStar1Count = owner_star_1_count.ToObject<int>();
                MaxOwnerStarCount = Math.Max(OwnerStar1Count, MaxOwnerStarCount);
            }

            if (token.TryGetValue("owner_star_2_count", out JToken owner_star_2_count))
            {
                OwnerStar2Count = owner_star_2_count.ToObject<int>();
                MaxOwnerStarCount = Math.Max(OwnerStar2Count, MaxOwnerStarCount);
            }

            if (token.TryGetValue("owner_star_3_count", out JToken owner_star_3_count))
            {
                OwnerStar3Count = owner_star_3_count.ToObject<int>();
                MaxOwnerStarCount = Math.Max(OwnerStar3Count, MaxOwnerStarCount);
            }

            if (token.TryGetValue("owner_star_4_count", out JToken owner_star_4_count))
            {
                OwnerStar4Count = owner_star_4_count.ToObject<int>();
                MaxOwnerStarCount = Math.Max(OwnerStar4Count, MaxOwnerStarCount);
            }

            if (token.TryGetValue("owner_star_5_count", out JToken owner_star_5_count))
            {
                OwnerStar5Count = owner_star_5_count.ToObject<int>();
                MaxOwnerStarCount = Math.Max(OwnerStar5Count, MaxOwnerStarCount);
            }

            MaxStarCount = Math.Max(MaxStarCount, double.Epsilon);
            MaxOwnerStarCount = Math.Max(MaxOwnerStarCount, double.Epsilon);

            Star1Percent = Star1Count * 100 / MaxStarCount;
            Star2Percent = Star2Count * 100 / MaxStarCount;
            Star3Percent = Star3Count * 100 / MaxStarCount;
            Star4Percent = Star4Count * 100 / MaxStarCount;
            Star5Percent = Star5Count * 100 / MaxStarCount;

            OwnerStar1Percent = OwnerStar1Count * 100 / MaxOwnerStarCount;
            OwnerStar2Percent = OwnerStar2Count * 100 / MaxOwnerStarCount;
            OwnerStar3Percent = OwnerStar3Count * 100 / MaxOwnerStarCount;
            OwnerStar4Percent = OwnerStar4Count * 100 / MaxOwnerStarCount;
            OwnerStar5Percent = OwnerStar5Count * 100 / MaxOwnerStarCount;

            if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
            {
                Followed = follow.ToObject<bool>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("hot_num_txt", out JToken hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetValue("star_total_count", out JToken star_total_count))
            {
                StarCount = $"{star_total_count}位酷友打分";
            }

            if (token.TryGetValue("follow_num_txt", out JToken follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetValue("feed_comment_num_txt", out JToken commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetValue("rating_total_num", out JToken rating_total_num))
            {
                RatingCount = $"{rating_total_num}位机主打分";
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("owner_star_average_score", out JToken owner_star_average_score))
            {
                OwnerScore = owner_star_average_score.ToObject<double>();
            }

            if (token.TryGetValue("rating_average_score", out JToken rating_average_score))
            {
                RatingScore = rating_average_score.ToObject<double>();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("tagArr", out JToken tagArr) && (tagArr as JArray).Count > 0)
            {
                TagArr = tagArr.Select(x => x.ToString()).ToImmutableArray();
            }

            if (token.TryGetValue("recent_follow_list", out JToken recent_follow_list) && (recent_follow_list as JArray).Count > 0)
            {
                FollowUsers = recent_follow_list
                    .OfType<JObject>()
                    .Select(x => x.TryGetValue("userInfo", out JToken userInfo)
                        ? new UserModel((JObject)userInfo) : null)
                    .OfType<UserModel>()
                    .ToImmutableArray();
            }

            if (token.TryGetValue("coverArr", out JToken coverArr) && (coverArr as JArray).Count > 0)
            {
                CoverArr = coverArr.Where(x => !string.IsNullOrEmpty(x?.ToString()))
                                   .Select(x => new ImageModel(x.ToString(), ImageType.SmallImage))
                                   .ToImmutableArray();

                foreach (ImageModel item in CoverArr)
                {
                    item.ContextArray = CoverArr;
                }
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("UnFollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollowAsync()
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent id = new StringContent(ID.ToString()))
                using (StringContent status = new StringContent(Followed ? "0" : "1"))
                {
                    content.Add(id, "id");
                    content.Add(status, "status");
                    (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.OperateProductFollow), content, true);
                    if (!isSucceed) { return; }
                    Followed = !Followed;
                }
            }
        }

        public override string ToString() => new StringBuilder().AppendFormat("数码：{0}", Title)
                                                                .TryAppendLineJoin(" ", HotNum, CommentNum, FollowNum)
                                                                .Append(Description)
                                                                .ToString();
    }

    public class CollectionDetail : FeedListDetailBase, IHasDescription, ICanLike, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
        }

        private string followNum;
        public string FollowNum
        {
            get => followNum;
            set => SetProperty(ref followNum, value);
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

        private bool liked;
        public bool Liked
        {
            get => liked;
            set => SetProperty(ref liked, value);
        }


        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set => SetProperty(ref likeNum, value);
        }

        public int ID { get; private set; }
        public int ItemNum { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string LastUpdate { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ImageModel Pic => Cover;

        public CollectionDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("userAction", out JToken userAction))
            {
                if (((JObject)userAction).TryGetValue("follow", out JToken follow))
                {
                    Followed = follow.ToObject<bool>();
                }

                if (((JObject)userAction).TryGetValue("like", out JToken like))
                {
                    Liked = like.ToObject<bool>();
                }
            }

            if (token.TryGetValue("item_num", out JToken item_num))
            {
                ItemNum = item_num.ToObject<int>();
            }

            if (token.TryGetValue("like_num", out JToken like_num))
            {
                LikeNum = like_num.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("follow_num", out JToken follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetValue("lastupdate", out JToken lastupdate))
            {
                LastUpdate = $"{lastupdate.ToObject<long>().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("cover_pic", out JToken cover_pic))
            {
                Cover = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeLikeAsync()
        {
            UriType type = Liked ? UriType.PostCollectionUnlike : UriType.PostCollectionLike;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Liked = !Liked;
                if (result.ToObject<int>() is int follownum && follownum >= 0)
                {
                    LikeNum = follownum;
                }
            }
        }

        public async Task ChangeFollowAsync()
        {
            UriType type = Followed ? UriType.PostCollectionUnfollow : UriType.PostCollectionFollow;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Followed = !Followed;
                if (result.ToObject<int>() is int follownum && follownum >= 0)
                {
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                    FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
                }
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("收藏单：{0}", Title)
                                                                .AppendLineFormat("{0}点赞 {1}关注", LikeNum, FollowNum)
                                                                .Append(Description)
                                                                .ToString();
    }

    public class AppDetail : FeedListDetailBase, IHasSubtitle, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
        }

        private string followNum;
        public string FollowNum
        {
            get => followNum;
            set => SetProperty(ref followNum, value);
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

        public int ID { get; private set; }
        public int Star1Count { get; private set; }
        public int Star2Count { get; private set; }
        public int Star3Count { get; private set; }
        public int Star4Count { get; private set; }
        public int Star5Count { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string APKName { get; private set; }
        public string APKSize { get; private set; }
        public string VoteNum { get; private set; }
        public string SubTitle { get; private set; }
        public string Introduce { get; private set; }
        public string CommentNum { get; private set; }
        public string VersionCode { get; private set; }
        public string VersionName { get; private set; }
        public string DownloadNum { get; private set; }
        public string Description { get; private set; }

        public double RatingScore { get; private set; }
        public double Star1Percent { get; private set; }
        public double Star2Percent { get; private set; }
        public double Star3Percent { get; private set; }
        public double Star4Percent { get; private set; }
        public double Star5Percent { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImmutableArray<string> TagArr { get; private set; } = ImmutableArray<string>.Empty;

        public ImmutableArray<UserModel> FollowUsers { get; private set; } = ImmutableArray<UserModel>.Empty;

        public ImmutableArray<ImageModel> ScreenshotArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

        public ImageModel Pic => Logo;

        public AppDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            double MaxStarCount = 0;

            if (token.TryGetValue("votenum1", out JToken votenum1))
            {
                Star1Count = votenum1.ToObject<int>();
                MaxStarCount = Math.Max(Star1Count, MaxStarCount);
            }

            if (token.TryGetValue("votenum2", out JToken votenum2))
            {
                Star2Count = votenum2.ToObject<int>();
                MaxStarCount = Math.Max(Star2Count, MaxStarCount);
            }

            if (token.TryGetValue("votenum3", out JToken votenum3))
            {
                Star3Count = votenum3.ToObject<int>();
                MaxStarCount = Math.Max(Star3Count, MaxStarCount);
            }

            if (token.TryGetValue("votenum4", out JToken votenum4))
            {
                Star4Count = votenum4.ToObject<int>();
                MaxStarCount = Math.Max(Star4Count, MaxStarCount);
            }

            if (token.TryGetValue("votenum5", out JToken votenum5))
            {
                Star5Count = votenum5.ToObject<int>();
                MaxStarCount = Math.Max(Star5Count, MaxStarCount);
            }

            MaxStarCount = Math.Max(MaxStarCount, double.Epsilon);

            Star1Percent = Star1Count * 100 / MaxStarCount;
            Star2Percent = Star2Count * 100 / MaxStarCount;
            Star3Percent = Star3Count * 100 / MaxStarCount;
            Star4Percent = Star4Count * 100 / MaxStarCount;
            Star5Percent = Star5Count * 100 / MaxStarCount;

            if (token.TryGetValue("userAction", out JToken userAction))
            {
                if (((JObject)userAction).TryGetValue("follow", out JToken follow))
                {
                    Followed = follow.ToObject<bool>();
                }
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("hot_num_txt", out JToken hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetValue("apkname", out JToken apkname))
            {
                APKName = apkname.ToString();
            }

            if (token.TryGetValue("apklength", out JToken apklength))
            {
                APKSize = apklength.ToObject<double>().GetSizeString();
            }

            if (token.TryGetValue("votenum", out JToken votenum))
            {
                VoteNum = $"{votenum}个点评";
            }

            if (token.TryGetValue("subtitle", out JToken subtitle))
            {
                SubTitle = subtitle.ToString();
            }

            if (token.TryGetValue("followCount", out JToken followCount))
            {
                FollowNum = $"{followCount}{loader.GetString("Follow")}";
            }

            if (token.TryGetValue("introduce", out JToken introduce))
            {
                Introduce = introduce.ToString();
            }

            if (token.TryGetValue("commentCount", out JToken commentCount))
            {
                CommentNum = $"{commentCount}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetValue("apkversioncode", out JToken apkversioncode))
            {
                VersionCode = apkversioncode.ToString();
            }

            if (token.TryGetValue("apkversionname", out JToken apkversionname))
            {
                VersionName = apkversionname.ToString();
            }

            if (token.TryGetValue("downCount", out JToken downCount))
            {
                DownloadNum = $"{downCount}{loader.GetString("DownloadNum")}";
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("score", out JToken score))
            {
                RatingScore = score.ToObject<double>();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("tagList", out JToken tagList) && (tagList as JArray).Count > 0)
            {
                TagArr = tagList.Select(x => x.ToString()).ToImmutableArray();
            }

            if (token.TryGetValue("recent_follow_list", out JToken recent_follow_list) && (recent_follow_list as JArray).Count > 0)
            {
                FollowUsers = recent_follow_list
                    .OfType<JObject>()
                    .Select(x => x.TryGetValue("userInfo", out JToken userInfo)
                        ? new UserModel((JObject)userInfo) : null)
                    .OfType<UserModel>()
                    .ToImmutableArray();
            }

            if (token.TryGetValue("screenList", out JToken screenList) && (screenList as JArray).Count > 0)
            {
                ScreenshotArr = screenList.Where(x => !string.IsNullOrEmpty(x?.ToString()))
                                   .Select(x => new ImageModel(x.ToString(), ImageType.SmallImage))
                                   .ToImmutableArray();

                foreach (ImageModel item in ScreenshotArr)
                {
                    item.ContextArray = ScreenshotArr;
                }
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("UnFollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollowAsync()
        {
            UriType type = Followed ? UriType.PostAppUnfollow : UriType.PostAppFollow;

            (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, ID), null, true);
            if (!isSucceed) { return; }

            JObject token = result as JObject;

            if (token.TryGetValue("follow", out JToken follow))
            {
                Followed = follow.ToObject<bool>();
            }

            if (token.TryGetValue("followCount", out JToken followCount))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowNum = $"{followCount}{loader.GetString("Follow")}";
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("应用：{0}", Title)
                                                                .TryAppendLineJoin(" ", HotNum, CommentNum, FollowNum)
                                                                .TryAppendJoin("\n", SubTitle, Description, Introduce.HtmlToString())
                                                                .ToString();
    }
}
