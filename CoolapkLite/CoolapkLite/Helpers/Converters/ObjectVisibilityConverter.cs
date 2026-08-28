using Microsoft.Toolkit.Uwp.UI.Converters;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers.Converters
{
    /// <summary>
    /// This class converts a object value into a Visibility value (if the value is null or empty returns a collapsed value).
    /// </summary>
    public class ObjectVisibilityConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectVisibilityConverter"/> class.
        /// </summary>
        public ObjectVisibilityConverter()
        {
            NotEmptyValue = Visibility.Visible;
            EmptyValue = Visibility.Collapsed;
        }
    }
}
