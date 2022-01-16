using CoolapkLite.Controls;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string id;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedShellViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            this.id = id;
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
        public ObservableCollection<ShyHeaderItem> ShyHeaderItemSource { get; set; }
    }

    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSourse ReplyItemSourse { get; private set; }

        internal FeedDetailViewModel(string id) : base(id)
        {
            ShyHeaderItemSource = new ObservableCollection<ShyHeaderItem>()
            {
                new ShyHeaderItem() { Header = "回复", ItemSource = ReplyItemSourse },
                new ShyHeaderItem() { Header = "点赞", },
                new ShyHeaderItem() { Header = "转发", },
            };
        }

        public override async Task Refresh(int p = -1)
        {
            if (FeedDetail == null || p == 1)
            {
                FeedDetailModel feedDetail = await GetFeedDetailAsync(id);
                Title = feedDetail.Title;
                if (ReplyItemSourse == null)
                {
                    ReplyItemSourse = new ReplyItemSourse(id);
                }
                FeedDetail = feedDetail;
            }
            await ReplyItemSourse?.Refresh(p);
        }
    }

    public abstract class EntityItemSourse : DataSourceBase<Entity>
    {
        protected CoolapkListProvider Provider;

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                await Provider.GetEntity(Models, _currentPage);
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullModel) { continue; }
                    Add(item);
                }
            }
        }

        public virtual async Task Refresh(int p = -1)
        {
            if (p == -2)
            {
                await Reset();
            }
            else if (p == -1)
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }
    }

    public class ReplyItemSourse : EntityItemSourse
    {
        public string ID;

        public ReplyItemSourse(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetFeedReplies,
                            id,
                            "lastupdate_desc",
                            p,
                            $"&firstItem={firstItem}&lastItem={lastItem}",
                            0),
                        GetEntities,
                        "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedReplyModel(jo);
        }
    }
}
