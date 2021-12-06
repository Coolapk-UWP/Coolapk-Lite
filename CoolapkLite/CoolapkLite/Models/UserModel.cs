using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;

namespace CoolapkLite.Models
{
    public class UserModel : Entity
    {
        public int UID { get; private set; }
        public int Level { get; private set; }
        public int Status { get; private set; }
        public int BlockStatus { get; private set; }
        public string UserName { get; private set; }
        public double Experience { get; private set; }

        public UserModel(JObject token) : base(token)
        {
            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;

                if (userInfo.TryGetValue("uid", out JToken uid))
                {
                    UID = uid.ToObject<int>();
                }

                if (userInfo.TryGetValue("level", out JToken level))
                {
                    Level = level.ToObject<int>();
                }

                if (userInfo.TryGetValue("status", out JToken status))
                {
                    Status = status.ToObject<int>();
                }

                if (userInfo.TryGetValue("username", out JToken username))
                {
                    UserName = username.ToString();
                }

                if (userInfo.TryGetValue("experience", out JToken experience))
                {
                    Experience = experience.ToObject<double>();
                }
            }
        }
    }
}
