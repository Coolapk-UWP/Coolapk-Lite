using CoolapkLite.Models.Images;
using Microsoft.Toolkit.Uwp.UI.Converters;

namespace CoolapkLite.Helpers.Converters
{
    /// <summary>
    /// This class converts a <see cref="ImageModel"/> value into a an object (if the value is null or empty returns the false value).
    /// Can be used to bind a visibility, a color or an image to the value of a <see cref="ImageModel"/>.
    /// </summary>
    public class EmptyImageModelToObjectConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Checks <see cref="ImageModel"/> for emptiness.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is null or empty <see cref="ImageModel"/>, false otherwise.</returns>
        protected override bool CheckValueIsEmpty(object value)
        {
            return (value as ImageModel)?.IsEmpty != false;
        }
    }
}
