using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public class PersonPicture : ImageControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageControl"/> class.
        /// </summary>
        public PersonPicture() : base()
        {
            DefaultStyleKey = typeof(PersonPicture);
        }

        #region DisplayName

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                nameof(DisplayName),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnDisplayNamePropertyChanged));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void OnDisplayNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PersonPicture)sender).OnDisplayNamePropertyChanged();
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                VisualStateManager.GoToState(this, "NoInitials", false);
            }
            base.OnApplyTemplate();
        }

        private void OnDisplayNamePropertyChanged()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                VisualStateManager.GoToState(this, "NoInitials", false);
            }
            else
            {
                string m_displayNameInitials = InitialsGenerator.InitialsFromDisplayName(DisplayName);
                if (string.IsNullOrWhiteSpace(m_displayNameInitials))
                {
                    VisualStateManager.GoToState(this, "NoInitials", false);
                }
                else
                {
                    TemplateSettings.ActualInitials = m_displayNameInitials;
                    VisualStateManager.GoToState(this, "Initials", false);
                }
            }
        }
    }
}
