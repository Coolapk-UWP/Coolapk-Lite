using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.DataSource;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.DataSource
{
    internal class IndexDS : DataSourceBase<Entity>
    {
        private CoolapkListProvider _provider;

        public async Task Refresh()
        {
            await Reset();
        }

        internal IndexDS(CoolapkListProvider provider)
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
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }
    }
}
