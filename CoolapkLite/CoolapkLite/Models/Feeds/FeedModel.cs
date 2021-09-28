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
        internal enum FeedDisplayMode
        {
            normal = 0,
            notShowDyhName = 0x02,
            isFirstPageFeed = 0x01,
            notShowMessageTitle = 0x04
        }

        public FeedModel(JObject token, FeedDisplayMode mode = FeedDisplayMode.normal) : base(token)
        {

        }
    }
}
