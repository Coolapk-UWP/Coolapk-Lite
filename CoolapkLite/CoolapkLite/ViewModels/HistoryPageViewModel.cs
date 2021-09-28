using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.DataSource;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.HistoryPage
{
    internal class ViewModel : IViewModel
    {
        internal HistoryDS DataSource;

        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; }

        internal ViewModel(string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }

            Title = title;
            UriType type = UriType.CheckLoginInfo;

            switch (title)
            {
                case "我的常去":
                    type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            CoolapkListProvider provider;
            provider =
                    new CoolapkListProvider(
                        (p, page, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                type,
                                p < 0 ? ++page : p,
                                string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                        (o) => new Entity[] { new HistoryModel(o) },
                        "id");

            DataSource = new HistoryDS(provider);
            DataSource.OnLoadMoreStarted += UIHelper.ShowProgressBar;
            DataSource.OnLoadMoreCompleted += UIHelper.HideProgressBar;
        }

        public async Task Refresh(int p = -1)
        {
            if (p == -2)
            {
                await DataSource.Refresh();
            }
            else if (p == -1)
            {
                _ = await DataSource.LoadMoreItemsAsync(20);
            }
        }
    }
}
