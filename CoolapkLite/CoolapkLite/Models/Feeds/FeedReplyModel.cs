using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models.Feeds
{
    internal class FeedReplyModel : Entity
    {
        public FeedReplyModel(JObject o) : base(o)
        {
        }
    }
}
