using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class HistoryViewModel : DataSourceBase<Entity>, IViewModel
    {
        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("History");

        private readonly CoolapkListProvider Provider;

        public HistoryViewModel()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserHistory, p, firstItem, lastItem),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return jo.Value<string>("entityType") == "history" ? new HistoryModel(jo) : null;
        }

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset();
            }
            else
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is HistoryViewModel model && IsEqual(model);
        public bool IsEqual(HistoryViewModel other) => Title == other.Title;

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
}
