using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.DataSource;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.DataSource;
using CoolapkLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.HistoryPage
{
    internal class ViewModel : DataSourceBase<Entity>, IViewModel
    {
        public string Title { get; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private readonly UriType _type = UriType.CheckLoginInfo;
        private string _firstItem, _lastItem;

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

        protected async override Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                if (Models.Count > 0)
                {
                    _currentPage++;
                }
                (bool isSucceed, JToken result) result = await Utils.GetDataAsync(UriHelper.GetUri(_type, _currentPage, _firstItem, _lastItem), false);
                if (result.isSucceed)
                {
                    JArray array = (JArray)result.result;
                    if (array.Count < 1) { break; }
                    if (string.IsNullOrEmpty(_firstItem))
                    {
                        _firstItem = Utils.GetId(array.First, "id");
                    }
                    _lastItem = Utils.GetId(array.Last, "id");
                    foreach (JObject item in array)
                    {
                        IEnumerable<Entity> entities = GetEntities(item);
                        if (entities == null) { continue; }

                        foreach (Entity i in entities)
                        {
                            if (i == null) { continue; }
                            Models.Add(i);
                        }
                    }
                }
                else
                {
                    break;
                }
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
