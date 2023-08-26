using CoolapkLite.Models.Images;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public class MediaPlayerElementExTemplateSettings : DependencyObject
    {
        #region Title

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(MediaPlayerElementExTemplateSettings),
                null);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #endregion

        #region Artist

        /// <summary>
        /// Identifies the <see cref="Artist"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ArtistProperty =
            DependencyProperty.Register(
                nameof(Artist),
                typeof(string),
                typeof(MediaPlayerElementExTemplateSettings),
                null);

        public string Artist
        {
            get => (string)GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }

        #endregion

        #region PosterSource

        /// <summary>
        /// Identifies the <see cref="PosterSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PosterSourceProperty =
            DependencyProperty.Register(
                nameof(PosterSource),
                typeof(ImageModel),
                typeof(MediaPlayerElementExTemplateSettings),
                null);

        /// <summary>
        /// Gets or sets the image source that is used for a placeholder image during <see cref="MediaPlayerElementEx"/> loading transition states.
        /// </summary>
        public ImageModel PosterSource
        {
            get => (ImageModel)GetValue(PosterSourceProperty);
            set => SetValue(PosterSourceProperty, value);
        }

        #endregion
    }
}
