using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.Models.Users
{
    public class UserModel : Entity, IUserModel, IHasSubtitle
    {
        private readonly int fansNum;
        int IUserModel.FansNum => fansNum;

        private readonly int followNum;
        int IUserModel.FollowNum => followNum;

        public int UID { get; private set; }
        public int Level { get; private set; }
        public int Status { get; private set; }
        public int BlockStatus { get; private set; }

        public string Bio { get; private set; }
        public string FansNum { get; private set; }
        public string UserName { get; private set; }
        public string SubTitle { get; private set; }
        public string LoginText { get; private set; }
        public string FollowNum { get; private set; }
        public string VerifyTitle { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public long Experience { get; private set; }
        public long NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }

        public DateTimeOffset RegDate { get; private set; }
        public DateTimeOffset LoginTime { get; private set; }

        public string Url => $"/u/{UID}";
        public string Title => UserName;
        public string Description => Bio;

        public ImageModel Pic => UserAvatar;

        public UserModel(JObject token) : base(token)
        {
            if (token == null) { return; }

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                fansNum = fans.ToObject<int>();
                FansNum = $"{fansNum}{loader.GetString("Fan")}";
            }

            if (token.TryGetValue("level", out JToken level))
            {
                Level = level.ToObject<int>();
            }

            if (token.TryGetValue("cover", out JToken cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetValue("status", out JToken status))
            {
                Status = status.ToObject<int>();
            }

            if (token.TryGetValue("regdate", out JToken regdate))
            {
                RegDate = regdate.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginTime = logintime.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
                LoginText = $"{LoginTime.ConvertDateTimeOffsetToReadable()}活跃";
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                followNum = follow.ToObject<int>();
                FollowNum = $"{followNum}{loader.GetString("Follow")}";
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

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("用户：{0}", UserName)
                                                                .TryAppendLine(LoginText)
                                                                .AppendLineFormat("{0}关注 {1}粉丝 {2}", FollowNum, FansNum, BlockStatus)
                                                                .Append(Bio)
                                                                .ToString();
    }
}
