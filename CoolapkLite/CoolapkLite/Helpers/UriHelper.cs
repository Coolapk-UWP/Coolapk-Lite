using System;

namespace CoolapkLite.Helpers
{
    public static class UriType
    {
        public const string DataList = "/v6/page/dataList?url={0}&page={1}{2}{3}";
        public const string CheckLoginInfo = "/v6/account/checkLoginInfo";
        public const string CreateFeed = "/v6/feed/createFeed";
        public const string CreateFeedReply = "/v6/feed/reply?id={0}&type=feed";
        public const string CreateReplyReply = "/v6/feed/reply?id={0}&type=reply";
        public const string GetAnswers = "/v6/question/answerList?id={0}&sort={1}&page={2}{3}{4}";
        public const string GetAppDetail = "/v6/apk/detail?id={0}";
        public const string GetAppFeeds = "/v6/page/dataList?url=%23/feed/apkCommentList?isIncludeTop=1&id={0}&sort={1}&page={2}{3}{4}";
        public const string GetAppRates = "/v6/page/dataList?url=%23/feed/apkCommentList?type=5&id={0}&page={1}{2}{3}";
        public const string GetCollectionContents = "/v6/collection/itemList?id={0}&page={1}{2}{3}";
        public const string GetCollectionDetail = "/v6/collection/detail?id={0}";
        public const string GetCollectionList = "/v6/collection/list?uid={0}&page={1}{2}{3}";
        public const string GetDyhDetail = "/v6/dyh/detail?dyhId={0}";
        public const string GetDyhFeeds = "/v6/dyhArticle/list?dyhId={0}&type={1}&page={2}{3}{4}";
        public const string GetProductDetail = "/v6/product/detail?id={0}";
        public const string GetProductDetailByName = "/v6/product/detail?name={0}";
        public const string GetProductFeeds = "/v6/page/dataList?url=/page?url=/product/feedList?type={4}&id={0}&page={1}{2}{3}";
        public const string GetFeedDetail = "/v6/feed/detail?id={0}";
        public const string GetFeedReplies = "/v6/feed/replyList?id={0}&listType={1}&page={2}{3}{4}&discussMode=1&feedType=feed&fromFeedAuthor={5}";
        public const string GetVoteComments = "/v6/vote/commentList?fid={0}{1}&page={2}{3}{4}";
        public const string GetChangeHistoryList = "/v6/feed/changeHistoryList?id={0}";
        public const string GetHotReplies = "/v6/feed/hotReplyList?id={0}&page={1}{2}{3}&discussMode=1";
        public const string GetInitPage = "/v6/main/init?t={0}";
        public const string GetIndexPage = "/v6{0}{1}page={2}{3}{4}";
        public const string GetLikeList = "/v6/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}{3}";
        public const string GetMyPageCard = "/v6/account/loadConfig?key=my_page_card_config";
        public const string GetMessageList = "/v6/message/list?page={0}{1}{2}";
        public const string GetMessageRead = "/v6/message/read?ukey={0}";
        public const string GetMessageChat = "/v6/message/chat?ukey={0}&page={1}{2}{3}";
        public const string GetMessageImage = "/v6/message/showImage?id={0}";
        public const string GetNotifications = "/v6/notification/{0}?page={1}{2}{3}";
        public const string GetNotificationNumbers = "/v6/notification/checkCount";
        public const string GetReplyReplies = "/v6/feed/replyList?id={0}&page={1}{2}{3}&feedType=feed_reply";
        public const string GetSearchWords = "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
        public const string GetShareList = "/v6/feed/forwardList?id={0}&type={1}&page={2}{3}{4}";
        public const string GetTagDetail = "/v6/topic/newTagDetail?tag={0}";
        public const string GetTagFeeds = "/v6/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}";
        public const string GetTopicDetail = "/v6/topic/tagDetail?tag={0}";
        public const string GetDeviceFeeds = "/v6/topic/deviceFeedList?tag={0}&page={1}{2}{3}&listType={4}";
        public const string GetUserFeeds = "/v6/user/{4}List?uid={0}&page={1}{2}{3}&isIncludeTop=1";
        public const string GetUserHistory = "/v6/user/hitHistoryList?page={0}{1}{2}";
        public const string GetUserList = "/v6/user/{0}?uid={1}&page={2}{3}{4}&isIncludeTop=1";
        public const string GetUserRecentHistory = "/v6/user/recentHistoryList?page={0}{1}{2}";
        public const string GetUserSpace = "/v6/user/space?uid={0}";
        public const string GetUserProfile = "/v6/user/profile?uid={0}";
        public const string GetRemarkList = "/v6/user/remarkList?uid={0}";
        public const string GetCaptchaImage = "/v6/account/captchaImage?time={0}&w={1}&h={2}";
        public const string PostUpdateRemark = "/v6/user/updateRemark";
        public const string PostUserFollow = "/v6/user/follow?uid={0}";
        public const string PostTopicFollow = "/v6/feed/followTag?tag={0}";
        public const string PostDyhFollow = "/v6/dyh/follow?dyhId={0}";
        public const string PostCollectionFollow = "/v6/collection/follow";
        public const string PostAppFollow = "/v6/apk/follow?id={0}";
        public const string PostFeedLike = "/v6/feed/like{0}?id={1}";
        public const string PostCollectionLike = "/v6/collection/like";
        public const string PostUserUnfollow = "/v6/user/unfollow?uid={0}";
        public const string PostDyhUnfollow = "/v6/dyh/unFollow?dyhId={0}";
        public const string PostTopicUnfollow = "/v6/feed/unFollowTag?tag={0}";
        public const string PostCollectionUnfollow = "/v6/collection/unFollow";
        public const string PostAppUnfollow = "/v6/apk/unfollow?id={0}";
        public const string PostFeedUnlike = "/v6/feed/unlike{0}?id={1}";
        public const string PostCollectionUnlike = "/v6/collection/unLike";
        public const string SendMessage = "/v6/message/send?uid={0}";
        public const string OperateProductFollow = "/v6/product/changeFollowStatus";
        public const string OOSUploadPrepare = "/v6/upload/ossUploadPrepare";
        public const string RequestValidate = "/v6/account/requestValidate";
        public const string UploadImage = "/v6/feed/uploadImage?fieldName=picFile&uploadDir={0}";
        public const string Search = "/v6/search?type={0}&searchValue={1}&page={2}{3}{4}&showAnonymous=-1";
        public const string SearchTarget = "/v6/search?type={0}&feedType={1}&sort={2}&searchValue={3}&pageType={4}&pageParam={5}&page={6}{7}{8}&showAnonymous=-1";
        public const string SearchFeeds = "/v6/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}{5}&showAnonymous=-1";
        public const string SearchWords = "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
        public const string SearchCreateTags = "/v6/feed/searchTag?q={0}&page={1}{2}{3}";
        public const string SearchCreateUsers = "/v6/user/search?q={0}&page={1}{2}{3}";
    }

    public static class UriHelper
    {
        public static bool IsUseAPI2 { get; set; } = SettingsHelper.Get<bool>(SettingsHelper.IsUseAPI2);

        public static readonly Uri BaseUri = new Uri("https://api.coolapk.com");
        public static readonly Uri Base2Uri = new Uri("https://api2.coolapk.com");
        public static readonly Uri CoolapkUri = new Uri("https://www.coolapk.com");

        public const string LoginUri = "https://account.coolapk.com/auth/loginByCoolapk";

        public static Uri GetUri(string uri, params object[] args)
        {
            string u = string.Format(uri, args);
            return new Uri(IsUseAPI2 ? Base2Uri : BaseUri, u);
        }

        public static Uri GetV1Uri(string uri, params object[] args)
        {
            string u = string.Format(uri, args);
            return new Uri(BaseUri, u);
        }
    }
}
