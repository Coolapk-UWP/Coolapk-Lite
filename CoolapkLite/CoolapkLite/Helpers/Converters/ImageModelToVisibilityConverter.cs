using CoolapkLite.Models.Images;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers.Converters
{
    /// <summary>
    /// This class converts a <see cref="ImageModel"/> value into a Visibility value (if the value is null or empty returns a collapsed value).
    /// </summary>
    public class ImageModelToVisibilityConverter : EmptyImageModelToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageModelToVisibilityConverter"/> class.
        /// </summary>
        public ImageModelToVisibilityConverter()
        {
            NotEmptyValue = Visibility.Visible;
            EmptyValue = Visibility.Collapsed;
        }
    }
}
