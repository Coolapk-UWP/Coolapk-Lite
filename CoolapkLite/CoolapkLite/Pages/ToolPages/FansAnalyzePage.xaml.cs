using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.ToolPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly EventDelayer _delayer = new EventDelayer();

        public FansAnalyzePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("History");
            if (e.Parameter is FansAnalyzeViewModel ViewModel)
            {
                Provider = ViewModel;
                _delayer.Arrived += OnArrived;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                Provider.OnLoadMoreProgressChanged += UIHelper.ShowProgressBar;
                Provider.OnFanNumListByDateChanged += FanNumListByDateChanged;
                await Refresh(-2);
                ((LineSeries)LineChart.Series[0]).ItemsSource = Provider.FanNumListByDate;
                if (!string.IsNullOrEmpty(Provider.Title))
                {
                    TitleBar.Title = Provider.Title;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _delayer.Arrived -= OnArrived;
            Provider.OnLoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.OnLoadMoreCompleted -= UIHelper.HideProgressBar;
            Provider.OnLoadMoreProgressChanged -= UIHelper.ShowProgressBar;
            Provider.OnFanNumListByDateChanged -= FanNumListByDateChanged;
        }

        private async void OnArrived(object sender, EventArgs e)
        {
            FrameworkElement AxisGrid = LineChart.FindDescendantByName("AxisGrid");
            while (AxisGrid == null)
            {
                await Task.Delay(10);
                AxisGrid = LineChart.FindDescendantByName("AxisGrid");
            }
            IEnumerable<NumericAxisLabel> NumericAxisLabels = AxisGrid?.FindDescendants<NumericAxisLabel>();
            if (NumericAxisLabels != null)
            {
                foreach (NumericAxisLabel NumericAxisLabel in NumericAxisLabels)
                {
                    TextBlock TextBlock = NumericAxisLabel?.FindDescendant<TextBlock>();
                    if (TextBlock != null)
                    {
                        if (NumericAxisLabel == NumericAxisLabels.First() || NumericAxisLabel == NumericAxisLabels.Last())
                        {
                            TextBlock.Text = Convert.ToDouble(TextBlock.Text).ConvertUnixTimeStampToReadable();
                        }
                        else
                        {
                            TextBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void FanNumListByDateChanged()
        {
            _delayer.Delay();
        }

        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);
    }
}
