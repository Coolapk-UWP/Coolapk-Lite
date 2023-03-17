using CoolapkLite.Helpers;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class TitleBarTemplateSettings : DependencyObject
    {
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(
                nameof(ProgressValue),
                typeof(double),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty IsProgressActiveProperty =
            DependencyProperty.Register(
                nameof(IsProgressActive),
                typeof(bool),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsProgressIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsProgressIndeterminate),
                typeof(bool),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(true));

        public static readonly DependencyProperty TopPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(TopPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        public static readonly DependencyProperty LeftPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(LeftPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        public static readonly DependencyProperty RightPaddingColumnGridLengthProperty =
            DependencyProperty.Register(
                nameof(RightPaddingColumnGridLength),
                typeof(GridLength),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(new GridLength(0)));

        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public bool IsProgressActive
        {
            get => (bool)GetValue(IsProgressActiveProperty);
            set => SetValue(IsProgressActiveProperty, value);
        }

        public bool IsProgressIndeterminate
        {
            get => (bool)GetValue(IsProgressIndeterminateProperty);
            set => SetValue(IsProgressIndeterminateProperty, value);
        }

        public GridLength TopPaddingColumnGridLength
        {
            get => (GridLength)GetValue(TopPaddingColumnGridLengthProperty);
            set => SetValue(TopPaddingColumnGridLengthProperty, value);
        }

        public GridLength LeftPaddingColumnGridLength
        {
            get => (GridLength)GetValue(LeftPaddingColumnGridLengthProperty);
            set => SetValue(LeftPaddingColumnGridLengthProperty, value);
        }

        public GridLength RightPaddingColumnGridLength
        {
            get => (GridLength)GetValue(RightPaddingColumnGridLengthProperty);
            set => SetValue(RightPaddingColumnGridLengthProperty, value);
        }
    }
}
