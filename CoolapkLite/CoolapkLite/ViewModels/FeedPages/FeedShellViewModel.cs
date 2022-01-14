using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string id;
        public string Title { get; protected set; } = string.Empty;
        public double[] VerticalOffsets { get; set; } = new double[3];

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
    }

    internal class FeedDetailViewModel : FeedShellViewModel
    {
        public AdaptiveViewModel ReplyViewModel { get; private set; }

        internal FeedDetailViewModel(string id) : base(id)
        {
        }

        public override async Task Refresh(int p = -1)
        {
            if (FeedDetail == null || p == 1)
            {
                FeedDetailModel feedDetail = await GetFeedDetailAsync(id);
                //Title = feedDetail.Title;
                if (ReplyViewModel == null)
                {
                    ReplyViewModel = new AdaptiveViewModel(new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetFeedReplies,
                            id,
                            "lastupdate_desc",
                            p,
                            $"&firstItem={firstItem}&lastItem={lastItem}",
                            0),
                        (o) => new Entity[] { new FeedReplyModel(o) },
                        "id"));
                }
                FeedDetail = feedDetail;
            }
            //await ReplyListVM?.Refresh(p);
        }
    }
}
