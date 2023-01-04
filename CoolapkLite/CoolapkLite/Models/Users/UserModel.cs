﻿using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkLite.Models.Users
{
    public class UserModel : Entity, IListWithSubtitle
    {
        public int UID { get; private set; }
        public int Level { get; private set; }
        public int Status { get; private set; }
        public int RegDate { get; private set; }
        public int FansNum { get; private set; }
        public int LoginTime { get; private set; }
        public int FollowNum { get; private set; }
        public int Experience { get; private set; }
        public int BlockStatus { get; private set; }

        public string Bio { get; private set; }
        public string UserName { get; private set; }
        public string SubTitle { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public string Url => $"/u/{UID}";
        public string Title => UserName;

        public ImageModel Pic => UserAvatar;

        public UserModel(JObject token) : base(token)
        {
            if (token == null) { return; }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                Level = level.ToObject<int>();
            }

            if (token.TryGetValue("cover", out JToken cover))
            {
                Cover = new ImageModel(cover.ToString(), Helpers.ImageType.OriginImage);
            }

            if (token.TryGetValue("status", out JToken status))
            {
                Status = status.ToObject<int>();
            }

            if (token.TryGetValue("regdate", out JToken regdate))
            {
                RegDate = regdate.ToObject<int>();
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                FansNum = fans.ToObject<int>();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginTime = logintime.ToObject<int>();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<int>();
            }

            if (token.TryGetValue("experience", out JToken experience))
            {
                Experience = experience.ToObject<int>();
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), Helpers.ImageType.OriginImage);
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
