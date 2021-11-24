using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Helpers.DataSource;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.DataSource
{
    internal class HistoryDS : DataSourceBase<Entity>
    {
        private readonly CoolapkListProvider _provider;

        public async Task Refresh()
        {
            await Reset();
        }

        internal HistoryDS(CoolapkListProvider provider)
        {
            _provider = provider;
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            ObservableCollection<Entity> Models = new ObservableCollection<Entity>() /*= await _provider.GetEntity(_currentPage)*/;
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
