using CoolapkLite.Core.Models;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkLite.Models.Feeds
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string Uurl { get; private set; }
        public string Message { get; private set; }
        public string UserName { get; private set; }
        public string Dateline { get; private set; }
        public string ShareUrl { get; private set; }
        public BackgroundImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

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

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                if (userInfo.TryGetValue("url", out JToken url))
                {
                    Uurl = url.ToString();
                }
            }
            else if (token.TryGetValue("uid", out JToken uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetValue("shareUrl", out JToken shareUrl) && !string.IsNullOrEmpty(shareUrl.ToString()))
            {
                ShareUrl = shareUrl.ToString();
            }
            else
            {
                ShareUrl = "https://www.coolapk.com" + Url != null ? Url.Replace("/question/", "/feed/") : string.Empty; ;
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

            if (token.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString()))
            {
                PicArr = (from item in picArr select new ImageModel(item.ToString(), ImageType.SmallImage)).ToImmutableArray();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new BackgroundImageModel(pic.ToString(), ImageType.SmallImage);
            }
        }
    }
}
