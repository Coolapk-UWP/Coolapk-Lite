using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// Arranges child elements into a single line that can be oriented horizontally
    /// or vertically.
    /// </summary>
    public class StackPanelEx : Panel
    {
        #region Padding

        /// <summary>
        /// Identifies the <see cref="Padding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(StackPanelEx),
                new PropertyMetadata(null, OnLayoutPropertyChanged));

        /// <summary>
        /// Gets or sets the distance between the border and its child object.
        /// </summary>
        /// <value>The dimensions of the space between the border and its child as a <see cref="Thickness"/> value.
        /// <see cref="Thickness"/> is a structure that stores dimension values using pixel measures.</value>
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(StackPanelEx),
                new PropertyMetadata(Orientation.Vertical, OnLayoutPropertyChanged));

        /// <summary>
        /// Gets or sets the dimension by which child elements are stacked.
        /// </summary>
        /// <value>One of the enumeration values that specifies the orientation
        /// of child elements. The default is <see cref="Orientation.Vertical"/>.</value>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        #endregion

        #region Spacing

        /// <summary>
        /// Identifies the <see cref="Spacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(double),
                typeof(StackPanelEx),
                new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        /// <summary>
        /// Gets or sets a uniform distance (in pixels) between stacked items.
        /// It is applied in the direction of the StackPanel's Orientation.
        /// </summary>
        /// <value>The uniform distance (in pixels) between stacked items.</value>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StackPanelEx"/> class.
        /// </summary>
        public StackPanelEx() { }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as StackPanelEx).InvalidateMeasure();
            }
        }

        /// <summary>
        /// Measures the child elements of a <see cref="StackPanelEx"/> in anticipation
        /// of arranging them during the StackPanelEx.ArrangeOverride(<see cref="Size"/>)
        /// pass.
        /// </summary>
        /// <param name="availableSize">An upper limit <see cref="Size"/> that should not be exceeded.</param>
        /// <returns>The <see cref="Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // We adjust availableSize based on our Padding and BorderThickness:
            Thickness padding = Padding;
            double effectiveHorizontalPadding = padding.Left + padding.Right;
            double effectiveVerticalPadding = padding.Top + padding.Bottom;

            bool fHorizontal = Orientation == Orientation.Horizontal;
            double spacing = Spacing;
            bool hasVisibleChild = false;

            Size adjustedSize = availableSize;

            if (fHorizontal)
            {
                adjustedSize.Width = double.PositiveInfinity;
                adjustedSize.Height -= effectiveVerticalPadding;
                adjustedSize.Height = Math.Max(0.0, adjustedSize.Height);
            }
            else
            {
                adjustedSize.Height = double.PositiveInfinity;
                adjustedSize.Width -= effectiveHorizontalPadding;
                adjustedSize.Width = Math.Max(0.0, adjustedSize.Width);
            }

            Size desiredSizeUnpadded = new Size();
            foreach (UIElement child in Children.OfType<UIElement>())
            {
                bool isVisible = child.Visibility != Visibility.Collapsed;

                if (isVisible && !hasVisibleChild)
                {
                    hasVisibleChild = true;
                }

                child.Measure(adjustedSize);
                Size childDesiredSize = child.DesiredSize;

                if (fHorizontal)
                {
                    desiredSizeUnpadded.Width += (isVisible ? spacing : 0) + childDesiredSize.Width;
                    desiredSizeUnpadded.Height = Math.Max(desiredSizeUnpadded.Height, childDesiredSize.Height);
                }
                else
                {
                    desiredSizeUnpadded.Width = Math.Max(desiredSizeUnpadded.Width, childDesiredSize.Width);
                    desiredSizeUnpadded.Height += (isVisible ? spacing : 0) + childDesiredSize.Height;
                }
            }

            if (fHorizontal)
            {
                desiredSizeUnpadded.Width -= hasVisibleChild ? spacing : 0;
            }
            else
            {
                desiredSizeUnpadded.Height -= hasVisibleChild ? spacing : 0;
            }

            Size desiredSize = desiredSizeUnpadded;
            desiredSize.Width += effectiveHorizontalPadding;
            desiredSize.Height += effectiveVerticalPadding;

            return desiredSize;
        }

        /// <summary>
        /// Arranges the content of a <see cref="StackPanelEx"/> element.
        /// </summary>
        /// <param name="finalSize">The <see cref="Size"/> that this element should use to arrange its child elements.</param>
        /// <returns>
        /// The <see cref="Size"/> that represents the arranged size of this <see cref="StackPanelEx"/>
        /// element and its child elements.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = finalSize;

            Thickness padding = Padding;

            double effectiveHorizontalPadding = padding.Left + padding.Right;
            double effectiveVerticalPadding = padding.Top + padding.Bottom;
            double leftAdjustment = padding.Left;
            double topAdjustment = padding.Top;

            Size adjustedSize = finalSize;
            adjustedSize.Width -= effectiveHorizontalPadding;
            adjustedSize.Height -= effectiveVerticalPadding;

            adjustedSize.Width = Math.Max(0.0, adjustedSize.Width);
            adjustedSize.Height = Math.Max(0.0, adjustedSize.Height);

            Rect arrangeRect = new Rect(leftAdjustment, topAdjustment, adjustedSize.Width, adjustedSize.Height);

            bool fHorizontal = Orientation == Orientation.Horizontal;
            double spacing = Spacing;
            double previousChildSize = 0.0;

            foreach (UIElement child in Children.OfType<UIElement>())
            {
                if (fHorizontal)
                {
                    arrangeRect.X += previousChildSize;
                    previousChildSize = child.DesiredSize.Width;
                    arrangeRect.Width = previousChildSize;
                    arrangeRect.Height = Math.Max(adjustedSize.Height, child.DesiredSize.Height);
                }
                else
                {
                    arrangeRect.Y += previousChildSize;
                    previousChildSize = child.DesiredSize.Height;
                    arrangeRect.Height = previousChildSize;
                    arrangeRect.Width = Math.Max(adjustedSize.Width, child.DesiredSize.Width);
                }

                if (child.Visibility != Visibility.Collapsed)
                {
                    previousChildSize += spacing;
                }

                child.Arrange(arrangeRect);
            }

            return result;
        }
    }
}
