using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CoolapkLite.Models.Message
{
    public class MessageModel : Entity
    {
        public int UID { get; private set; }
        public bool IsMe { get; private set; }
        public string UserUrl { get; private set; }
        public string Message { get; private set; }
        public string UserName { get; private set; }
        public string Dateline { get; private set; }
        public DateTime DateTime { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public MessageModel(JObject token) : base(token)
        {
            if (token.TryGetValue("fromuid", out JToken fromuid))
            {
                UID = fromuid.ToObject<int>();
                UserUrl = $"/u/{fromuid}";
                IsMe = fromuid.ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid);
            }

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("fromusername", out JToken fromusername))
            {
                UserName = fromusername.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                DateTimeOffset dateTimeOffset = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
                Dateline = dateTimeOffset.ConvertDateTimeOffsetToReadable();
                DateTime = dateTimeOffset.LocalDateTime;
            }

            if (token.TryGetValue("fromUserAvatar", out JToken fromUserAvatar))
            {
                UserAvatar = new ImageModel(fromUserAvatar.ToString(), ImageType.BigAvatar);
            }
        }

        public override string ToString() => new StringBuilder().TryAppendLineFormat("{0}：", UserName)
                                                                .TryAppendLine(Message.HtmlToString())
                                                                .Append(Dateline)
                                                                .ToString();
    }
}
