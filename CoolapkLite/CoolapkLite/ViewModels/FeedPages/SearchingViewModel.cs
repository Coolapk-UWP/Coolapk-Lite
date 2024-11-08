﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class SearchingViewModel : IViewModel
    {
        public int PivotIndex = -1;

        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private SearchFeedItemSource searchFeedItemSource;
        public SearchFeedItemSource SearchFeedItemSource
        {
            get => searchFeedItemSource;
            private set => SetProperty(ref searchFeedItemSource, value);
        }

        private SearchUserItemSource searchUserItemSource;
        public SearchUserItemSource SearchUserItemSource
        {
            get => searchUserItemSource;
            private set => SetProperty(ref searchUserItemSource, value);
        }

        private SearchTopicItemSource searchTopicItemSource;
        public SearchTopicItemSource SearchTopicItemSource
        {
            get => searchTopicItemSource;
            private set => SetProperty(ref searchTopicItemSource, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
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

        public SearchingViewModel(string keyword, CoreDispatcher dispatcher, int index = -1)
        {
            Dispatcher = dispatcher;
            Title = keyword;
            PivotIndex = index;
        }

        private void OnLoadMoreStarted() => _ = Dispatcher.ShowProgressBarAsync();

        private void OnLoadMoreCompleted() => _ = Dispatcher.HideProgressBarAsync();

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                if (SearchFeedItemSource == null)
                {
                    SearchFeedItemSource = new SearchFeedItemSource(Title);
                    SearchFeedItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    SearchFeedItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                else if (SearchFeedItemSource.Keyword != Title)
                {
                    SearchFeedItemSource.Keyword = Title;
                }
                if (SearchUserItemSource == null)
                {
                    SearchUserItemSource = new SearchUserItemSource(Title);
                    SearchUserItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    SearchUserItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                else if (SearchUserItemSource.Keyword != Title)
                {
                    SearchUserItemSource.Keyword = Title;
                }
                if (SearchTopicItemSource == null)
                {
                    SearchTopicItemSource = new SearchTopicItemSource(Title);
                    SearchTopicItemSource.LoadMoreStarted += OnLoadMoreStarted;
                    SearchTopicItemSource.LoadMoreCompleted += OnLoadMoreCompleted;
                }
                else if (SearchTopicItemSource.Keyword != Title)
                {
                    SearchTopicItemSource.Keyword = Title;
                }
            }
            if (SearchFeedItemSource != null) { await SearchFeedItemSource.Refresh(reset); }
            if (SearchUserItemSource != null) { await SearchUserItemSource.Refresh(reset); }
            if (SearchTopicItemSource != null) { await SearchTopicItemSource.Refresh(reset); }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is SearchingViewModel model && IsEqual(model);
        public bool IsEqual(SearchingViewModel other) => Title == other.Title;
    }

    public class SearchFeedItemSource : EntityItemSource, INotifyPropertyChanged
    {
        private string keyword;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider();
                }
            }
        }

        private int searchFeedTypeComboBoxSelectedIndex = 0;
        public int SearchFeedTypeComboBoxSelectedIndex
        {
            get => searchFeedTypeComboBoxSelectedIndex;
            set
            {
                if (searchFeedTypeComboBoxSelectedIndex != value)
                {
                    searchFeedTypeComboBoxSelectedIndex = value;
                    RaisePropertyChangedEvent();
                    UpdateProvider();
                    _ = Refresh(true);
                }
            }
        }

        private int searchFeedSortTypeComboBoxSelectedIndex = 0;
        public int SearchFeedSortTypeComboBoxSelectedIndex
        {
            get => searchFeedSortTypeComboBoxSelectedIndex;
            set
            {
                if (searchFeedTypeComboBoxSelectedIndex != value)
                {
                    searchFeedSortTypeComboBoxSelectedIndex = value;
                    RaisePropertyChangedEvent();
                    UpdateProvider();
                    _ = Refresh(true);
                }
            }
        }

        public SearchFeedItemSource(string keyword) => Keyword = keyword;

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
                    UriType.SearchFeeds,
                    feedType,
                    sortType,
                    Keyword,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedModel(jo);
        }
    }

    public class SearchUserItemSource : EntityItemSource
    {
        private string keyword;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider();
                }
            }
        }

        public SearchUserItemSource(string keyword) => Keyword = keyword;

        private void UpdateProvider()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.Search,
                    "user",
                    Keyword,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new UserModel(jo);
        }
    }

    public class SearchTopicItemSource : EntityItemSource
    {
        private string keyword;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider();
                }
            }
        }

        public SearchTopicItemSource(string keyword) => Keyword = keyword;

        private void UpdateProvider()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.Search,
                    "feedTopic",
                    Keyword,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new TopicModel(jo);
        }
    }
}
