using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CoolapkLite.Models.Pages
{
    public class ProfileDetailModel : Entity, ISourceUserModel
    {
        public int FansNum { get; private set; }
        public int FeedNum { get; private set; }
        public int LevelNum { get; private set; }
        public int FollowNum { get; private set; }

        public string Url { get; private set; }
        public string UserName { get; private set; }
        public string LevelTodayMessage { get; private set; }
        public string NextLevelNowExperience { get; private set; }

        public long Experience { get; private set; }
        public long NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }

        public ImageModel UserAvatar { get; private set; }

        public ProfileDetailModel(JObject token) : base(token)
        {
            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = $"https://www.coolapk.com{url}";
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                FansNum = fans.ToObject<int>();
            }

            if (token.TryGetValue("feed", out JToken feed))
            {
                FeedNum = feed.ToObject<int>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                LevelNum = level.ToObject<int>();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<int>();
            }

            if (token.TryGetValue("level_today_message", out JToken level_today_message))
            {
                LevelTodayMessage = level_today_message.ToString();
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
        }

        public override string ToString() => new StringBuilder().TryAppendLine(UserName)
                                                                .AppendLineFormat("Lv.{0} {1}", LevelNum, NextLevelNowExperience)
                                                                .AppendFormat("{0}关注 {1}粉丝", FollowNum, FansNum)
                                                                .ToString();
    }
}
