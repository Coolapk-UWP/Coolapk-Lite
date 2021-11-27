using CoolapkLite.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public bool IsBackButtonEnabled { get => BackButton.IsEnabled; set => BackButton.IsEnabled = value; }

        public string Title
        {
            get => TitleBlock.Text;
            set => TitleBlock.Text = value ?? string.Empty;
        }

        public event RoutedEventHandler RefreshEvent;

        public event RoutedEventHandler BackButtonClicked;

        public GridLength RightMargin { get => Block.Width; set => Block.Width = value; }

        public Visibility BackButtonVisibility { get => BackButton.Visibility; set => BackButton.Visibility = value; }
        public Visibility RefreshButtonVisibility { get => RefreshButton.Visibility; set => RefreshButton.Visibility = value; }
        public Visibility BackgroundVisibility { get => TitleBackground.Visibility; set => TitleBackground.Visibility = value; }

        public double TitleHeight { get => TitleGrid.Height; set => TitleGrid.Height = value; }
        public object RightAreaContent { get => UserContentPresenter.Content; set => UserContentPresenter.Content = value; }

        public TitleBar() => InitializeComponent();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshEvent?.Invoke(sender, e);

        private void BackButton_Click(object sender, RoutedEventArgs e) => BackButtonClicked?.Invoke(sender, e);

        private double PageTitleHeight => UIHelper.PageTitleHeight;

        private void TitleGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Grid || (e.OriginalSource is TextBlock a && a == TitleBlock))
            { RefreshEvent?.Invoke(sender, e); }
        }

        public void ShowProgressRing()
        {
            ProgressRing.IsActive = true;
            ProgressRing.Visibility = Visibility.Visible;
        }

        public void HideProgressRing()
        {
            ProgressRing.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Block.Width = Window.Current.Bounds.Width > 640 ? new GridLength(12) : new GridLength(60);
        }
    }
}
