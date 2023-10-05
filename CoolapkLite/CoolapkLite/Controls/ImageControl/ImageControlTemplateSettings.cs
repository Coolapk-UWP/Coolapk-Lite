using CoolapkLite.Models.Images;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public class ImageControlTemplateSettings : DependencyObject
    {
        #region ActualSource

        /// <summary>
        /// Identifies the <see cref="ActualSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualSourceProperty =
            DependencyProperty.Register(
                nameof(ActualSource),
                typeof(ImageModel),
                typeof(ImageControlTemplateSettings),
                new PropertyMetadata(null));

        public ImageModel ActualSource
        {
            get => (ImageModel)GetValue(ActualSourceProperty);
            set => SetValue(ActualSourceProperty, value);
        }

        #endregion

        #region ActualInitials

        private static readonly DependencyProperty ActualInitialsProperty =
            DependencyProperty.Register(
                nameof(ActualInitials),
                typeof(string),
                typeof(ImageControlTemplateSettings),
                new PropertyMetadata(string.Empty));

        public string ActualInitials
        {
            get => (string)GetValue(ActualInitialsProperty);
            set => SetValue(ActualInitialsProperty, value);
        }

        #endregion
    }
}
