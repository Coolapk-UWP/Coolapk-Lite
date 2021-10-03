using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models.Feeds
{
    internal class FeedModel : FeedModelBase
    {
        public string Uurl { get; private set; }
        public bool IsStickTop { get; private set; }
        public bool ShowDateline { get; private set; } = true;

        internal enum FeedDisplayMode
        {
            normal = 0,
            notShowDyhName = 0x02,
            isFirstPageFeed = 0x01,
            notShowMessageTitle = 0x04
        }

        public FeedModel(JObject token, FeedDisplayMode mode = FeedDisplayMode.normal) : base(token)
        {
            IsStickTop = token.TryGetValue("isStickTop", out JToken j) && int.Parse(j.ToString()) == 1;

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                if (userInfo.TryGetValue("url", out JToken url))
                {
                    Uurl = url.ToString();
                }
            }
            else if (token.TryGetValue("uid", out JToken uid))
            {
                Uurl = $"/u/{uid}";
            }
        }
    }
}
