using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls
{
    [ContentProperty(Name = nameof(ItemSource))]
    [TemplatePart(Name = PolylineBorderName, Type = typeof(Border))]
    public partial class PolylineView : Control
    {
        private const string PolylineBorderName = "PART_PolylineBorder";

        private Border PolylineBorder;

        /// <summary>
        /// Creates a new instance of the <see cref="PolylineView"/> class.
        /// </summary>
        public PolylineView()
        {
            DefaultStyleKey = typeof(PolylineView);
            SetValue(TemplateSettingsProperty, new PolylineViewTemplateSettings());
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (PolylineBorder != null)
            {
                PolylineBorder.SizeChanged -= OnSizeChanged;
            }

            PolylineBorder = GetTemplateChild(PolylineBorderName) as Border;

            if (PolylineBorder != null)
            {
                PolylineBorder.SizeChanged += OnSizeChanged;
            }

            UpdateQRCode();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateQRCode();
        }

        private void OnItemSourceChanged()
        {
            UpdateQRCode();
        }

        private void UpdateQRCode()
        {
            PolylineViewTemplateSettings templateSettings = TemplateSettings;

            if (ItemSource?.Any() != true)
            {
                templateSettings.PointCollection = null;
                return;
            }

            if (PolylineBorder != null)
            {
                double actualWidth = PolylineBorder.ActualWidth - StrokeThickness;
                double actualHeight = PolylineBorder.ActualHeight - StrokeThickness;

                double miniX = ItemSource.Min((x) => x.X);
                double miniY = ItemSource.Min((x) => x.Y);

                double width = ItemSource.Max((x) => x.X) - miniX;
                double height = ItemSource.Max((x) => x.Y) - miniY;

                double scaleX = actualWidth / width;
                double scaleY = actualHeight / height;

                double halfStroke = StrokeThickness / 2;

                if (width != 0 && height != 0)
                {
                    PointCollection points = new PointCollection();

                    foreach (Point point in ItemSource)
                    {
                        points.Add(new Point { X = (scaleX * (point.X - miniX)) + halfStroke, Y = ActualHeight - (scaleY * (point.Y - miniY)) - halfStroke });
                    }

                    templateSettings.PointCollection = points;
                }
            }
        }
    }
}
