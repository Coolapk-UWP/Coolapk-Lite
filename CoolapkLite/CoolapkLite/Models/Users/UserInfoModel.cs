using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkLite.Models.Users
{
    public class UserInfoModel : Entity, ISourceUserModel
    {
        public int UID { get; private set; }
        public int Level { get; private set; }
        public int RegDate { get; private set; }
        public int LoginTime { get; private set; }
        public int BlockStatus { get; private set; }

        public string RegIP { get; private set; }
        public string Email { get; private set; }
        public string Mobile { get; private set; }
        public string LoginIP { get; private set; }
        public string UserName { get; private set; }
        public string LoginText { get; private set; }
        public string VerifyTitle { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public long Experience { get; private set; }
        public long NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }

        public string Url => $"/u/{UID}";

        public UserInfoModel(JObject token) : base(token)
        {
            if (token == null) { return; }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                Level = level.ToObject<int>();
            }

            if (token.TryGetValue("regdate", out JToken regdate))
            {
                RegDate = regdate.ToObject<int>();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginTime = logintime.ToObject<int>();
                LoginText = $"{logintime.ToObject<long>().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetValue("regip", out JToken regip))
            {
                RegIP = regip.ToString();
            }

            if (token.TryGetValue("email", out JToken email))
            {
                Email = email.ToString();
            }

            if (token.TryGetValue("mobile", out JToken mobile))
            {
                Mobile = mobile.ToString();
            }

            if (token.TryGetValue("loginip", out JToken loginip))
            {
                LoginIP = loginip.ToString();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
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
        }
    }
}
