using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.DataSource
{
    /// <summary>
    /// Datasource base for Coolapk that enabled incremental loading (page based). <br/>
    /// Clone from <see cref="cnblogs UAP" href="https://github.com/MS-UAP/cnblogs-UAP"./>
    /// </summary>
    public abstract class DataSourceBase<T> : IncrementalLoadingBase<T>
    {
        /// <summary>
        /// The refresh will clear current items, and re-fetch from beginning, so that we will keep a correct page number.
        /// </summary>
        public virtual async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            await ClearAsync().ConfigureAwait(false);
            _ = await LoadMoreItemsAsync(20);
        }

        protected DateTime _lastTime = DateTime.MinValue;

        protected virtual bool IsInTime()
        {
            TimeSpan delta = DateTime.UtcNow - _lastTime;
            _lastTime = DateTime.UtcNow;
            return delta.TotalMilliseconds < 500;
        }

        /// <summary>
        /// Special for Coolapk, as their items are paged.
        /// </summary>
        protected override async Task<uint> LoadMoreItemsOverrideAsync(CancellationToken cancellationToken, uint count)
        {
            if (IsInTime())
            {
                return 0;
            }

            uint loaded = await LoadItemsAsync(count);

            // Network page state.
            if (loaded > 0)
            {
                _currentPage++;
            }

            _hasMoreItems = loaded > 0;

            return loaded;
        }

        protected async Task<uint> AddItemsAsync(IEnumerable<T> items)
        {
            uint count = 0;
            foreach (T item in items)
            {
                if (await AddItemAsync(item))
                {
                    count++;
                }
            }
            return count;
        }

        protected override bool HasMoreItemsOverride() => _hasMoreItems;

        protected abstract Task<uint> LoadItemsAsync(uint count);

        protected int _currentPage = 1;
        protected bool _hasMoreItems = true;
    }
}
