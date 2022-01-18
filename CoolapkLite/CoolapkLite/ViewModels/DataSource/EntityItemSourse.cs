using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.DataSource
{
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

}
