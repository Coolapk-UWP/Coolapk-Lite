﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.DataTemplates
{
    public sealed partial class IndexCardTemplates : ResourceDictionary
    {
        public IndexCardTemplates() => InitializeComponent();

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).Height = e.NewSize.Width / 3;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            FlipView view = sender as FlipView;
            view.Height = view.ActualWidth / 3;
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20)
            };
            timer.Tick += (_, __) =>
            {
                if (view.SelectedIndex != -1)
                {
                    if (view.SelectedIndex + 1 >= view.Items.Count)
                    {
                        while (view.SelectedIndex > 0)
                        {
                            view.SelectedIndex -= 1;
                        }
                    }
                    else
                    {
                        view.SelectedIndex += 1;
                    }
                }
            };
            view.Unloaded += (_, __) => timer.Stop();
            timer.Start();
        }
    }
}
