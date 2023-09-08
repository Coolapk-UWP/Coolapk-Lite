using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class TitleBarTemplateSettings : DependencyObject
    {
        #region ProgressValue

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(
                nameof(ProgressValue),
                typeof(double),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(0d));

        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        #endregion

        #region IsProgressActive

        public static readonly DependencyProperty IsProgressActiveProperty =
            DependencyProperty.Register(
                nameof(IsProgressActive),
                typeof(bool),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(false));

        public bool IsProgressActive
        {
            get => (bool)GetValue(IsProgressActiveProperty);
            set => SetValue(IsProgressActiveProperty, value);
        }

        #endregion

        #region IsProgressIndeterminate

        public static readonly DependencyProperty IsProgressIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsProgressIndeterminate),
                typeof(bool),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(true));

        public bool IsProgressIndeterminate
        {
            get => (bool)GetValue(IsProgressIndeterminateProperty);
            set => SetValue(IsProgressIndeterminateProperty, value);
        }

        #endregion

        #region TopPaddingColumnGridLength

        public static readonly DependencyProperty TopPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(TopPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        public GridLength TopPaddingColumnGridLength
        {
            get => (GridLength)GetValue(TopPaddingColumnGridLengthProperty);
            set => SetValue(TopPaddingColumnGridLengthProperty, value);
        }

        public static readonly DependencyProperty LeftPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(LeftPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        #endregion

        #region LeftPaddingColumnGridLength

        public GridLength LeftPaddingColumnGridLength
        {
            get => (GridLength)GetValue(LeftPaddingColumnGridLengthProperty);
            set => SetValue(LeftPaddingColumnGridLengthProperty, value);
        }

        #endregion

        #region RightPaddingColumnGridLength

        public static readonly DependencyProperty RightPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(RightPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        public GridLength RightPaddingColumnGridLength
        {
            get => (GridLength)GetValue(RightPaddingColumnGridLengthProperty);
            set => SetValue(RightPaddingColumnGridLengthProperty, value);
        }

        #endregion
    }
}
