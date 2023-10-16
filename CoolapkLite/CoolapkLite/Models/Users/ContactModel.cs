using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Users
{
    public class ContactModel : Entity
    {
        public bool IsFriend { get; private set; }
        public UserModel UserInfo { get; private set; }
        public DateTimeOffset Dateline { get; private set; }

        public ContactModel(JObject token) : base(token)
        {
            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("isfriend", out JToken isfriend))
            {
                IsFriend = Convert.ToBoolean(isfriend.ToObject<int>());
            }

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                UserInfo = new UserModel(userInfo);
            }
        }

        public override string ToString() => UserInfo.ToString();
    }
}
