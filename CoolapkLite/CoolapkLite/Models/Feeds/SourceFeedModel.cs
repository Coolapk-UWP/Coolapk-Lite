using CoolapkLite.Core.Models;
using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkLite.Models.Feeds
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string Message { get; private set; }
        public string UserName { get; private set; }
        public string Dateline { get; private set; }

        public SourceFeedModel(JObject token) : base(token)
        {
            if (token.TryGetValue("url", out JToken uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else if (token.TryGetValue("id", out JToken id))
            {
                Url = $"/feed/{id.ToString().Replace("\"", string.Empty)}";
            }

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<double>().ConvertUnixTimeStampToReadable();
            }
        }
    }
}
