using CoolapkLite.Models.Images;
using System;
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

        #region SymbolFontFamily

        /// <summary>
        /// Identifies the <see cref="SymbolFontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolFontFamilyProperty =
            DependencyProperty.Register(
                nameof(SymbolFontFamily),
                typeof(FontFamily),
                typeof(TextBlockEx),
                new PropertyMetadata((FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]));

        /// <summary>
        /// Gets or sets the font used to display symbol.
        /// </summary>
        public FontFamily SymbolFontFamily
        {
            get => (FontFamily)GetValue(SymbolFontFamilyProperty);
            set => SetValue(SymbolFontFamilyProperty, value);
        }

        #endregion

        #region QuoteBackground

        /// <summary>
        /// Identifies the <see cref="QuoteBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuoteBackgroundProperty =
            DependencyProperty.Register(
                nameof(QuoteBackground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to fill the background of a quote block.
        /// </summary>
        public Brush QuoteBackground
        {
            get => (Brush)GetValue(QuoteBackgroundProperty);
            set => SetValue(QuoteBackgroundProperty, value);
        }

        #endregion

        #region QuoteBorderBrush

        /// <summary>
        /// Identifies the <see cref="QuoteBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuoteBorderBrushProperty =
            DependencyProperty.Register(
                nameof(QuoteBorderBrush),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to render a quote border.
        /// </summary>
        public Brush QuoteBorderBrush
        {
            get => (Brush)GetValue(QuoteBorderBrushProperty);
            set => SetValue(QuoteBorderBrushProperty, value);
        }

        #endregion

        #region QuoteBorderThickness

        /// <summary>
        /// Identifies the <see cref="QuoteBorderThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuoteBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(QuoteBorderThickness),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(2, 0, 0, 0)));

        /// <summary>
        /// Gets or sets the thickness of quote borders.
        /// </summary>
        public Thickness QuoteBorderThickness
        {
            get => (Thickness)GetValue(QuoteBorderThicknessProperty);
            set => SetValue(QuoteBorderThicknessProperty, value);
        }

        #endregion

        #region QuoteForeground

        /// <summary>
        /// Identifies the <see cref="QuoteForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuoteForegroundProperty =
            DependencyProperty.Register(
                nameof(QuoteForeground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to render the text inside a quote block.
        /// </summary>
        public Brush QuoteForeground
        {
            get => (Brush)GetValue(QuoteForegroundProperty);
            set => SetValue(QuoteForegroundProperty, value);
        }

        #endregion

        #region QuoteMargin

        /// <summary>
        /// Identifies the <see cref="QuoteMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuoteMarginProperty =
            DependencyProperty.Register(
                nameof(QuoteMargin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(7, 5, 0, 5)));

        /// <summary>
        /// Gets or sets the space outside of quote borders.
        /// </summary>
        public Thickness QuoteMargin
        {
            get => (Thickness)GetValue(QuoteMarginProperty);
            set => SetValue(QuoteMarginProperty, value);
        }

        #endregion

        #region QuotePadding

        /// <summary>
        /// Identifies the <see cref="QuotePadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty QuotePaddingProperty =
            DependencyProperty.Register(
                nameof(QuotePadding),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(7, 2, 0, 4)));

        /// <summary>
        /// Gets or sets the space between the quote border and the text.
        /// </summary>
        public Thickness QuotePadding
        {
            get => (Thickness)GetValue(QuotePaddingProperty);
            set => SetValue(QuotePaddingProperty, value);
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

        #region ContainerForeground

        /// <summary>
        /// Identifies the <see cref="ContainerForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContainerForegroundProperty =
            DependencyProperty.Register(
                "ContainerForeground",
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets the brush used to render the text inside the container.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>The brush used to render the text inside the container.</returns>
        public static Brush GetContainerForeground(DependencyObject control)
        {
            return (Brush)control.GetValue(ContainerForegroundProperty);
        }

        /// <summary>
        /// Sets the brush used to render the text inside the container.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetContainerForeground(DependencyObject control, Brush value)
        {
            control.SetValue(ContainerForegroundProperty, value);
        }

        #endregion

        /// <summary>
        /// Fired when a link element was tapped.
        /// </summary>
        public event EventHandler<string> LinkClicked;

        /// <summary>
        /// Fired when an image element was tapped.
        /// </summary>
        public event EventHandler<ImageModel> ImageClicked;
    }
}
