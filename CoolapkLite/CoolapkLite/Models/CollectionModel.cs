using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CoolapkLite.Models
{
    public class CollectionModel : Entity, IHasSubtitle
    {
        public int ID { get; private set; }
        public string Url { get; private set; }
        public int ItemNum { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public ImageModel Cover { get; private set; }
        public string Description { get; private set; }

        public ImageModel Pic => Cover;

        public CollectionModel(JObject token) : base(token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("item_num", out JToken item_num))
            {
                ItemNum = item_num.ToObject<int>();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("subTitle", out JToken subTitle))
            {
                SubTitle = subTitle.ToString();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("cover_pic", out JToken cover_pic))
            {
                Cover = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }
        }

        public override string ToString() => new StringBuilder().AppendFormat("收藏单：{0}", Title)
                                                                .TryAppendLine(SubTitle)
                                                                .Append(Description)
                                                                .ToString();
    }
}
