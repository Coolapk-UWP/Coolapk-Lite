﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
    public sealed partial class SearchingPage : Page
    {
        private Func<bool, Task> Refresh;
        private static int PivotIndex = 0;

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(SearchingViewModel),
                typeof(SearchingPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public SearchingViewModel Provider
        {
            get => (SearchingViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public SearchingPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                if (Provider.PivotIndex != -1)
                { PivotIndex = Provider.PivotIndex; }
                await Provider.Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.SelectedIndex = PivotIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is ListView ListView && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                Refresh = (reset) => _ = ItemsSource.Refresh(reset);
            }
            RightHeader.Visibility = Pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Pivot_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = this.GetXAMLRootSize().Width > 640 ? 0 : 48;

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (Refresh != null)
            {
                _ = Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is ListView ListView && ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        private async void ListView_RefreshRequested(object sender, EventArgs e)
        {
            ListView ListView = sender as ListView;
            if (ListView.ItemsSource is EntityItemSource ItemsSource)
            {
                await ItemsSource.Refresh(true);
            }
        }

        #region 搜索框

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<Entity> observableCollection = new ObservableCollection<Entity>();
                sender.ItemsSource = observableCollection;
                string keyWord = sender.Text;
                await ThreadSwitcher.ResumeBackgroundAsync();
                await semaphoreSlim.WaitAsync();
                try
                {
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, keyWord), true);
                    if (isSucceed && result != null && result is JArray array && array.Count > 0)
                    {
                        foreach (JObject token in array.OfType<JObject>())
                        {
                            switch (token.Value<string>("entityType"))
                            {
                                case "apk":
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new AppModel(token)));
                                    break;
                                case "searchWord":
                                default:
                                    await Dispatcher.AwaitableRunAsync(() => observableCollection.Add(new SearchWord(token)));
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppModel app)
            {
                _ = Frame.OpenLinkAsync(app.Url);
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                Provider.Title = word.ToString();
                await Provider.Refresh(true);
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                Provider.Title = sender.Text;
                await Provider.Refresh(true);
            }
        }

        #endregion
    }
}
