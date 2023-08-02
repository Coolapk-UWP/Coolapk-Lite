using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class PolylineViewTemplateSettings : DependencyObject
    {
        #region PointCollection

        /// <summary>
        /// Identifies the <see cref="PointCollection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PointCollectionProperty =
            DependencyProperty.Register(
                nameof(PointCollection),
                typeof(PointCollection),
                typeof(PolylineViewTemplateSettings),
                null);

        /// <summary>
        /// Gets or sets the PointCollection.
        /// </summary>
        public PointCollection PointCollection
        {
            get => (PointCollection)GetValue(PointCollectionProperty);
            set => SetValue(PointCollectionProperty, value);
        }

        #endregion
    }
}
