using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class HistoryViewModel : EntityItemSource, IViewModel
    {
        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("History");

        public HistoryViewModel(CoreDispatcher dispatcher) : base(dispatcher)
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserHistory, p, firstItem, lastItem),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return jo.Value<string>("entityType") == "history" ? new HistoryModel(jo) : null;
        }

        bool IViewModel.IsEqual(IViewModel other) => other is HistoryViewModel model && IsEqual(model);
        public bool IsEqual(HistoryViewModel other) => Title == other.Title;
    }
}
