using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.ToolPages
{
    internal class FansAnalyzeViewModel : IViewModel
    {
        public string Title { get; protected set; }
        public ObservableCollection<Entity> FanList { get; set; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private readonly CoolapkListProvider Provider;

        internal FansAnalyzeViewModel(string id)
        {
            FanList = new ObservableCollection<Entity>();
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFollows, "fansList", id, p),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return jo.Value<string>("entityType") == "contacts" ? new UserModel(jo) : null;
        }

        public async Task Refresh(int p)
        {
            await GetFanList();
        }

        private async Task GetFanList()
        {
            int page = 1;
            FanList.Clear();
            while (true)
            {
                int temp = FanList.Count;
                await Provider.GetEntity(FanList, page);
                if (FanList.Count <= 0 || FanList.Count <= temp) { break; }
                page++;
            }
        }
    }
}
