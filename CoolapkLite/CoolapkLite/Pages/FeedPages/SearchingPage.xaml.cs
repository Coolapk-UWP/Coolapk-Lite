using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
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
        private static int PivotIndex = 0;
        private SearchingViewModel Provider;
        private Thickness PivotTitleMargin => UIHelper.PivotTitleMargin;

        public SearchingPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
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
            if ((Pivot.SelectedItem as PivotItem).Content is ListView ListView && ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                RelayCommand RefreshButtonCommand = new RelayCommand(async () => await ItemsSource.Refresh(true));
                RefreshButton.Command = RefreshButtonCommand;
            }
            RightHeader.Visibility = Pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Pivot_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = Window.Current.Bounds.Width > 640 ? 0 : 48;

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
        }

        private async void ListView_RefreshRequested(object sender, EventArgs e)
        {
            ListView ListView = sender as ListView;
            if (ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                await ItemsSource.Refresh(true);
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsStackPanel StackPanel = (sender as FrameworkElement).FindDescendant<ItemsStackPanel>();
            if (StackPanel != null) { StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch; }
        }

        #region 搜索框

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, sender.Text), true);
                if (isSucceed && result != null && result is JArray array && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (JToken token in array)
                    {
                        switch (token.Value<string>("entityType"))
                        {
                            case "apk":
                                observableCollection.Add(new SearchWord(token as JObject));
                                break;
                            case "searchWord":
                            default:
                                observableCollection.Add(new SearchWord(token as JObject));
                                break;
                        }
                    }
                }
            }
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            //if (args.ChosenSuggestion is AppModel app)
            //{
            //    UIHelper.NavigateInSplitPane(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url);
            //}
            //else
            if (args.ChosenSuggestion is SearchWord word)
            {
                Provider.Title = word.ToString();
                await Provider.Refresh(true);
            }
            else if (args.ChosenSuggestion is null)
            {
                Provider.Title = sender.Text;
                await Provider.Refresh(true);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord searchWord)
            {
                sender.Text = searchWord.ToString();
            }
        }

        #endregion
    }
}
