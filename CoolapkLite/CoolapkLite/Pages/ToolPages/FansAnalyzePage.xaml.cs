using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.ToolPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.ToolPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FansAnalyzePage : Page
    {
        private FansAnalyzeViewModel Provider;

        public FansAnalyzePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("History");
            if (e.Parameter is FansAnalyzeViewModel ViewModel)
            {
                Provider = ViewModel;
                Provider.OnFanNumListByDateChanged += FanNumListByDateChanged;
                await Refresh(-2);
                ((LineSeries)this.LineChart.Series[0]).ItemsSource = Provider.FanNumListByDate;
                if (!string.IsNullOrEmpty(Provider.Title))
                {
                    TitleBar.Title = Provider.Title;
                }
            }
        }

        private async void FanNumListByDateChanged()
        {
            await Task.Delay(500);
            FrameworkElement AxisGrid = LineChart.FindDescendantByName("AxisGrid");
            if (AxisGrid != null)
            {
                IEnumerable<NumericAxisLabel> NumericAxisLabels = AxisGrid.FindDescendants<NumericAxisLabel>();
                if (NumericAxisLabels != null)
                {
                    foreach (NumericAxisLabel NumericAxisLabel in NumericAxisLabels)
                    {
                        TextBlock TextBlock = NumericAxisLabel.FindDescendant<TextBlock>();
                        if (TextBlock != null)
                        {
                            TextBlock.Text = Convert.ToDouble(TextBlock.Text).ConvertUnixTimeStampToReadable();
                        }
                    }
                }
            }
        }

        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);
    }
}
