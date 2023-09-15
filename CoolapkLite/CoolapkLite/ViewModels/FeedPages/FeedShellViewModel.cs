using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel
    {
        protected string ID { get; set; }

        public CoreDispatcher Dispatcher { get; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set => SetProperty(ref title, value);
        }

        private FeedDetailModel feedDetail;
        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            protected set => SetProperty(ref feedDetail, value);
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set => SetProperty(ref itemSource, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        protected FeedShellViewModel(string id, CoreDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            Dispatcher = dispatcher;
            ID = id;
        }

        protected virtual async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = id.Contains("changeHistoryDetail") ? await RequestHelper.GetDataAsync(new Uri(UriHelper.BaseUri.ToString() + "v6/feed/" + id), true) : await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetFeedDetail, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new FeedDetailModel(detail) : null;
        }

        protected void OnLoadMoreStarted() => Dispatcher.ShowProgressBar();

        protected void OnLoadMoreCompleted() => Dispatcher.HideProgressBar();

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedShellViewModel model && IsEqual(model);
        public bool IsEqual(FeedShellViewModel other) => ID == other.ID;
    }

    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSource ReplyItemSource { get; private set; }
        public LikeItemSource LikeItemSource { get; private set; }
        public ShareItemSource ShareItemSource { get; private set; }

        public FeedDetailViewModel(string id, CoreDispatcher dispatcher) : base(id, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new ReplyItemSource(ID);
                    ReplyItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    ReplyItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "回复",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new LikeItemSource(ID);
                    LikeItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    LikeItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞",
                    ItemSource = LikeItemSource
                });
                if (ShareItemSource == null || ShareItemSource.ID != ID)
                {
                    ShareItemSource = new ShareItemSource(ID, FeedDetail.FeedType);
                    ShareItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    ShareItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "转发",
                    ItemSource = ShareItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

    public class QuestionViewModel : FeedShellViewModel
    {
        public QuestionItemSource ReplyItemSource { get; private set; }
        public QuestionItemSource LikeItemSource { get; private set; }
        public QuestionItemSource DatelineItemSource { get; private set; }

        public QuestionViewModel(string id, CoreDispatcher dispatcher) : base(id, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new QuestionItemSource(ID, "reply");
                    ReplyItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    ReplyItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "热度排序",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new QuestionItemSource(ID, "like");
                    LikeItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    LikeItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞排序",
                    ItemSource = LikeItemSource
                });
                if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                {
                    DatelineItemSource = new QuestionItemSource(ID, "dateline");
                    DatelineItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    DatelineItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "时间排序",
                    ItemSource = DatelineItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

    public class VoteViewModel : FeedShellViewModel
    {
        public VoteViewModel(string id, CoreDispatcher dispatcher) : base(id, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (FeedDetail.VoteType == 0)
                {
                    foreach (VoteItem vote in FeedDetail.VoteRows)
                    {
                        VoteItemSource VoteItemSource = new VoteItemSource(vote.ID.ToString(), vote.VoteID.ToString());
                        VoteItemSource.LoadMoreStarted += OnLoadMoreStarted;
                        VoteItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = vote.Title,
                            ItemSource = VoteItemSource
                        });
                    }
                }
                else
                {
                    VoteItemSource VoteItemSource = new VoteItemSource(string.Empty, FeedDetail.ID.ToString());
                    VoteItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    VoteItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "观点",
                        ItemSource = VoteItemSource
                    });
                    if (!string.IsNullOrEmpty(FeedDetail.VoteTag))
                    {
                        TagItemSource TagItemSource = new TagItemSource(FeedDetail.VoteTag);
                        TagItemSource.LoadMoreStarted += OnLoadMoreStarted;
                        TagItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "话题",
                            ItemSource = TagItemSource
                        });
                    }
                }
                base.ItemSource = ItemSource;
            }
            await (ItemSource.FirstOrDefault()?.ItemSource as EntityItemSource)?.Refresh(reset);
        }
    }

    public class ReplyItemSource : EntityItemSource, INotifyPropertyChanged, IComboBoxChangeSelectedIndex, IToggleChangeSelectedIndex
    {
        public string ID;
        public List<string> ItemSource { get; private set; }
        private readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedShellPage");

        private bool toggleIsOn;
        public bool ToggleIsOn
        {
            get => toggleIsOn;
            set
            {
                if (toggleIsOn != value)
                {
                    toggleIsOn = value;
                    SetProvider();
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string replyListType = "lastupdate_desc";
        public string ReplyListType
        {
            get => replyListType;
            set
            {
                if (replyListType != value)
                {
                    replyListType = value;
                    SetProvider();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int comboBoxSelectedIndex;
        public int ComboBoxSelectedIndex
        {
            get => comboBoxSelectedIndex;
            set
            {
                if (comboBoxSelectedIndex != value)
                {
                    comboBoxSelectedIndex = value;
                    SetComboBoxSelectedIndex(value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ReplyItemSource(string id)
        {
            ID = id;
            ItemSource = new List<string>()
            {
                loader.GetString("lastupdate_desc"),
                loader.GetString("dateline_desc"),
                loader.GetString("popular")
            };
            SetProvider();
        }

        private async void SetProvider()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetFeedReplies,
                    ID,
                    ReplyListType,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                    toggleIsOn ? 1 : 0),
                GetEntities,
                "id");
            await Refresh(true);
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return json.Value<string>("entityType") == "feed_reply" ? new FeedReplyModel(json) : (Entity)new NullEntity();
        }

        public void SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    ReplyListType = "lastupdate_desc";
                    break;

                case 1:
                    ReplyListType = "dateline_desc";
                    break;

                case 2:
                    ReplyListType = "popular";
                    break;
            }
        }
    }

    public class LikeItemSource : EntityItemSource
    {
        public string ID;

        public LikeItemSource(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetLikeList,
                    id,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new UserModel(json);
        }
    }

    public class ShareItemSource : EntityItemSource
    {
        public string ID;

        public ShareItemSource(string id, string feedType = "feed")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetShareList,
                    id,
                    feedType,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class QuestionItemSource : EntityItemSource
    {
        public string ID;

        public QuestionItemSource(string id, string answerSortType = "reply")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetAnswers,
                    id,
                    answerSortType,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class VoteItemSource : EntityItemSource
    {
        public string ID;

        public VoteItemSource(string id, string fid)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetVoteComments,
                    fid,
                    string.IsNullOrEmpty(id) ? string.Empty : $"&extra_key={id}",
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class TagItemSource : EntityItemSource
    {
        public string ID;

        public TagItemSource(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetTagFeeds,
                    id,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                    "lastupdate_desc"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }
}
