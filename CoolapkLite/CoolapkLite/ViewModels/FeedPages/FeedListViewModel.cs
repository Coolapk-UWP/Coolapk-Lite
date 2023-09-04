using CoolapkLite.BackgroundTasks;
using CoolapkLite.Common;
using CoolapkLite.Controls;
using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedListViewModel : IViewModel
    {
        protected const string idName = "id";

        public string ID { get; }
        private FeedListType ListType { get; }
        public DataTemplateSelector DataTemplateSelector;

        public CoreDispatcher Dispatcher { get; }

        private string title;
        public string Title
        {
            get => title;
            protected set => SetProperty(ref title, value);
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set => SetProperty(ref itemSource, value);
        }

        private FeedListDetailBase detail;
        public FeedListDetailBase Detail
        {
            get => detail;
            protected set
            {
                detail = value;
                RaisePropertyChangedEvent();
                Title = GetTitleBarText(value);
                DetailDataTemplate = DataTemplateSelector?.SelectTemplate(value);
            }
        }

        private SearchItemSource searchItemSource;
        public SearchItemSource SearchItemSource
        {
            get => searchItemSource;
            protected set => SetProperty(ref searchItemSource, value);
        }

        private DataTemplate detailDataTemplate;
        public DataTemplate DetailDataTemplate
        {
            get => detailDataTemplate;
            protected set => SetProperty(ref detailDataTemplate, value);
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

        protected FeedListViewModel(string id, FeedListType type, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListViewModel GetProvider(FeedListType type, string id, CoreDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id, dispatcher);
                case FeedListType.TagPageList: return new TagViewModel(id, dispatcher);
                case FeedListType.DyhPageList: return new DyhViewModel(id, dispatcher);
                case FeedListType.ProductPageList: return new ProductViewModel(id, dispatcher);
                case FeedListType.CollectionPageList: return new CollectionViewModel(id, dispatcher);
                default: return null;
            }
        }

        public static SearchItemSource GetSearchProvider(FeedListType type, string keyword, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new SearchItemSource(keyword, "user", id);
                case FeedListType.TagPageList: return new SearchItemSource(keyword, "tag", id);
                case FeedListType.DyhPageList: return new SearchItemSource(keyword, "dyh", id);
                case FeedListType.ProductPageList: return new SearchItemSource(keyword, "product_phone", id);
                case FeedListType.CollectionPageList: return new SearchItemSource(keyword, "collection", id);
                default: return null;
            }
        }

        public void ChangeCopyMode(bool mode)
        {
            if (Detail != null)
            {
                Detail.IsCopyEnabled = mode;
            }
        }

        public virtual async Task SearchQuerySubmittedAsync(string keyword)
        {
            if (SearchItemSource == null)
            {
                SearchItemSource = GetSearchProvider(ListType, keyword, ID);
                SearchItemSource.LoadMoreStarted += OnLoadMoreStarted;
                SearchItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
            }
            else if (SearchItemSource.Keyword != Title)
            {
                SearchItemSource.Keyword = Title;
            }
            await SearchItemSource?.Refresh(true);
        }

        private void OnLoadMoreStarted() => Dispatcher.ShowProgressBar();

        private void OnLoadMoreCompleted() => Dispatcher.HideProgressBar();

        public virtual Task SearchRefresh(bool reset = false) => SearchItemSource?.Refresh(reset);

        public abstract Task<bool> PinSecondaryTileAsync(Entity entity);

        public abstract Task<FeedListDetailBase> GetDetailAsync();

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedListViewModel model && IsEqual(model);
        public bool IsEqual(FeedListViewModel other) => ListType == other.ListType && ID == other.ID;

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

    }

    public class UserViewModel : FeedListViewModel
    {
        public FeedListItemSource FeedItemSource { get; private set; }
        public FeedListItemSource HtmlFeedItemSource { get; private set; }
        public FeedListItemSource QAItemSource { get; private set; }
        public FeedListItemSource CollectionItemSource { get; private set; }

        public UserViewModel(string uid, CoreDispatcher dispatcher) : base(uid, FeedListType.UserPageList, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync();
            }
            if (ItemSource == null)
            {
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                if (FeedItemSource == null || FeedItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    FeedItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "动态",
                        ItemSource = FeedItemSource
                    });
                }
                if (HtmlFeedItemSource == null || HtmlFeedItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "htmlFeed"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    HtmlFeedItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "图文",
                        ItemSource = HtmlFeedItemSource
                    });
                }
                if (QAItemSource == null || QAItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "questionAndAnswer"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    QAItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "问答",
                        ItemSource = QAItemSource
                    });
                }
                if (CollectionItemSource == null || CollectionItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionList, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    CollectionItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "收藏单",
                        ItemSource = CollectionItemSource
                    });
                }
                base.ItemSource = ItemSource;
            }
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail)?.UserName;

        public override async Task<FeedListDetailBase> GetDetailAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserSpace, ID), true);
            if (!isSucceed) { return null; }

            JObject token = (JObject)result;
            FeedListDetailBase detail = null;

            if (token != null)
            {
                detail = new UserDetail(token);
            }

            return detail;
        }

        public override async Task<bool> PinSecondaryTileAsync(Entity entity)
        {
            IUserModel user = (IUserModel)entity;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = user.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTileAsync(tileId, user.UserName, user.Url);
            if (isPinned)
            {
                try
                {
                    LiveTileTask.UpdateTile(tileId, LiveTileTask.GetUserTile(user));
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(FeedShellDetailControl)).Error(ex.ExceptionToMessage(), ex);
                }

                return isPinned;
            }

            Dispatcher.ShowMessage(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }
    }

    public class TagViewModel : FeedListViewModel
    {
        public FeedListItemSource LastUpdateItemSource { get; private set; }
        public FeedListItemSource DatelineItemSource { get; private set; }
        public FeedListItemSource PopularItemSource { get; private set; }

        public TagViewModel(string id, CoreDispatcher dispatcher) : base(id, FeedListType.TagPageList, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync();
            }
            if (ItemSource == null)
            {
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                if (LastUpdateItemSource == null || LastUpdateItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "lastupdate_desc"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    LastUpdateItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "最近回复",
                        ItemSource = LastUpdateItemSource
                    });
                }
                if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "dateline_desc"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    DatelineItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "最近发布",
                        ItemSource = DatelineItemSource
                    });
                }
                if (PopularItemSource == null || PopularItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "popular"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    PopularItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "热门动态",
                        ItemSource = PopularItemSource
                    });
                }
                base.ItemSource = ItemSource;
            }
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as TopicDetail)?.Title;

        public override async Task<FeedListDetailBase> GetDetailAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetTagDetail, ID), true);
            if (!isSucceed) { return null; }

            JObject token = (JObject)result;
            FeedListDetailBase detail = null;

            if (token != null)
            {
                detail = new TopicDetail(token);
            }

            return detail;
        }

        public override async Task<bool> PinSecondaryTileAsync(Entity entity)
        {
            IHasSubtitle detail = (IHasSubtitle)entity;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = detail.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTileAsync(tileId, detail.Title, detail.Url);
            if (isPinned)
            {
                try
                {
                    LiveTileTask.UpdateTile(tileId, LiveTileTask.GetListTile(detail));
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(FeedShellDetailControl)).Error(ex.ExceptionToMessage(), ex);
                }

                return isPinned;
            }

            Dispatcher.ShowMessage(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }
    }

    public class DyhViewModel : FeedListViewModel
    {
        public FeedListItemSource AllItemSource { get; private set; }
        public FeedListItemSource SquareItemSource { get; private set; }

        public DyhViewModel(string id, CoreDispatcher dispatcher) : base(id, FeedListType.DyhPageList, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync();
            }
            if (ItemSource == null)
            {
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                if (AllItemSource == null || AllItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetDyhFeeds, ID, "all", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    AllItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "精选",
                        ItemSource = AllItemSource
                    });
                }
                if (SquareItemSource == null || SquareItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, "square", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    SquareItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "广场",
                        ItemSource = SquareItemSource
                    });
                }
                base.ItemSource = ItemSource;
            }
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as DyhDetail)?.Title;

        public override async Task<FeedListDetailBase> GetDetailAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetDyhDetail, ID), true);
            if (!isSucceed) { return null; }

            JObject token = (JObject)result;
            FeedListDetailBase detail = null;

            if (token != null)
            {
                detail = new DyhDetail(token);
            }

            return detail;
        }

        public override async Task<bool> PinSecondaryTileAsync(Entity entity)
        {
            IHasDescription detail = (IHasDescription)entity;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = detail.Url.GetMD5();

            bool isPinned = await LiveTileTask.PinSecondaryTileAsync(tileId, detail.Title, detail.Url);
            if (isPinned)
            {
                try
                {
                    LiveTileTask.UpdateTile(tileId, LiveTileTask.GetListTile(detail));
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(FeedShellDetailControl)).Error(ex.ExceptionToMessage(), ex);
                }

                return isPinned;
            }

            Dispatcher.ShowMessage(loader.GetString("PinnedTileFailed"));
            return isPinned;
        }
    }

    public class ProductViewModel : FeedListViewModel
    {
        public FeedListItemSource FeedItemSource { get; private set; }
        public FeedListItemSource AnswerItemSource { get; private set; }
        public FeedListItemSource ArticleItemSource { get; private set; }
        public FeedListItemSource VideoItemSource { get; private set; }
        public FeedListItemSource TradeItemSource { get; private set; }

        public ProductViewModel(string id, CoreDispatcher dispatcher) : base(id, FeedListType.ProductPageList, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync();
            }
            if (ItemSource == null)
            {
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                if (FeedItemSource == null || FeedItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    FeedItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "讨论",
                        ItemSource = FeedItemSource
                    });
                }
                if (AnswerItemSource == null || AnswerItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "answer"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    AnswerItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "问答",
                        ItemSource = AnswerItemSource
                    });
                }
                if (ArticleItemSource == null || ArticleItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "article"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    ArticleItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "图文",
                        ItemSource = ArticleItemSource
                    });
                }
                if (VideoItemSource == null || VideoItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "video"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    VideoItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "视频",
                        ItemSource = VideoItemSource
                    });
                }
                if (TradeItemSource == null || TradeItemSource.ID != ID)
                {
                    CoolapkListProvider Provider = new CoolapkListProvider(
                        (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "trade"),
                        EntityTemplateSelector.GetEntities,
                        idName);
                    TradeItemSource = new FeedListItemSource(ID, Provider);
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "交易",
                        ItemSource = TradeItemSource
                    });
                }
                base.ItemSource = ItemSource;
            }
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as ProductDetail)?.Title;

        public override async Task<FeedListDetailBase> GetDetailAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetail, ID), true);
            if (!isSucceed) { return null; }

            JObject token = (JObject)result;
            FeedListDetailBase detail = null;

            if (token != null)
            {
                detail = new ProductDetail(token);
            }

            return detail;
        }

        public override Task<bool> PinSecondaryTileAsync(Entity entity) => Task.Run(() => false);
    }

    public class CollectionViewModel : FeedListViewModel
    {
        public CollectionViewModel(string id, CoreDispatcher dispatcher) : base(id, FeedListType.CollectionPageList, dispatcher) { }

        public override async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync();
            }
            if (ItemSource == null)
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionContents, ID, "1", ""), true);
                if (isSucceed)
                {
                    JArray array = (JArray)result;
                    foreach (JObject item in array)
                    {
                        if (item.TryGetValue("entityTemplate", out JToken entityTemplate) && entityTemplate.ToString() == "selectorLinkCard")
                        {
                            if (item.TryGetValue("entities", out JToken v1))
                            {
                                JArray entities = (JArray)v1;
                                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                                foreach (JObject entity in entities)
                                {
                                    if (entity.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
                                    {
                                        CoolapkListProvider Provider = new CoolapkListProvider(
                                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.DataList, url.ToString().Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"), $"&page={p}" + (string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}") + (string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")),
                                            EntityTemplateSelector.GetEntities,
                                            "id");
                                        FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                                        ShyHeaderItem ShyHeaderItem = new ShyHeaderItem { ItemSource = FeedListItemSource };
                                        if (entity.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
                                        {
                                            ShyHeaderItem.Header = title.ToString();
                                        }
                                        ItemSource.Add(ShyHeaderItem);
                                    }
                                }
                                this.ItemSource = ItemSource;
                                break;
                            }
                        }
                    }
                    if (ItemSource == null)
                    {
                        List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionContents, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            EntityTemplateSelector.GetEntities,
                            "id");
                        FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                        ShyHeaderItem ShyHeaderItem = new ShyHeaderItem
                        {
                            ItemSource = FeedListItemSource,
                            Header = Detail is CollectionDetail CollectionDetail && CollectionDetail.ItemNum > 0 ? $"全部({CollectionDetail.ItemNum})" : (object)$"全部"
                        };
                        ItemSource.Add(ShyHeaderItem);
                        this.ItemSource = ItemSource;
                    }
                }
            }
        }
        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as CollectionDetail)?.Title;

        public override async Task<FeedListDetailBase> GetDetailAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionDetail, ID), true);
            if (!isSucceed) { return null; }

            JObject token = (JObject)result;
            FeedListDetailBase detail = null;

            if (token != null)
            {
                detail = new CollectionDetail(token);
            }

            return detail;
        }

        public override Task<bool> PinSecondaryTileAsync(Entity entity) => Task.Run(() => false);
    }

    public class FeedListItemSource : EntityItemSource
    {
        public string ID;

        public FeedListItemSource(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    await AddAsync(item);
                }
            }
        }
    }

    public class SearchItemSource : EntityItemSource, INotifyPropertyChanged
    {
        public string Keyword;
        public string PageType;
        public string PageParam;

        private int searchFeedTypeComboBoxSelectedIndex = 0;
        public int SearchFeedTypeComboBoxSelectedIndex
        {
            get => searchFeedTypeComboBoxSelectedIndex;
            set
            {
                searchFeedTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        private int searchFeedSortTypeComboBoxSelectedIndex = 0;
        public int SearchFeedSortTypeComboBoxSelectedIndex
        {
            get => searchFeedSortTypeComboBoxSelectedIndex;
            set
            {
                searchFeedSortTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        public SearchItemSource(string keyword, string pageType, string pageParam)
        {
            Keyword = keyword;
            PageType = pageType;
            PageParam = pageParam;
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBoxSelectedIndex)
            {
                case 0: feedType = "all"; break;
                case 1: feedType = "feed"; break;
                case 2: feedType = "feedArticle"; break;
                case 3: feedType = "rating"; break;
                case 4: feedType = "picture"; break;
                case 5: feedType = "question"; break;
                case 6: feedType = "answer"; break;
                case 7: feedType = "video"; break;
                case 8: feedType = "ershou"; break;
                case 9: feedType = "vote"; break;
            }
            switch (SearchFeedSortTypeComboBoxSelectedIndex)
            {
                case 0: sortType = "default"; break;
                case 1: sortType = "hot"; break;
                case 2: sortType = "reply"; break;
            }
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchTarget,
                    "feed",
                    feedType,
                    sortType,
                    keyword,
                    pageType,
                    pageParam,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedModel(jo);
        }

        private void UpdateProvider()
        {
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBoxSelectedIndex)
            {
                case 0: feedType = "all"; break;
                case 1: feedType = "feed"; break;
                case 2: feedType = "feedArticle"; break;
                case 3: feedType = "rating"; break;
                case 4: feedType = "picture"; break;
                case 5: feedType = "question"; break;
                case 6: feedType = "answer"; break;
                case 7: feedType = "video"; break;
                case 8: feedType = "ershou"; break;
                case 9: feedType = "vote"; break;
            }
            switch (SearchFeedSortTypeComboBoxSelectedIndex)
            {
                case 0: sortType = "default"; break;
                case 1: sortType = "hot"; break;
                case 2: sortType = "reply"; break;
            }
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchTarget,
                    "feed",
                    feedType,
                    sortType,
                    Keyword,
                    PageType,
                    PageParam,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }
    }
}
