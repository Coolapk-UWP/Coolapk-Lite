﻿using CoolapkLite.Controls;
using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AdaptivePage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(AdaptiveViewModel),
                typeof(AdaptivePage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public AdaptiveViewModel Provider
        {
            get => (AdaptiveViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public AdaptivePage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AdaptiveViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
            }

            Provider.LoadMoreStarted += OnLoadMoreStarted;
            Provider.LoadMoreCompleted += OnLoadMoreCompleted;

            if (!Provider.Any)
            {
                await Refresh(true);
            }

            Provider.IsShowTitle = this.FindAscendant<Page>() is MainPage;
            ListViewHelper.SetPadding(ListView, Provider.IsShowTitle ? (Thickness)Application.Current.Resources["StackPanelMargin"] : default);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= OnLoadMoreStarted;
            Provider.LoadMoreCompleted -= OnLoadMoreCompleted;
        }

        private void OnLoadMoreStarted() => this.ShowProgressBar();

        private void OnLoadMoreCompleted() => this.HideProgressBar();

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            Provider.IsShowTitle = this.FindAscendant<Pivot>() is null;
            ListView.UpdatePadding(Provider.IsShowTitle ? (Thickness)Application.Current.Resources["StackPanelMargin"] : default);
        }
    }
}
