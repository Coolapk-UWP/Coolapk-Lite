//using CoolapkLite.Core.Helpers;
//using CoolapkLite.Core.Models;
//using CoolapkLite.Helpers;
//using CoolapkLite.Helpers.DataSource;
//using CoolapkLite.Models.Pages;
//using CoolapkLite.Pages.FeedPages;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CoolapkLite.ViewModels
//{
//    internal abstract class FeedListPageViewModelBase : DataSourceBase<Entity>, IViewModel
//    {
//        protected const string idName = "id";
//        private string _firstItem, _lastItem;

//        public string Id { get; }
//        public FeedListType ListType { get; }
//        public double[] VerticalOffsets { get; set; } = new double[1];

//        private string title;
//        public string Title
//        {
//            get => title;
//            protected set
//            {
//                title = value;
//                TitleUpdate?.Invoke(this, null);
//            }
//        }

//        public event EventHandler TitleUpdate;

//        protected FeedListPageViewModelBase(string id, FeedListType type)
//        {
//            Id = string.IsNullOrEmpty(id)
//                ? throw new ArgumentException(nameof(id))
//                : id;
//            ListType = type;
//        }

//        public static FeedListPageViewModelBase GetProvider(FeedListType type, string id)
//        {
//            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
//            switch (type)
//            {
//                //case FeedListType.TagPageList: return new TagViewModel(id);
//                //case FeedListType.DyhPageList: return new DyhViewModel(id);
//                //case FeedListType.AppPageList: return new AppViewModel(id);
//                //case FeedListType.UserPageList: return new UserViewModel(id);
//                //case FeedListType.DevicePageList: return new DeviceViewModel(id);
//                //case FeedListType.ProductPageList: return new ProductViewModel(id);
//                //case FeedListType.CollectionPageList: return new CollectionViewModel(id);
//                default: return null;
//            }
//        }

//        public void ChangeCopyMode(bool mode)
//        {
//            //if (Models.Count == 0) { return; }
//            //if (Models[0] is FeedListDetailBase detail)
//            //{
//            //    detail.IsCopyEnabled = mode;
//            //}
//        }

//        private async Task<FeedListDetailBase> GetDetail()
//        {
//            UriType type;
//            switch (ListType)
//            {
//                case FeedListType.AppPageList:
//                    type = UriType.GetAppDetail;
//                    break;

//                case FeedListType.TagPageList:
//                    type = UriType.GetTagDetail;
//                    break;

//                case FeedListType.DyhPageList:
//                    type = UriType.GetDyhDetail;
//                    break;

//                case FeedListType.UserPageList:
//                    type = UriType.GetUserSpace;
//                    break;

//                case FeedListType.DevicePageList:
//                    type = UriType.GetTopicDetail;
//                    break;

//                case FeedListType.ProductPageList:
//                    type = UriType.GetProductDetail;
//                    break;

//                case FeedListType.CollectionPageList:
//                    type = UriType.GetCollectionDetail;
//                    break;

//                default:
//                    throw new ArgumentException($"{typeof(FeedListType).FullName}值错误");
//            }

//            (bool isSucceed, JToken result) = await Utils.GetDataAsync(UriHelper.GetUri(type, Id), true);
//            if (!isSucceed) { return null; }

//            JObject o = (JObject)result;
//            FeedListDetailBase d = null;

//            if (o != null)
//            {
//                switch (ListType)
//                {
//                    //case FeedListType.AppPageList:
//                    //    d = new AppDetail(o);
//                    //    break;

//                    //case FeedListType.TagPageList:
//                    //    d = new TopicDetail(o);
//                    //    break;

//                    //case FeedListType.DyhPageList:
//                    //    d = new DyhDetail(o);
//                    //    break;

//                    case FeedListType.UserPageList:
//                        d = new UserDetail(o);
//                        break;

//                        //case FeedListType.DevicePageList:
//                        //    d = new TopicDetail(o);
//                        //    break;

//                        //case FeedListType.ProductPageList:
//                        //    d = new ProductDetail(o);
//                        //    break;

//                        //case FeedListType.CollectionPageList:
//                        //    d = new CollectionDetail(o);
//                        //    break;
//                }
//            }
//            return d;
//        }

//        protected abstract string GetTitleBarText(FeedListDetailBase detail);
//        public abstract Task Refresh(int p);

//        //public async Task Refresh(int p)
//        //{
//        //    await Provider.Refresh(p);

//        //    ICanComboBoxChangeSelectedIndex it = null;
//        //    if (Models.Count > 0)
//        //    {
//        //        it = Models[0] as ICanComboBoxChangeSelectedIndex;
//        //        Models.RemoveAt(0);
//        //    }
//        //    FeedListDetailBase item = await GetDetail();
//        //    Title = GetTitleBarText(item);
//        //    if (it != null)
//        //    {
//        //        await (item as ICanComboBoxChangeSelectedIndex).SetComboBoxSelectedIndex(it.ComboBoxSelectedIndex);
//        //    }
//        //    Models.Insert(0, item);
//        //}

//        internal class UserViewModel : FeedListPageViewModelBase, ICanComboBoxChangeSelectedIndex
//        {
//            public int ComboBoxSelectedIndex { get; private set; }

//            private string sortType = "feed";

//            public override Task Refresh(int p)
//            {
//                throw new NotImplementedException();
//            }

//            internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList)
//            {

//            }

//            protected async override Task<IList<Entity>> LoadItemsAsync(uint count)
//            {
//                List<Entity> Models = new List<Entity>();
//                while (Models.Count < count)
//                {
//                    if (Models.Count > 0)
//                    {
//                        _currentPage++;
//                    }
//                    (bool isSucceed, JToken result) result = await Utils.GetDataAsync(UriHelper.GetUri(UriType.GetUserFeeds, Id,_currentPage, _firstItem, _lastItem,sortType), false);
//                    if (result.isSucceed)
//                    {
//                        JArray array = (JArray)result.result;
//                        if (array.Count < 1) { break; }
//                        if (string.IsNullOrEmpty(_firstItem))
//                        {
//                            _firstItem = Utils.GetId(array.First, "id");
//                        }
//                        _lastItem = Utils.GetId(array.Last, "id");
//                        foreach (JObject item in array)
//                        {
//                            IEnumerable<Entity> entities = GetEntities(item);
//                            if (entities == null) { continue; }

//                            foreach (Entity i in entities)
//                            {
//                                if (i == null) { continue; }
//                                Models.Add(i);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }
//                return Models;
//            }

//            protected override void AddItems(IList<Entity> items)
//            {
//                if (items != null)
//                {
//                    foreach (Entity item in items)
//                    {
//                        if (item is NullModel) { continue; }
//                        Add(item);
//                    }
//                }
//            }

//            public void Report()
//            {
//                UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=user&m=report&id={Id}" });
//            }

//            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail).UserName;

//            public async Task SetComboBoxSelectedIndex(int value)
//            {
//                switch (value)
//                {
//                    case -1: return;
//                    case 0:
//                        sortType = "feed";
//                        break;

//                    case 1:
//                        sortType = "htmlFeed";
//                        break;

//                    case 2:
//                        sortType = "questionAndAnswer";
//                        break;
//                }
//                ComboBoxSelectedIndex = value;
//            }
//        }
//    }
//}
