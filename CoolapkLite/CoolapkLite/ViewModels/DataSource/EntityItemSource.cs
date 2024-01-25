using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.DataSource
{
    public abstract class EntityItemSource : DataSourceBase<Entity>
    {
        protected CoolapkListProvider Provider;
        protected CoolapkListProvider SubProvider;

        protected bool IsFullLoad { get; } = SettingsHelper.Get<bool>(SettingsHelper.IsFullLoad);

        public EntityItemSource() => Dispatcher = UIHelper.TryGetForCurrentCoreDispatcher();

        public EntityItemSource(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        protected override async Task<uint> LoadItemsAsync(uint count)
        {
            if (Provider != null)
            {
                if (IsFullLoad)
                {
                    uint loaded = 0;
                    while (loaded < count)
                    {
                        uint temp = loaded;
                        if (loaded > 0) { _currentPage++; }
                        if (SubProvider == null)
                        {
                            loaded += await Provider.GetEntityAsync(this, _currentPage).ConfigureAwait(false);
                        }
                        else
                        {
                            loaded += await SubProvider.GetEntityAsync(this, _currentPage).ConfigureAwait(false);
                        }
                        if (loaded <= 0 || loaded <= temp) { return loaded; }
                    }
                    return loaded;
                }
                else
                {
                    return SubProvider == null
                        ? await Provider.GetEntityAsync(this, _currentPage).ConfigureAwait(false)
                        : await SubProvider.GetEntityAsync(this, _currentPage).ConfigureAwait(false);
                }
            }
            return 0;
        }

        public override async Task<bool> AddItemAsync(Entity item)
        {
            if (item != null && !(item is NullEntity))
            {
                await AddAsync(item);
                AddSubProvider(item);
                return true;
            }
            return false;
        }

        public virtual async Task Refresh(bool reset = false)
        {
            if (_busy) { return; }
            if (reset)
            {
                await Reset().ConfigureAwait(false);
            }
            else if (_hasMoreItems)
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }

        public override async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            await ClearAsync().ConfigureAwait(false);
            SubProvider = null;
            _ = await LoadMoreItemsAsync(20);
        }

        protected virtual void AddSubProvider(Entity item)
        {
            if (item is IndexPageHasEntitiesModel model
                && model.EntitiesType == EntityType.TabLink)
            {
                string Uri = GetUri((model.Entities.Where(x => x is IndexPageModel).FirstOrDefault() as IndexPageModel).Url);
                SubProvider = new CoolapkListProvider(
                    (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetIndexPage, Uri, Uri.Contains('?') ? "&" : "?", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    Provider.GetEntities,
                    "entityId");
                _currentPage = 1;
            }
        }

        private string GetUri(string uri)
        {
            if (uri.StartsWith("url=", StringComparison.OrdinalIgnoreCase))
            {
                uri = uri.Substring(4);
            }

            int index = uri.IndexOf("/page", StringComparison.OrdinalIgnoreCase);
            if (index == 0 && !uri.Contains("/page/dataList", StringComparison.OrdinalIgnoreCase))
            {
                uri = uri.Insert(5, "/dataList");
            }
            else if (index != -1 && uri.StartsWith("#"))
            {
                uri = $"/page/dataList?url={uri}";
            }
            else if (!uri.ContainsAny(new[] { "/main/", "/user/", "/apk/", "/appForum/", "/picture/", "/topic/", "/discovery/" }, StringComparison.OrdinalIgnoreCase))
            {
                uri = $"/page/dataList?url={uri}";
            }

            return uri.Replace("#", "%23");
        }
    }
}
