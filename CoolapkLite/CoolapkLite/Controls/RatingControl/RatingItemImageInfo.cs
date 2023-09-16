using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls
{
    public class RatingItemImageInfo : RatingItemInfo
    {
        /// <summary>
        /// Initializes a new instance of the RatingItemImageInfo class.
        /// </summary>
        public RatingItemImageInfo()
        {
        }

        #region DisabledImage

        public static readonly DependencyProperty DisabledImageProperty =
            DependencyProperty.Register(
                nameof(DisabledImage),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource DisabledImage
        {
            get => (ImageSource)GetValue(DisabledImageProperty);
            set => SetValue(DisabledImageProperty, value);
        }

        #endregion

        #region Image

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                nameof(Image),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        #endregion

        #region PlaceholderImage

        public static readonly DependencyProperty PlaceholderImageProperty =
            DependencyProperty.Register(
                nameof(PlaceholderImage),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource PlaceholderImage
        {
            get => (ImageSource)GetValue(PlaceholderImageProperty);
            set => SetValue(PlaceholderImageProperty, value);
        }

        #endregion

        #region PointerOverImage

        public static readonly DependencyProperty PointerOverImageProperty =
            DependencyProperty.Register(
                nameof(PointerOverImage),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource PointerOverImage
        {
            get => (ImageSource)GetValue(PointerOverImageProperty);
            set => SetValue(PointerOverImageProperty, value);
        }

        #endregion

        #region PointerOverPlaceholderImage

        public static readonly DependencyProperty PointerOverPlaceholderImageProperty =
            DependencyProperty.Register(
                nameof(PointerOverPlaceholderImage),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource PointerOverPlaceholderImage
        {
            get => (ImageSource)GetValue(PointerOverPlaceholderImageProperty);
            set => SetValue(PointerOverPlaceholderImageProperty, value);
        }

        #endregion

        #region UnsetImage

        public static readonly DependencyProperty UnsetImageProperty =
            DependencyProperty.Register(
                nameof(UnsetImage),
                typeof(ImageSource),
                typeof(RatingItemImageInfo),
                null);

        public ImageSource UnsetImage
        {
            get => (ImageSource)GetValue(UnsetImageProperty);
            set => SetValue(UnsetImageProperty, value);
        }

        #endregion
    }
}
