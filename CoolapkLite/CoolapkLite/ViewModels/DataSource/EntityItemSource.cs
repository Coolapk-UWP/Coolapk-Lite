﻿using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.DataSource
{
    public abstract class EntityItemSource : DataSourceBase<Entity>
    {
        protected CoolapkListProvider Provider;
        protected CoolapkListProvider SubProvider;

        protected static bool IsFullLoad => SettingsHelper.Get<bool>(SettingsHelper.IsFullLoad);

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            if (Provider != null)
            {
                if (IsFullLoad)
                {
                    while (Models.Count < count)
                    {
                        int temp = Models.Count;
                        if (Models.Count > 0) { _currentPage++; }
                        if (SubProvider == null)
                        {
                            await Provider.GetEntity(Models, Dispatcher, _currentPage).ConfigureAwait(false);
                        }
                        else
                        {
                            await SubProvider.GetEntity(Models, Dispatcher, _currentPage).ConfigureAwait(false);
                        }
                        if (Models.Count <= 0 || Models.Count <= temp) { break; }
                    }
                }
                else
                {
                    if (SubProvider == null)
                    {
                        await Provider.GetEntity(Models, Dispatcher, _currentPage).ConfigureAwait(false);
                    }
                    else
                    {
                        await SubProvider.GetEntity(Models, Dispatcher, _currentPage).ConfigureAwait(false);
                    }
                }
            }
            return Models;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (!(item is NullEntity))
                    {
                        await AddAsync(item).ConfigureAwait(false);
                        AddSubProvider(item);
                    }
                }
            }
        }

        public virtual async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset().ConfigureAwait(false);
            }
            else
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }

        public override async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            Clear();
            SubProvider = null;
            _ = await LoadMoreItemsAsync(20);
        }

        protected virtual void AddSubProvider(Entity item)
        {
            if (item is IndexPageHasEntitiesModel model
                && model.EntitiesType == EntityType.TabLink)
            {
                string Uri = GetUri((model.Entities.Where((x) => x is IndexPageModel).FirstOrDefault() as IndexPageModel).Url);
                SubProvider = new CoolapkListProvider(
                    (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, Uri.Contains("?") ? "&" : "?", p),
                    Provider.GetEntities,
                    "entityId");
                _currentPage = 1;
            }
        }

        private string GetUri(string uri)
        {
            if (uri.StartsWith("url="))
            {
                uri = uri.Replace("url=", string.Empty);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList"))
            {
                uri = uri.Replace("/page", "/page/dataList");
            }
            return uri.Replace("#", "%23");
        }
    }
}
