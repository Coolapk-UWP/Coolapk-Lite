﻿using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.ToolPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

        private void OnArrived(object sender, EventArgs e)
        {
            AreaSeries.ItemsSource = Provider.FanNumListByDate;
        }

        private void FanNumListByDateChanged()
        {
            _delayer.Delay();
        }

        private void ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            Provider.FanNumListByDateTrack = e.Context;
        }


        private async Task Refresh(int p = -1) => await Provider.Refresh(p);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);
    }

    public class Datas
    {
        public DateTime Date { get; set; }

        public double Value { get; set; }
    }
}