using CoolapkLite.Core.Models;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkLite.Models
{
    public class HistoryModel : Entity
    {
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Id { get; private set; }

        public HistoryModel(JObject o) : base(o)
        {
            if (o.TryGetValue("id", out JToken id) && !string.IsNullOrEmpty(id.ToString()))
            {
                Id = id.ToString();
            }
            if (o.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            if (o.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }
            if (o.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (o.TryGetValue("target_type_title", out JToken target_type_title) && !string.IsNullOrEmpty(target_type_title.ToString()))
            {
                Description = target_type_title.ToString();
            }
            else if (o.TryGetValue("dateline", out JToken dateline) && !string.IsNullOrEmpty(dateline.ToString()))
            {
                Description = double.Parse(dateline.ToString()).ConvertUnixTimeStampToReadable();
            }
            if (o.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
        }
    }
}
