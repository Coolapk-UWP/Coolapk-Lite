using Windows.Foundation;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class TitleBar
    {
        #region CustomContent

        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register(
                nameof(CustomContent),
                typeof(object),
                typeof(TitleBar),
                new PropertyMetadata(default, OnCustomContentPropertyChanged));

        public object CustomContent
        {
            get => GetValue(CustomContentProperty);
            set => SetValue(CustomContentProperty, value);
        }

        private static void OnCustomContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnCustomContentPropertyChanged(e);
        }

        #endregion

        #region IconSource

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(UIElement),
                typeof(TitleBar),
                new PropertyMetadata(default(UIElement), OnIconSourcePropertyChanged));

        public UIElement IconSource
        {
            get => (UIElement)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIconSourcePropertyChanged(e);
        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool), OnIsBackButtonVisiblePropertyChanged));

        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        private static void OnIsBackButtonVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIsBackButtonVisiblePropertyChanged(e);
        }

        #endregion

        #region IsBackEnabled

        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool)));

        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        #endregion

        #region IsRefreshButtonVisible

        public static readonly DependencyProperty IsRefreshButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsRefreshButtonVisible),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool), OnIsRefreshButtonVisiblePropertyChanged));

        public bool IsRefreshButtonVisible
        {
            get => (bool)GetValue(IsRefreshButtonVisibleProperty);
            set => SetValue(IsRefreshButtonVisibleProperty, value);
        }

        private static void OnIsRefreshButtonVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnIsRefreshButtonVisiblePropertyChanged(e);
        }

        #endregion

        #region IsRefreshEnabled

        public static readonly DependencyProperty IsRefreshEnabledProperty =
            DependencyProperty.Register(
                nameof(IsRefreshEnabled),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(default(bool)));

        public bool IsRefreshEnabled
        {
            get => (bool)GetValue(IsRefreshEnabledProperty);
            set => SetValue(IsRefreshEnabledProperty, value);
        }

        #endregion

        #region TemplateSettings

        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(TitleBarTemplateSettings),
                typeof(TitleBar),
                new PropertyMetadata(null));

        public TitleBarTemplateSettings TemplateSettings
        {
            get => (TitleBarTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsProperty, value);
        }

        #endregion

        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(default(string), OnTitlePropertyChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnTitlePropertyChanged(e);
        }

        #endregion

        #region CompactModeThresholdWidth

        public static readonly DependencyProperty CompactModeThresholdWidthProperty =
            DependencyProperty.Register(
                nameof(CompactModeThresholdWidth),
                typeof(double),
                typeof(TitleBar),
                new PropertyMetadata(641.0));

        public double CompactModeThresholdWidth
        {
            get => (double)GetValue(CompactModeThresholdWidthProperty);
            set => SetValue(CompactModeThresholdWidthProperty, value);
        }

        #endregion

        #region TopPadding

        public static readonly DependencyProperty TopPaddingProperty =
            DependencyProperty.Register(
                nameof(TopPadding),
                typeof(double),
                typeof(TitleBar),
                new PropertyMetadata(32d, OnTopPaddingPropertyChanged));

        public double TopPadding
        {
            get => (double)GetValue(TopPaddingProperty);
            set => SetValue(TopPaddingProperty, value);
        }

        private static void OnTopPaddingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TitleBar)d).OnTopPaddingPropertyChanged(e);
        }

        #endregion

        public event TypedEventHandler<TitleBar, object> BackRequested;
        public event TypedEventHandler<TitleBar, object> RefreshRequested;
    }
}
