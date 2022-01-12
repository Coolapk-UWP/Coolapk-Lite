using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.ViewModels.FeedPages
{
    internal class HistoryViewModel : DataSourceBase<Entity>, IViewModel
    {
        public string Title { get; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private readonly CoolapkListProvider Provider;
        private readonly UriType _type = UriType.CheckLoginInfo;

        internal HistoryViewModel(string title)
        {
            Title = ResourceLoader.GetForCurrentView("MainPage").GetString("History");
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }
            Title = title;
            switch (title)
            {
                case "我的常去":
                    _type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    _type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(_type, p, firstItem, lastItem),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return jo.Value<string>("entityType") == "history" ? new HistoryModel(jo) : null;
        }

        public async Task Refresh(int p = -1)
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
    }
}
