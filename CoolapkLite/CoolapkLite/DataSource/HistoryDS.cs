using CoolapkLite.Core.DataSource;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.DataSource
{
    internal class HistoryDS : DataSourceBase<Entity>
    {
        private CoolapkListProvider _provider;

        public async Task Refresh()
        {
            _provider.Clear();
            await Reset();
        }

        internal HistoryDS(CoolapkListProvider provider)
        {
            _provider = provider;
        }

        protected async override Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            ObservableCollection<Entity> Models = await _provider.GetEntity(_currentPage);
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (!Contains(item))
                    { Add(item); }
                }
            }
        }
    }
}
