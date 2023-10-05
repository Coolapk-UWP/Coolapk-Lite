using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public partial class PolylineView
    {
        #region ItemSource

        /// <summary>
        /// Identifies the <see cref="ItemSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<Point>),
                typeof(PolylineView),
                new PropertyMetadata(null, OnItemSourcePropertyChanged));

        public IEnumerable<Point> ItemSource
        {
            get => (IEnumerable<Point>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        #endregion

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(PolylineView),
                null);

        /// <summary>
        /// Gets or sets the payload which shall be encoded in the QR code.
        /// </summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the <see cref="ContentCornerRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(PolylineView),
                null);

        /// <summary>
        /// Gets or sets the payload which shall be encoded in the QR code.
        /// </summary>
        public CornerRadius ContentCornerRadius
        {
            get => (CornerRadius)GetValue(ContentCornerRadiusProperty);
            set => SetValue(ContentCornerRadiusProperty, value);
        }

        #endregion

        #region TemplateSettings

        /// <summary>
        /// Identifies the <see cref="TemplateSettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(PolylineViewTemplateSettings),
                typeof(PolylineView),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TemplateSettings.
        /// </summary>
        public PolylineViewTemplateSettings TemplateSettings
        {
            get => (PolylineViewTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsProperty, value);
        }

        #endregion`

        private static void OnItemSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((PolylineView)d).OnItemSourceChanged();
            }
        }
    }
}
