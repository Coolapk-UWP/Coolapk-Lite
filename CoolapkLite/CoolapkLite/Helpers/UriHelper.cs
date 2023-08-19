using System;

namespace CoolapkLite.Helpers
{
    public enum UriType
    {
        DataList,
        CheckLoginInfo,
        CreateFeed,
        CreateFeedReply,
        CreateReplyReply,
        GetAnswers,
        GetAppDetail,
        GetAppFeeds,
        GetCollectionContents,
        GetCollectionDetail,
        GetCollectionList,
        GetDyhDetail,
        GetDyhFeeds,
        GetProductDetail,
        GetProductDetailByName,
        GetProductFeeds,
        GetFeedDetail,
        GetFeedReplies,
        GetFeedInfos,
        GetVoteComments,
        GetChangeHistoryList,
        GetHotReplies,
        GetIndexPage,
        GetIndexPageNames,
        GetLikeList,
        GetMyPageCard,
        GetChats,
        GetNotifications,
        GetNotificationNumbers,
        GetReplyReplies,
        GetSearchWords,
        GetShareList,
        GetTagDetail,
        GetTagFeeds,
        GetTopicDetail,
        GetDeviceFeeds,
        GetUserFeeds,
        GetUserHistory,
        GetUserList,
        GetUserRecentHistory,
        GetUserSpace,
        GetUserProfile,
        GetUserFollows,
        PostUserFollow,
        PostTopicFollow,
        PostDyhFollow,
        PostCollectionFollow,
        PostFeedLike,
        PostCollectionLike,
        PostUserUnfollow,
        PostTopicUnfollow,
        PostDyhUnfollow,
        PostCollectionUnfollow,
        PostFeedUnlike,
        PostCollectionUnlike,
        OperateProductFollow,
        OOSUploadPrepare,
        RequestValidate,
        UploadImage,
        Search,
        SearchTarget,
        SearchFeeds,
        SearchWords,
        SearchCreateTags,
        SearchCreateUsers,
    }

    public static class UriHelper
    {
        private static bool IsUseAPI2 => SettingsHelper.Get<bool>(SettingsHelper.IsUseAPI2);

        public static readonly Uri BaseUri = new Uri("https://api.coolapk.com");
        public static readonly Uri Base2Uri = new Uri("https://api2.coolapk.com");
        public static readonly Uri CoolapkUri = new Uri("https://www.coolapk.com");

        public const string LoginUri = "https://account.coolapk.com/auth/loginByCoolapk";

        public static Uri GetUri(UriType type, params object[] args)
        {
            string u = string.Format(GetTemplate(type), args);
            return new Uri(IsUseAPI2 ? Base2Uri : BaseUri, u);
        }

        public static Uri GetOldUri(UriType type, params object[] args)
        {
            string u = string.Format(GetTemplate(type), args);
            return new Uri(BaseUri, u);
        }

