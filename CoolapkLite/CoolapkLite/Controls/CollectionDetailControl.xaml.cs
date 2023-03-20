using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.FeedPages;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class CollectionDetailControl : UserControl, INotifyPropertyChanged
    {
        private CollectionViewModel provider;
        public CollectionViewModel Provider
        {
            get => provider;
            set
            {
                if (provider != value)
                {
                    provider = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public static readonly DependencyProperty IDProperty = DependencyProperty.Register(
           nameof(ID),
           typeof(int),
           typeof(CollectionDetailControl),
           new PropertyMetadata(0, OnIDPropertyChanged));

        public int ID
        {
            get => (int)GetValue(IDProperty);
            set => SetValue(IDProperty, value);
        }
        private static void OnIDPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as CollectionDetailControl).OnIDPropertyChanged(e);
            }
        }

        public CollectionDetailControl()
        {
            this.InitializeComponent();
        }

        private async void OnIDPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int id)
            {
                Provider = new CollectionViewModel(id.ToString());
                await Provider.Refresh();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShyHeaderListView.ItemsSource is EntityItemSourse entities)
            {
                _ = entities.Refresh(true);
            }
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) => Block.Width = Window.Current.Bounds.Width > 640 ? 0 : 48;
    }
}
