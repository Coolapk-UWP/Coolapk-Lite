using CoolapkLite.Controls;
using CoolapkLite.Core.Helpers.DataSource;
using CoolapkLite.Helpers.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        private NewDS ItemsSource = new NewDS();

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
        protected async override Task<IList<string>> LoadItemsAsync(uint count)
        {
            List<string> items = new();
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

        protected override void AddItems(IList<string> items)
        {
            if (items != null)
            {
                foreach (string item in items)
                {
                    Add(item);
                }
            }
        }
    }
}
