using CoolapkLite.Controls;
using CoolapkLite.ViewModels.DataSource;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        private readonly NewDS ItemsSource = new NewDS();

        public BlankPage()
        {
            InitializeComponent();
            List<ShyHeaderItem> ShyHeaderItemSource = new List<ShyHeaderItem>()
            {
                new ShyHeaderItem()
                {
                    Header = "Test1",
                    ItemSource = ItemsSource
                },
                new ShyHeaderItem()
                {
                    Header = "Test2",
                    ItemSource = new List<double>()
                    {
                        1,2,3,4,5,6
                    }
                },
            };
            ShyHeaderListView.ShyHeaderItemSource = ShyHeaderItemSource;
            _ = ItemsSource.LoadMoreItemsAsync(20);
        }
    }

    internal class NewDS : DataSourceBase<string>
    {
        private int _loadnum = 0;
        protected override async Task<IList<string>> LoadItemsAsync(uint count)
        {
            List<string> items = new List<string>();
            await Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    items.Add((i + _loadnum).ToString());
                    _loadnum += items.Count;
                }
            });
            return items;
        }

        protected override async Task AddItemsAsync(IList<string> items)
        {
            if (items != null)
            {
                foreach (string item in items)
                {
                    await AddAsync(item);
                }
            }
        }
    }
}
