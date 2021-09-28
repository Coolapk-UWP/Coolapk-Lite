using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models.Feeds
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string UserName { get; private set; }

        public SourceFeedModel(JObject o) : base(o)
        {
            if (o.TryGetValue("url", out JToken uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else if (o.TryGetValue("id", out JToken id) && !string.IsNullOrEmpty(id.ToString()))
            {
                Url = $"/feed/{id.ToString().Replace("\"", string.Empty)}";
            }

            if (o.TryGetValue("username", out JToken username) && !string.IsNullOrEmpty(username.ToString()))
            {
                UserName = username.ToString();
            }
        }
    }
}
