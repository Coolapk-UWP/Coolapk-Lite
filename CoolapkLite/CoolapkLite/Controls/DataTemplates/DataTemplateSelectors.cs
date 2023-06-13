using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Pages;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkLite.Models.Feeds.FeedModel;

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate CommentMe { get; set; }
        public DataTemplate LikeNotify { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        public DataTemplate SubtitleList { get; set; }
        public DataTemplate MessageNotify { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel) { return Feed; }
            else if (item is UserModel) { return User; }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.Image: return Images;
                    default: return Others;
                }
            }
            else if (item is LikeNotificationModel) { return LikeNotify; }
            else if (item is SimpleNotificationModel) { return CommentMe; }
            else
            {
                return item is MessageNotificationModel
                ? MessageNotify
                : item is AtCommentMeNotificationModel
                ? AtCommentMe
                : item is IHasDescription ? List : item is IHasSubtitle ? SubtitleList : Others;
            }
        }
    }

    public sealed class ProfileCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.TextLinks: return TextLinkList;
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel IndexPageOperationCardModel)
            {
                switch (IndexPageOperationCardModel.OperationType)
                {
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else { return Others; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ProfileItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; }
        public DataTemplate History { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextLink { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is CollectionModel) { return History; }
            else if (item is IndexPageModel IndexPageModel)
            {
                switch (IndexPageModel?.EntityType)
                {
                    case "topic":
                    case "recentHistory": return IconLink;
                    case "textLink": return TextLink;
                    case "collection":
                    case "history": return History;
                    default: return Empty;
                }
            }
            else { return Empty; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            //if (item is AppModel) return App;
            return SearchWord;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JObject json, bool isHotFeedPage = false)
        {
            switch (json.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(json, isHotFeedPage ? FeedDisplayMode.IsFirstPageFeed : FeedDisplayMode.Normal);
                case "history": return new HistoryModel(json);
                case "collection": return new CollectionModel(json);
                default:
                    if (json.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "headCard":
                            case "imageCard":
                            case "imageCarouselCard_1": return new IndexPageHasEntitiesModel(json, EntityType.Image);
                            default: return null;
                        }
                    }
                    return null;
            }
        }

        public static IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return GetEntity(json);
        }
    }
}
