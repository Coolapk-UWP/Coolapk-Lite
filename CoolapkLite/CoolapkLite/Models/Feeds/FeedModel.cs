using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Feeds
{
    [Flags]
    public enum FeedDisplayMode
    {
        Normal = 0x00,
        NotShowDyhName = 0x02,
        IsFirstPageFeed = 0x01,
        NotShowMessageTitle = 0x04
    }

    public class FeedModel : FeedModelBase
    {
        public bool IsStickTop { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public bool ShowDateline { get; private set; } = true;

        public FeedModel(JObject token, FeedDisplayMode mode = FeedDisplayMode.Normal) : base(token)
        {
            ShowLikes = EntityType != "forwardFeed";
            ShowDateline = !mode.HasFlag(FeedDisplayMode.IsFirstPageFeed);
            IsStickTop = token.TryGetValue("isStickTop", out JToken isStickTop) && isStickTop.ToString() == "1";
        }
    }
}
