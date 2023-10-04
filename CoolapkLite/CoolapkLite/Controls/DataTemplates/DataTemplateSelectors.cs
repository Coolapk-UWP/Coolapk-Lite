using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Message;
using CoolapkLite.Models.Pages;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            switch (item)
            {
                case FeedModel _:
                    return Feed;
                case IUserModel _:
                    return User;
                case FeedReplyModel _:
                    return FeedReply;
                case IndexPageHasEntitiesModel IndexPageHasEntitiesModel:
                    switch (IndexPageHasEntitiesModel.EntitiesType)
                    {
                        case EntityType.Image: return Images;
                        default: return Others;
                    }
                case LikeNotificationModel _:
                    return LikeNotify;
                case SimpleNotificationModel _:
                    return CommentMe;
                case MessageNotificationModel _:
                    return MessageNotify;
                case AtCommentMeNotificationModel _:
                    return AtCommentMe;
                case IHasDescription _ when item is IHasSubtitle:
                    return SubtitleList;
                case IHasDescription _:
                    return List;
                default:
                    return Others;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ProfileCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case IndexPageHasEntitiesModel IndexPageHasEntitiesModel:
                    switch (IndexPageHasEntitiesModel.EntitiesType)
                    {
                        case EntityType.TextLinks: return TextLinkList;
                        default: return ImageTextScrollCard;
                    }

                case IndexPageOperationCardModel IndexPageOperationCardModel:
                    switch (IndexPageOperationCardModel.OperationType)
                    {
                        case OperationType.ShowTitle: return TitleCard;
                        default: return Others;
                    }

                default:
                    return Others;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ProfileItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; }
        public DataTemplate History { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextLink { get; set; }
        public DataTemplate Collection { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case CollectionModel _:
                    return Collection;
                case IndexPageModel IndexPageModel:
                    switch (IndexPageModel?.EntityType)
                    {
                        case "topic":
                        case "recentHistory": return IconLink;
                        case "textLink": return TextLink;
                        case "collection":
                        case "history": return History;
                        default: return Empty;
                    }
                default:
                    return Empty;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }

        protected override DataTemplate SelectTemplateCore(object item) => item is AppModel ? App : SearchWord;

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ChatCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate MessageLeft { get; set; }
        public DataTemplate MessageRight { get; set; }
        public DataTemplate MessageExtra { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case MessageModel messageModel:
                    return messageModel.IsMe ? MessageRight : MessageLeft;
                case MessageExtraModel _:
                    return MessageExtra;
                default:
                    return Others;
            }
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
