using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            }
            hamburgerMenuControl.ItemsSource = MenuItem.GetMainItems();
            hamburgerMenuControl.OptionsItemsSource = MenuItem.GetOptionsItems();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }
    }

    public class MenuItem : INotifyPropertyChanged
    {
        public Symbol Icon { get; set; }
        private ImageSource image;
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource Image
        {
            get => image;
            set
            {
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public int Index { get; set; }

        public static ObservableCollection<MenuItem> GetMainItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3},
                new MenuItem() { Icon = Symbol.Home, Name = "首页"/*, PageType = typeof(InitialPage)*/, Index = 1},
                new MenuItem() { Icon = Symbol.Shop, Name = "应用游戏"/*, PageType = typeof(AppRecommendPage)*/, Index = 2}
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                 new MenuItem() { Icon = Symbol.Contact, Name = string.Empty/*, PageType = typeof(UserPage)*/, Index = -1},
                 new MenuItem() { Icon = Symbol.Setting, Name = "设置"/*, PageType = typeof(SettingPage)*/, Index = 0}
            };
            return items;
        }
    }
}
