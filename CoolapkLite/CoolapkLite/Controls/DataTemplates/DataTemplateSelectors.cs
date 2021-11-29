using CoolapkLite.Core.Models;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkLite.Models.Feeds.FeedModel;

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate History { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item.GetType().Name)
            {
                case "FeedModel": return Feed;
                case "HistoryModel": return History;
                case "IndexPageHasEntitiesModel":
                    switch ((item as IndexPageHasEntitiesModel).EntitiesType)
                    {
                        case EntityType.Image: return Images;
                        default: return Others;
                    }
                default: return Others;
            }
        }
    }

    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JObject jo, bool isHotFeedPage = false)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "history": return new HistoryModel(jo);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "headCard":
                            case "imageCard":
                            case "imageCarouselCard_1": return new IndexPageHasEntitiesModel(jo, EntityType.Image);
                            default: return null;
                        }
                    }
                    return null;
            }
        }
    }
}
