using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Helpers.DataSource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.HistoryPage
{
    internal class ViewModel : DataSourceBase<Entity>, IViewModel
    {
        public string Title { get; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private readonly CoolapkListProvider Provider;
        private readonly UriType _type = UriType.CheckLoginInfo;

        internal ViewModel(string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }
            Title = title;
            switch (title)
            {
                case "我的常去":
                    _type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    _type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(_type, p, firstItem, lastItem),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
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
                    if (item is NullModel) { continue; }
                    Add(item);
                }
            }
        }
    }
}
