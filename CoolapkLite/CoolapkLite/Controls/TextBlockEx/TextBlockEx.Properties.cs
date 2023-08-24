using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls
{
    public partial class TextBlockEx
    {
        #region IsColorFontEnabled

        /// <summary>
        /// Identifies the <see cref="IsColorFontEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsColorFontEnabledProperty =
            DependencyProperty.Register(
                nameof(IsColorFontEnabled),
                typeof(bool),
                typeof(TextBlockEx),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that determines whether font glyphs that contain color layers,
        /// such as Segoe UI Emoji, are rendered in color.
        /// </summary>
        public bool IsColorFontEnabled
        {
            get => (bool)GetValue(IsColorFontEnabledProperty);
            set => SetValue(IsColorFontEnabledProperty, value);
        }

        #endregion

        #region IsTextSelectionEnabled

        /// <summary>
        /// Identifies the <see cref="IsTextSelectionEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTextSelectionEnabled),
                typeof(bool),
                typeof(TextBlockEx),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that determines whether text content of the <see cref="TextBlockEx"/> can
        /// be selected for clipboard or drag purposes, or for UI styling changes that indicate selected text.
        /// </summary>
        public bool IsTextSelectionEnabled
        {
            get => (bool)GetValue(IsTextSelectionEnabledProperty);
            set => SetValue(IsTextSelectionEnabledProperty, value);
        }

        #endregion

        #region MaxLines

        /// <summary>
        /// Identifies the <see cref="MaxLines"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                nameof(MaxLines),
                typeof(int),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the maximum lines of text shown in the <see cref="TextBlockEx"/>.
        /// </summary>
        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        #endregion

        #region SelectionHighlightColor

        /// <summary>
        /// Identifies the <see cref="SelectionHighlightColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionHighlightColorProperty =
            DependencyProperty.Register(
                nameof(SelectionHighlightColor),
                typeof(SolidColorBrush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to highlight the selected text.
        /// </summary>
        public SolidColorBrush SelectionHighlightColor
        {
            get => (SolidColorBrush)GetValue(SelectionHighlightColorProperty);
            set => SetValue(SelectionHighlightColorProperty, value);
        }

        #endregion

        #region Text

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextBlockEx),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

        /// <summary>
        /// Gets or sets the text contents of a <see cref="TextBlockEx"/>.
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextBlockEx).RenderTextBlock();
        }

        #endregion

        #region TextAlignment

        /// <summary>
        /// Identifies the <see cref="TextAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(TextBlockEx),
                new PropertyMetadata(TextAlignment.Left));

        /// <summary>
        /// Gets or sets a value that indicates how the line box height is determined for each line of text in the <see cref="TextBlockEx"/>.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        #endregion

        #region TextLineBounds

        /// <summary>
        /// Identifies the <see cref="TextLineBounds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextLineBoundsProperty =
            DependencyProperty.Register(
                nameof(TextLineBounds),
                typeof(TextLineBounds),
                typeof(TextBlockEx),
                new PropertyMetadata(TextLineBounds.Full));

        /// <summary>
        /// Gets or sets a value that indicates how the line box height is determined for each line of text in the <see cref="TextBlockEx"/>.
        /// </summary>
        public TextLineBounds TextLineBounds
        {
            get => (TextLineBounds)GetValue(TextLineBoundsProperty);
            set => SetValue(TextLineBoundsProperty, value);
        }

        #endregion

        #region TextReadingOrder

        /// <summary>
        /// Identifies the <see cref="TextReadingOrder"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextReadingOrderProperty =
            DependencyProperty.Register(
                nameof(TextReadingOrder),
                typeof(TextReadingOrder),
                typeof(TextBlockEx),
                new PropertyMetadata(TextReadingOrder.DetectFromContent));

        /// <summary>
        /// Gets or sets a value that indicates how the reading order is determined for the <see cref="TextBlockEx"/>.
        /// </summary>
        public TextReadingOrder TextReadingOrder
        {
            get => (TextReadingOrder)GetValue(TextReadingOrderProperty);
            set => SetValue(TextReadingOrderProperty, value);
        }

        #endregion

        #region TextTrimming

        /// <summary>
        /// Identifies the <see cref="TextTrimming"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(
                nameof(TextTrimming),
                typeof(TextTrimming),
                typeof(TextBlockEx),
                new PropertyMetadata(TextTrimming.CharacterEllipsis));

        /// <summary>
        /// Gets or sets a value that indicates how text is trimmed when it overflows the content area.
        /// </summary>
        public TextTrimming TextTrimming
        {
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        #endregion

        #region TextWrapping

        /// <summary>
        /// Identifies the <see cref="TextWrapping"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                nameof(TextWrapping),
                typeof(TextWrapping),
                typeof(TextBlockEx),
                new PropertyMetadata(TextWrapping.Wrap));

        /// <summary>
        /// Gets or sets a value that indicates whether text wrapping occurs if a line of text
        /// extends beyond the available width of the <see cref="TextBlockEx"/>.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        #endregion
    }
}
