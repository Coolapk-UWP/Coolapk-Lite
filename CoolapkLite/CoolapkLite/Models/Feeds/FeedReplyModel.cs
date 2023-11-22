using CoolapkLite.Helpers;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models.Feeds
{
    public class FeedReplyModel : SourceFeedReplyModel, ICanLike, ICanReply, ICanCopy
    {
        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set => SetProperty(ref likeNum, value);
        }

        private int replyNum;
        public int ReplyNum
        {
            get => replyNum;
            set => SetProperty(ref replyNum, value);
        }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int ReplyRowsMore { get; private set; }
        public int ReplyRowsCount { get; private set; }

        public bool ShowReplyRow { get; private set; }

        public ImmutableArray<SourceFeedReplyModel> ReplyRows { get; private set; } = ImmutableArray<SourceFeedReplyModel>.Empty;

        public FeedReplyModel(JObject token, bool showReplyRow = true) : base(token)
        {
            ShowReplyRow = showReplyRow;

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("likenum", out JToken likenum))
            {
                LikeNum = likenum.ToObject<int>();
            }

            if (token.TryGetValue("replynum", out JToken replynum))
            {
                ReplyNum = replynum.ToObject<int>();
            }

            if (token.TryGetValue("replyRowsMore", out JToken replyRowsMore))
            {
                ReplyRowsMore = replyRowsMore.ToObject<int>();
            }

            if (token.TryGetValue("replyRowsCount", out JToken replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount.ToObject<int>();
            }

            if (token.TryGetValue("replyRows", out JToken replyRows))
            {
                ReplyRows = replyRows.OfType<JObject>().Select(item => new SourceFeedReplyModel(item)).ToImmutableArray();
            }
        }

        public async Task ChangeLikeAsync()
        {
            UriType type = Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetV1Uri(type, "Reply", ID), null, true);
            if (!isSucceed) { return; }
            Liked = !Liked;
            if (result.ToObject<int>() is int likenum && likenum >= 0)
            {
                LikeNum = likenum;
            }
        }

        public override string ToString() => new StringBuilder().AppendLineFormat("{0}的回复", UserInfo?.UserName)
                                                                .TryAppendLine(Message.HtmlToString())
                                                                .AppendFormat("{0}点赞 {1}回复", LikeNum, ReplyNum)
                                                                .ToString();
    }
}
