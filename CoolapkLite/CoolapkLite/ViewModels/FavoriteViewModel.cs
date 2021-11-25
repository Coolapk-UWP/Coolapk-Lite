using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Helpers.DataSource;
using CoolapkLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels
{
    internal class FavoriteViewModel : DataSourceBase<Entity>, IViewModel
    {
        public string Title { get; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        internal FavoriteViewModel()
        {
            Provider = new CoolapkListProvider(
                (p, _, _) => UriHelper.GetUri(UriType.GetUserFollows, "apkFollowList", string.Empty, p),
                GetEntities,
                "entityId");
        }

        private readonly CoolapkListProvider Provider;

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new IndexPageModel(jo);
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
                Models = await Provider.GetEntity(Models, _currentPage);
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
