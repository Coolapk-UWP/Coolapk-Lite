using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.DataSource;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.DataSource;
using CoolapkLite.Models.Feeds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.IndexPage
{
    internal class ViewModel : DataSourceBase<Entity>, IViewModel
    {
        private readonly string Uri;
        protected bool IsInitPage => Uri == "/main/init";
        protected bool IsIndexPage => !Uri.Contains("?");
        protected bool IsHotFeedPage => Uri == "/main/indexV8" || Uri == "/main/index";

        internal bool ShowTitleBar { get; }
        private readonly CoolapkListProvider Provider;

        public string Title { get; protected set; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        internal ViewModel(string uri, bool showTitleBar = true)
        {
            Uri = GetUri(uri);
            ShowTitleBar = showTitleBar;
            Provider = new CoolapkListProvider(
                (p, _, _) => UriHelper.GetUri(UriType.GetIndexPage, Uri, IsHotFeedPage ? "?" : "&", p),
                GetEntities,
                "entityId");
        }

        public async Task Refresh(int p = -1)
        {
            if (p == -2)
            {
                await Reset();
            }
            else if (p == -1)
            {
                _ = await LoadItemsAsync(20);
            }
        }

        private string GetUri(string uri)
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
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

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            if (jo.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
            {
                JObject j = JObject.Parse(jo.Value<string>("extraData"));
                Title = j.Value<string>("pageTitle");
                yield return null;
            }
            else if (jo.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JObject item in jo.Value<JArray>("entities"))
                {
                    Entity entity = EntityTemplateSelector.GetEntity(item, IsHotFeedPage);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(jo, IsHotFeedPage);
            }
            yield break;
        }

        protected async override Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                if (Models.Count > 0) { _currentPage++; }
                Models = await Provider.GetEntity(Models, _currentPage);
                if (Models.Count <= 0) { break; }
            }
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if(item is NullModel) { continue; }
                    Add(item);
                }
            }
        }
    }
}
