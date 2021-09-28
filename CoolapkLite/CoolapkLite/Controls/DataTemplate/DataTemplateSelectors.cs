using CoolapkLite.Core.Models;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CoolapkLite.Models.Feeds.FeedModel;

namespace CoolapkLite.Controls.DataTemplate
{
    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JObject jo, bool isHotFeedPage = false)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                default: return null;
            }
        }
    }
}
