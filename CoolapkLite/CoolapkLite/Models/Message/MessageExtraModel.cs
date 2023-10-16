using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Message
{
    public class MessageExtraModel : Entity
    {
        public string Title { get; private set; }
        public DateTimeOffset Dateline { get; private set; }

        public MessageExtraModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }
        }

        public override string ToString() => Title;
    }
}
