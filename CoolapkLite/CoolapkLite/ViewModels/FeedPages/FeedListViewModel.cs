﻿using CoolapkLite.Controls;
using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Pages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.ViewModels.FeedPages
{
    public abstract class FeedListViewModel : INotifyPropertyChanged, IViewModel
    {
        protected const string idName = "id";

        public string ID { get; }
        public FeedListType ListType { get; }
        public DataTemplateSelector DataTemplateSelector;
        public double[] VerticalOffsets { get; set; } = new double[1];

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                title = value;
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
        private FeedListDetailBase detail;
        public FeedListDetailBase Detail
        {
            get => detail;
            protected set
            {
                detail = value;
                RaisePropertyChangedEvent();
                Title = GetTitleBarText(value);
                DetailDataTemplate = DataTemplateSelector.SelectTemplate(value);
            }
        }

        private DataTemplate detailDataTemplate;
        public DataTemplate DetailDataTemplate
        {
            get => detailDataTemplate;
            protected set
            {
                detailDataTemplate = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListViewModel(string id, FeedListType type)
        {
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListViewModel GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id);
                case FeedListType.TagPageList: return new TagViewModel(id);
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

        public abstract Task<FeedListDetailBase> GetDetail();

        public abstract Task Refresh(bool reset = false);

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }

        public class UserViewModel : FeedListViewModel
        {
            public FeedListItemSourse FeedItemSourse { get; private set; }
            public FeedListItemSourse HtmlFeedItemSourse { get; private set; }
            public FeedListItemSourse QAItemSourse { get; private set; }

            internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (FeedItemSourse == null || FeedItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "动态",
                            ItemSource = FeedItemSourse
                        });
                    }
                    if (HtmlFeedItemSourse == null || HtmlFeedItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "htmlFeed"),
                            GetEntities,
                            idName);
                        HtmlFeedItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "图文",
                            ItemSource = HtmlFeedItemSourse
                        });
                    }
                    if (QAItemSourse == null || QAItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "questionAndAnswer"),
                            GetEntities,
                            idName);
                        QAItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "问答",
                            ItemSource = QAItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            public void Report()
            {
                UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=user&m=report&id={ID}" });
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail).UserName;

            public override async Task<FeedListDetailBase> GetDetail()
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
        }

        internal class TagViewModel : FeedListViewModel
        {
            public FeedListItemSourse LastupdateItemSourse { get; private set; }
            public FeedListItemSourse DatelineItemSourse { get; private set; }
            public FeedListItemSourse PopularItemSourse { get; private set; }

            internal TagViewModel(string uid) : base(uid, FeedListType.TagPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (LastupdateItemSourse == null || LastupdateItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "lastupdate_desc"),
                            GetEntities,
                            idName);
                        LastupdateItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "最近回复",
                            ItemSource = LastupdateItemSourse
                        });
                    }
                    if (DatelineItemSourse == null || DatelineItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "dateline_desc"),
                            GetEntities,
                            idName);
                        DatelineItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "最近发布",
                            ItemSource = DatelineItemSourse
                        });
                    }
                    if (PopularItemSourse == null || PopularItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "popular"),
                            GetEntities,
                            idName);
                        PopularItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem()
                        {
                            Header = "热门动态",
                            ItemSource = PopularItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as TopicDetail).Title;

            public override async Task<FeedListDetailBase> GetDetail()
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
        }
    }

    public class FeedListItemSourse : EntityItemSourse
    {
        public string ID;

        public FeedListItemSourse(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
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
    }
}
