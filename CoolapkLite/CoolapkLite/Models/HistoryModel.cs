using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CoolapkLite.Models
{
    public class HistoryModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Description { get; private set; }

        public HistoryModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("typeName", out JToken typeName) && !string.IsNullOrEmpty(typeName.ToString()))
            {
                Description = typeName.ToString();
            }
            else if (token.TryGetValue("dateline", out JToken dateline))
            {
                Description = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
        }

        public override string ToString() => new StringBuilder().TryAppendLine(Title)
                                                                .Append(Description)
                                                                .ToString();
    }
}
