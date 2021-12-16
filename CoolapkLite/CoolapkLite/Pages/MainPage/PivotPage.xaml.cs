﻿using CoolapkLite.Helpers;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.ViewModels.FeedPages;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PivotPage : Page, IHaveTitleBar
    {
        private Thickness PivotTitleMargin => UIHelper.PivotTitleMargin;

        public PivotPage()
        {
            InitializeComponent();
            UIHelper.AppTitle = this;
            AppTitle.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
            CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
            TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            Window.Current.SetTitleBar(CustomTitleBar);
            TitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(TitleBar);
            UIHelper.CheckTheme();
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.ItemsSource = MenuItem.GetMainItems();
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            Thickness TitleMargin = CustomTitleBar.Margin;
            CustomTitleBar.Height = TitleBar.Height;
            CustomTitleBar.Margin = new Thickness(SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible ? 48 : 0, TitleMargin.Top, TitleBar.SystemOverlayRightInset, TitleMargin.Bottom);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                Frame.Navigate(typeof(AdapListPage), new AdaptiveViewModel(MenuItem.Tag.ToString().Contains("V") ? $"/page?url={MenuItem.Tag}" : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}"));
                RelayCommand RefreshButtonCommand = new RelayCommand((Frame.Content as AdapListPage).Refresh);
                RefreshButton.Command = RefreshButtonCommand;
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdapListPage AdapListPage)
            {
                RelayCommand RefreshButtonCommand = new RelayCommand(AdapListPage.Refresh);
                RefreshButton.Command = RefreshButtonCommand;
            }
        }

        private AppViewBackButtonVisibility TryGoBack()
        {
            if (!Frame.CanGoBack)
            { return AppViewBackButtonVisibility.Disabled; }

            Frame.GoBack();
            return AppViewBackButtonVisibility.Visible;
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => CustomTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        #region 进度条
        public void ShowProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
        }

        public void ShowProgressBar(double value)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = value;
        }

        public void PausedProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = true;
        }

        public void ErrorProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowPaused = false;
            ProgressBar.ShowError = true;
        }

        public void HideProgressBar()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = 0;
        }

        public void ShowMessage(string message = null) => AppTitle.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安 Lite";
        #endregion
    }

    public class MenuItem
    {
        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("CirclePage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem() { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem() { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem() { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem() { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem() { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }
    }
}