        private static string GetTemplate(UriType type)
        {
            switch (type)
            {
                case UriType.DataList: return "/v6/page/dataList?url={0}{1}";
                case UriType.CheckLoginInfo: return "/v6/account/checkLoginInfo";
                case UriType.CreateFeed: return "/v6/feed/createFeed";
                case UriType.CreateFeedReply: return "/v6/feed/reply?id={0}&type=feed";
                case UriType.CreateReplyReply: return "/v6/feed/reply?id={0}&type=reply";
                case UriType.GetAnswers: return "/v6/question/answerList?id={0}&sort={1}&page={2}{3}{4}";
                case UriType.GetAppDetail: return "/v6/apk/detail?id={0}&installed=0";
                case UriType.GetAppFeeds: return "/v6/page/dataList?url=%23/feed/apkCommentList?isIncludeTop=1&id={0}&subTitle=&page={1}{2}{3}";
                case UriType.GetCollectionContents: return "/v6/collection/itemList?id={0}&page={1}{2}";
                case UriType.GetCollectionDetail: return "/v6/collection/detail?id={0}";
                case UriType.GetCollectionList: return "/v6/collection/list?uid={0}&page={1}{2}";
                case UriType.GetDyhDetail: return "/v6/dyh/detail?dyhId={0}";
                case UriType.GetDyhFeeds: return "/v6/dyhArticle/list?dyhId={0}&type={1}&page={2}{3}{4}";
                case UriType.GetProductDetail: return "/v6/product/detail?id={0}";
                case UriType.GetProductDetailByName: return "/v6/product/detail?name={0}";
                case UriType.GetProductFeeds: return "/v6/page/dataList?url=/page?url=/product/feedList?type={4}&id={0}&page={1}{2}{3}";
                case UriType.GetFeedDetail: return "/v6/feed/detail?id={0}";
                case UriType.GetFeedReplies: return "/v6/feed/replyList?id={0}&listType={1}&page={2}{3}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={4}";
                case UriType.GetFeedInfos: return "/v6/feed/{4}List?id={0}&page={1}{2}{3}";
                case UriType.GetVoteComments: return "/v6/vote/commentList?fid={0}{1}&page={2}{3}{4}";
                case UriType.GetChangeHistoryList: return "/v6/feed/changeHistoryList?id={0}";
                case UriType.GetHotReplies: return "/v6/feed/hotReplyList?id={0}&page={1}{2}&discussMode=1";
                case UriType.GetIndexPage: return "/v6{0}{1}page={2}";
                case UriType.GetIndexPageNames: return "/v6/main/init";
                case UriType.GetLikeList: return "/v6/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}";
                case UriType.GetMyPageCard: return "/v6/account/loadConfig?key=my_page_card_config";
                case UriType.GetChats: return "/v6/message/list?page={0}{1}{2}";
                case UriType.GetNotifications: return "/v6/notification/{0}?page={1}{2}{3}";
                case UriType.GetNotificationNumbers: return "/v6/notification/checkCount";
                case UriType.GetReplyReplies: return "/v6/feed/replyList?id={0}&listType=&page={1}{2}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0";
                case UriType.GetSearchWords: return "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
                case UriType.GetShareList: return "/v6/feed/forwardList?id={0}&type={1}&page={2}";
                case UriType.GetTagDetail: return "/v6/topic/newTagDetail?tag={0}";
                case UriType.GetTagFeeds: return "/v6/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0";
                case UriType.GetTopicDetail: return "/v6/topic/tagDetail?tag={0}";
                case UriType.GetDeviceFeeds: return "/v6/topic/deviceFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0";
                case UriType.GetUserFeeds: return "/v6/user/{4}List?uid={0}&page={1}{2}{3}&isIncludeTop=1";
                case UriType.GetUserHistory: return "/v6/user/hitHistoryList?page={0}{1}{2}";
                case UriType.GetUserList: return "/v6/user/{0}?uid={1}&page={2}{3}{4}&isIncludeTop=1";
                case UriType.GetUserRecentHistory: return "/v6/user/recentHistoryList?page={0}{1}{2}";
                case UriType.GetUserSpace: return "/v6/user/space?uid={0}";
                case UriType.GetUserProfile: return "/v6/user/profile?uid={0}";
                case UriType.GetUserFollows: return "/v6/user/{0}?uid={1}&page={2}";
                case UriType.PostUserFollow: return "/v6/user/follow?uid={0}";
                case UriType.PostTopicFollow: return "/v6/feed/followTag?tag={0}";
                case UriType.PostDyhFollow: return "/v6/dyh/follow?dyhId={0}";
                case UriType.PostCollectionFollow: return "/v6/collection/follow";
                case UriType.PostFeedLike: return "/v6/feed/like{0}?id={1}";
                case UriType.PostCollectionLike: return "/v6/collection/like";
                case UriType.PostUserUnfollow: return "/v6/user/unfollow?uid={0}";
                case UriType.PostDyhUnfollow: return "/v6/dyh/unFollow?dyhId={0}";
                case UriType.PostTopicUnfollow: return "/v6/feed/unFollowTag?tag={0}";
                case UriType.PostCollectionUnfollow: return "/v6/collection/unFollow";
                case UriType.PostFeedUnlike: return "/v6/feed/unlike{0}?id={1}";
                case UriType.PostCollectionUnlike: return "/v6/collection/unLike";
                case UriType.OperateProductFollow: return "/v6/product/changeFollowStatus";
                case UriType.OOSUploadPrepare: return "/v6/upload/ossUploadPrepare";
                case UriType.RequestValidate: return "/v6/account/requestValidate";
                case UriType.UploadImage: return "/v6/feed/uploadImage?fieldName=picFile&uploadDir={0}";
                case UriType.Search: return "/v6/search?type={0}&searchValue={1}&page={2}{3}&showAnonymous=-1";
                case UriType.SearchTarget: return "/v6/search?type={0}&feedType={1}&sort={2}&searchValue={3}&pageTape={4}&pageParam={5}&page={6}{7}&showAnonymous=-1";
                case UriType.SearchFeeds: return "/v6/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}&showAnonymous=-1";
                case UriType.SearchWords: return "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
                case UriType.SearchCreateTags: return "/v6/feed/searchTag?q={0}&page={1}{2}";
                case UriType.SearchCreateUsers: return "/v6/user/search?q={0}&page={1}{2}";
                default: throw new ArgumentException($"{typeof(UriType).FullName}值错误");
            }
        }
    }
}
