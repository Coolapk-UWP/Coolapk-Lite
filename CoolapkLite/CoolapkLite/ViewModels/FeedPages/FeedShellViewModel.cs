using CoolapkLite.Controls;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.DataSource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string ID;
        public double[] VerticalOffsets { get; set; } = new double[3];

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

        private FeedDetailModel feedDetail;
        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            protected set
            {
                feedDetail = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                itemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedShellViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            ID = id;
        }

        internal static async Task<FeedShellViewModel> GetViewModelAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            FeedDetailModel detail = await GetFeedDetailAsync(id);
            return detail != null ? (FeedShellViewModel)new FeedDetailViewModel(id) : null;
        }

        protected static async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = id.Contains("changeHistoryDetail") ? await Utils.GetDataAsync(new Uri(UriHelper.BaseUri.ToString() + "v6/feed/" + id), true) : await Utils.GetDataAsync(UriHelper.GetUri(UriType.GetFeedDetail, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new FeedDetailModel(detail) : null;
        }

        public abstract Task Refresh(int p = -1);
    }

    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSourse ReplyItemSourse { get; private set; }
        public LikeItemSourse LikeItemSourse { get; private set; }
        public ShareItemSourse ShareItemSourse { get; private set; }

        internal FeedDetailViewModel(string id) : base(id) { }

        public override async Task Refresh(int p = -1)
        {
            if (FeedDetail == null || p == 1)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSourse == null || ReplyItemSourse.ID != ID)
                {
                    ReplyItemSourse = new ReplyItemSourse(ID);
                    ItemSource.Add(new ShyHeaderItem()
                    {
                        Header = "回复",
                        ItemSource = ReplyItemSourse
                    });
                }
                if (LikeItemSourse == null || LikeItemSourse.ID != ID)
                {
                    LikeItemSourse = new LikeItemSourse(ID);
                    ItemSource.Add(new ShyHeaderItem()
                    {
                        Header = "点赞",
                        ItemSource = LikeItemSourse
                    });
                }
                if (ShareItemSourse == null || ShareItemSourse.ID != ID)
                {
                    ShareItemSourse = new ShareItemSourse(ID, FeedDetail.FeedType);
                    ItemSource.Add(new ShyHeaderItem()
                    {
                        Header = "转发",
                        ItemSource = ShareItemSourse
                    });
                }
                base.ItemSource = ItemSource;
            }
            await ReplyItemSourse?.Refresh(p);
        }
    }

    public class ReplyItemSourse : EntityItemSourse, INotifyPropertyChanged, ICanComboBoxChangeSelectedIndex, ICanToggleChangeSelectedIndex
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
                toggleIsOn = value;
                SetProvider();
                RaisePropertyChangedEvent();
            }
        }

        private string replyListType = "lastupdate_desc";
        public string ReplyListType
        {
            get => replyListType;
            set
            {
                replyListType = value;
                SetProvider();
                RaisePropertyChangedEvent();
            }
        }

        public int comboBoxSelectedIndex;
        public int ComboBoxSelectedIndex
        {
            get => comboBoxSelectedIndex;
            set
            {
                comboBoxSelectedIndex = value;
                SetComboBoxSelectedIndex(value);
                RaisePropertyChangedEvent();
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ReplyItemSourse(string id)
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
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty,
                    toggleIsOn ? 1 : 0),
                GetEntities,
                "id");
            await Refresh(-2);
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedReplyModel(jo);
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

    public class LikeItemSourse : EntityItemSourse
    {
        public string ID;

        public LikeItemSourse(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetLikeList,
                    id,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new UserModel(jo);
        }
    }

    public class ShareItemSourse : EntityItemSourse
    {
        public string ID;

        public ShareItemSourse(string id, string feedtype = "feed")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, _, __) =>
                UriHelper.GetUri(
                    UriType.GetShareList,
                    id,
                    feedtype,
                    p),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedModel(jo);
        }
    }
}
