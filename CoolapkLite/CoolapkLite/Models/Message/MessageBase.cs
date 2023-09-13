using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Message
{
    public class MessageExtraModel : Entity
    {
        public string Title { get; private set; }
        public string Dateline { get; private set; }
        public DateTime DateTime { get; private set; }

        public MessageExtraModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                DateTimeOffset dateTimeOffset = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
                Dateline = dateTimeOffset.ConvertDateTimeOffsetToReadable();
                DateTime = dateTimeOffset.LocalDateTime;
            }
        }

        public override string ToString() => Title;
    }
}
