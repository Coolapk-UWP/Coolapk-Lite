using Microsoft.Toolkit.Uwp.UI.Converters;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers.ValueConverters
{
    /// <summary>
    /// This class converts a object value into a Visibility value (if the value is null or empty returns a collapsed value).
    /// </summary>
    public class EmptyObjectToVisibilityConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyObjectToVisibilityConverter"/> class.
        /// </summary>
        public EmptyObjectToVisibilityConverter()
        {
            NotEmptyValue = Visibility.Visible;
            EmptyValue = Visibility.Collapsed;
        }
    }
}
