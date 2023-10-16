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
        public bool IsNew { get; private set; }
        public bool IsCard { get; private set; }
        public string UserUrl { get; private set; }
        public string Message { get; private set; }
        public string UserName { get; private set; }
        public ImageModel UserAvatar { get; private set; }
        public ImageModel MessagePic { get; private set; }
        public MessageCard MessageCard { get; private set; }
        public DateTimeOffset Dateline { get; private set; }

        public MessageModel(JObject token) : base(token)
        {
            if (token.TryGetValue("fromuid", out JToken fromuid))
            {
                UID = fromuid.ToObject<int>();
                UserUrl = $"/u/{fromuid}";
                IsMe = fromuid.ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid);
            }

            if (token.TryGetValue("isnew", out JToken isnew))
            {
                IsNew = isnew.ToObject<bool>();
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
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }

            if (token.TryGetValue("fromUserAvatar", out JToken fromUserAvatar))
            {
                UserAvatar = new ImageModel(fromUserAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("message_pic", out JToken message_pic) && !string.IsNullOrEmpty(message_pic.ToString()))
            {
                if (token.TryGetValue("id", out JToken id))
                {
                    MessagePic = new ImageModel(UriHelper.GetUri(UriType.GetMessageImage, id).ToString(), ImageType.SmallMessage);
                }
            }

            if (token.TryGetValue("message_card", out JToken message_card) && !string.IsNullOrEmpty(message_card.ToString()))
            {
                MessageCard = new MessageCard(JObject.Parse(message_card.ToString()));
                IsCard = MessageCard != null;
            }
        }

        public override string ToString() => new StringBuilder().TryAppendLineFormat("{0}：", UserName)
                                                                .TryAppendLine(Message.HtmlToString())
                                                                .Append(Dateline)
                                                                .ToString();
    }

    public class MessageCard : IHasTitle
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ExtraText { get; set; }

        public ImageModel Pic { get; set; }
        public ImageModel ExtraPic { get; set; }

        public MessageCard(JObject token)
        {
            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("sub_title", out JToken sub_title))
            {
                SubTitle = sub_title.ToString();
            }

            if (token.TryGetValue("extra_text", out JToken extra_text))
            {
                ExtraText = extra_text.ToString();
                if (string.IsNullOrEmpty(Title))
                {
                    Title = ExtraText.HtmlToString();
                }
            }

            if (token.TryGetValue("pic", out JToken pic))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("extra_pic", out JToken extra_pic))
            {
                ExtraPic = new ImageModel(extra_pic.ToString(), ImageType.BigAvatar);
            }
        }
    }

}
