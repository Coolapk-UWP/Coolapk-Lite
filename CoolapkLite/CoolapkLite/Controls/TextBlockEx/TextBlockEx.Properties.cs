using CoolapkLite.Models.Images;
using System;
using Windows.UI.Text;
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

        #region CodeBackground

        /// <summary>
        /// Identifies the <see cref="CodeBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeBackgroundProperty =
            DependencyProperty.Register(
                nameof(CodeBackground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to fill the background of a code block.
        /// </summary>
        public Brush CodeBackground
        {
            get => (Brush)GetValue(CodeBackgroundProperty);
            set => SetValue(CodeBackgroundProperty, value);
        }

        #endregion

        #region CodeBorderBrush

        /// <summary>
        /// Identifies the <see cref="CodeBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeBorderBrushProperty =
            DependencyProperty.Register(
                nameof(CodeBorderBrush),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to render the border fill of a code block.
        /// </summary>
        public Brush CodeBorderBrush
        {
            get => (Brush)GetValue(CodeBorderBrushProperty);
            set => SetValue(CodeBorderBrushProperty, value);
        }

        #endregion

        #region CodeBorderThickness

        /// <summary>
        /// Identifies the <see cref="CodeBorderThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(CodeBorderThickness),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0)));

        /// <summary>
        /// Gets or sets the thickness of the border around code blocks.
        /// </summary>
        public Thickness CodeBorderThickness
        {
            get => (Thickness)GetValue(CodeBorderThicknessProperty);
            set => SetValue(CodeBorderThicknessProperty, value);
        }

        #endregion

        #region CodeForeground

        /// <summary>
        /// Identifies the <see cref="CodeForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeForegroundProperty =
            DependencyProperty.Register(
                nameof(CodeForeground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to render the text inside a code block.
        /// </summary>
        public Brush CodeForeground
        {
            get => (Brush)GetValue(CodeForegroundProperty);
            set => SetValue(CodeForegroundProperty, value);
        }

        #endregion

        #region CodeMargin

        /// <summary>
        /// Identifies the <see cref="CodeMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeMarginProperty =
            DependencyProperty.Register(
                nameof(CodeMargin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 7, 0, 7)));

        /// <summary>
        /// Gets or sets the space between the code border and the text.
        /// </summary>
        public Thickness CodeMargin
        {
            get => (Thickness)GetValue(CodeMarginProperty);
            set => SetValue(CodeMarginProperty, value);
        }

        #endregion

        #region CodePadding

        /// <summary>
        /// Identifies the <see cref="CodePadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodePaddingProperty =
            DependencyProperty.Register(
                nameof(CodePadding),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(10, 6, 10, 6)));

        /// <summary>
        /// Gets or sets space between the code border and the text.
        /// </summary>
        public Thickness CodePadding
        {
            get => (Thickness)GetValue(CodePaddingProperty);
            set => SetValue(CodePaddingProperty, value);
        }

        #endregion

        #region CodeFontFamily

        /// <summary>
        /// Identifies the <see cref="CodeFontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeFontFamilyProperty =
            DependencyProperty.Register(
                nameof(CodeFontFamily),
                typeof(FontFamily),
                typeof(TextBlockEx),
                new PropertyMetadata(new FontFamily("Consolas")));

        /// <summary>
        /// Gets or sets the font used to display code.
        /// </summary>
        public FontFamily CodeFontFamily
        {
            get => (FontFamily)GetValue(CodeFontFamilyProperty);
            set => SetValue(CodeFontFamilyProperty, value);
        }

        #endregion

        #region InlineCodeBackground

        /// <summary>
        /// Identifies the <see cref="InlineCodeBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeBackgroundProperty =
            DependencyProperty.Register(
                nameof(InlineCodeBackground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the background brush for inline code.
        /// </summary>
        public Brush InlineCodeBackground
        {
            get => (Brush)GetValue(InlineCodeBackgroundProperty);
            set => SetValue(InlineCodeBackgroundProperty, value);
        }

        #endregion

        #region InlineCodeBorderBrush

        /// <summary>
        /// Identifies the <see cref="InlineCodeBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeBorderBrushProperty =
            DependencyProperty.Register(
                nameof(InlineCodeBorderBrush),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the border brush for inline code.
        /// </summary>
        public Brush InlineCodeBorderBrush
        {
            get => (Brush)GetValue(InlineCodeBorderBrushProperty);
            set => SetValue(InlineCodeBorderBrushProperty, value);
        }

        #endregion

        #region InlineCodeBorderThickness

        /// <summary>
        /// Identifies the <see cref="InlineCodeBorderThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(InlineCodeBorderThickness),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0)));

        /// <summary>
        /// Gets or sets the thickness of the border for inline code.
        /// </summary>
        public Thickness InlineCodeBorderThickness
        {
            get => (Thickness)GetValue(InlineCodeBorderThicknessProperty);
            set => SetValue(InlineCodeBorderThicknessProperty, value);
        }

        #endregion

        #region InlineCodeForeground

        /// <summary>
        /// Identifies the <see cref="InlineCodeForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeForegroundProperty =
            DependencyProperty.Register(
                nameof(InlineCodeForeground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for inline code.
        /// </summary>
        public Brush InlineCodeForeground
        {
            get => (Brush)GetValue(InlineCodeForegroundProperty);
            set => SetValue(InlineCodeForegroundProperty, value);
        }

        #endregion

        #region InlineCodeMargin

        /// <summary>
        /// Identifies the <see cref="InlineCodeMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeMarginProperty =
            DependencyProperty.Register(
                nameof(InlineCodeMargin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(2,0,2,-4)));

        /// <summary>
        /// Gets or sets the margin for inline code.
        /// </summary>
        public Thickness InlineCodeMargin
        {
            get => (Thickness)GetValue(InlineCodeMarginProperty);
            set => SetValue(InlineCodeMarginProperty, value);
        }

        #endregion

        #region InlineCodePadding

        /// <summary>
        /// Identifies the <see cref="InlineCodePadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodePaddingProperty =
            DependencyProperty.Register(
                nameof(InlineCodePadding),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(4,2,4,2)));

        /// <summary>
        /// Gets or sets the space between the code border and the text.
        /// </summary>
        public Thickness InlineCodePadding
        {
            get => (Thickness)GetValue(InlineCodePaddingProperty);
            set => SetValue(InlineCodePaddingProperty, value);
        }

        #endregion

        #region InlineCodeFontFamily

        /// <summary>
        /// Identifies the <see cref="InlineCodeFontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InlineCodeFontFamilyProperty =
            DependencyProperty.Register(
                nameof(InlineCodeFontFamily),
                typeof(FontFamily),
                typeof(TextBlockEx),
                new PropertyMetadata(new FontFamily("Consolas")));

        /// <summary>
        /// Gets or sets the font used to display code.
        /// </summary>
        public FontFamily InlineCodeFontFamily
        {
            get => (FontFamily)GetValue(InlineCodeFontFamilyProperty);
            set => SetValue(InlineCodeFontFamilyProperty, value);
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

        #region Header1FontWeight

        /// <summary>
        /// Identifies the <see cref="Header1FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header1FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header1FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Bold));

        /// <summary>
        /// Gets or sets the font weight to use for level 1 headers.
        /// </summary>
        public FontWeight Header1FontWeight
        {
            get => (FontWeight)GetValue(Header1FontWeightProperty);
            set => SetValue(Header1FontWeightProperty, value);
        }

        #endregion

        #region Header1FontSize

        /// <summary>
        /// Identifies the <see cref="Header1FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header1FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header1FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(20d));

        /// <summary>
        /// Gets or sets the font weight to use for level 1 headers.
        /// </summary>
        public double Header1FontSize
        {
            get => (double)GetValue(Header1FontSizeProperty);
            set => SetValue(Header1FontSizeProperty, value);
        }

        #endregion

        #region Header1Foreground

        /// <summary>
        /// Identifies the <see cref="Header1Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header1ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header1Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 1 headers.
        /// </summary>
        public Brush Header1Foreground
        {
            get => (Brush)GetValue(Header1ForegroundProperty);
            set => SetValue(Header1ForegroundProperty, value);
        }

        #endregion

        #region Header1Margin

        /// <summary>
        /// Identifies the <see cref="Header1Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header1MarginProperty =
            DependencyProperty.Register(
                nameof(Header1Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0,15,0,15)));

        /// <summary>
        /// Gets or sets the font weight to use for level 1 headers.
        /// </summary>
        public Thickness Header1Margin
        {
            get => (Thickness)GetValue(Header1MarginProperty);
            set => SetValue(Header1MarginProperty, value);
        }

        #endregion

        #region Header2FontWeight

        /// <summary>
        /// Identifies the <see cref="Header2FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header2FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header2FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Bold));

        /// <summary>
        /// Gets or sets the font weight to use for level 2 headers.
        /// </summary>
        public FontWeight Header2FontWeight
        {
            get => (FontWeight)GetValue(Header2FontWeightProperty);
            set => SetValue(Header2FontWeightProperty, value);
        }

        #endregion

        #region Header2FontSize

        /// <summary>
        /// Identifies the <see cref="Header2FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header2FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header2FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(20d));

        /// <summary>
        /// Gets or sets the font weight to use for level 2 headers.
        /// </summary>
        public double Header2FontSize
        {
            get => (double)GetValue(Header2FontSizeProperty);
            set => SetValue(Header2FontSizeProperty, value);
        }

        #endregion

        #region Header2Foreground

        /// <summary>
        /// Identifies the <see cref="Header2Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header2ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header2Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 2 headers.
        /// </summary>
        public Brush Header2Foreground
        {
            get => (Brush)GetValue(Header2ForegroundProperty);
            set => SetValue(Header2ForegroundProperty, value);
        }

        #endregion

        #region Header2Margin

        /// <summary>
        /// Identifies the <see cref="Header2Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header2MarginProperty =
            DependencyProperty.Register(
                nameof(Header2Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 15, 0, 15)));

        /// <summary>
        /// Gets or sets the font weight to use for level 2 headers.
        /// </summary>
        public Thickness Header2Margin
        {
            get => (Thickness)GetValue(Header2MarginProperty);
            set => SetValue(Header2MarginProperty, value);
        }

        #endregion

        #region Header3FontWeight

        /// <summary>
        /// Identifies the <see cref="Header3FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header3FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header3FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Bold));

        /// <summary>
        /// Gets or sets the font weight to use for level 3 headers.
        /// </summary>
        public FontWeight Header3FontWeight
        {
            get => (FontWeight)GetValue(Header3FontWeightProperty);
            set => SetValue(Header3FontWeightProperty, value);
        }

        #endregion

        #region Header3FontSize

        /// <summary>
        /// Identifies the <see cref="Header3FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header3FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header3FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(17d));

        /// <summary>
        /// Gets or sets the font weight to use for level 3 headers.
        /// </summary>
        public double Header3FontSize
        {
            get => (double)GetValue(Header3FontSizeProperty);
            set => SetValue(Header3FontSizeProperty, value);
        }

        #endregion

        #region Header3Foreground

        /// <summary>
        /// Identifies the <see cref="Header3Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header3ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header3Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 3 headers.
        /// </summary>
        public Brush Header3Foreground
        {
            get => (Brush)GetValue(Header3ForegroundProperty);
            set => SetValue(Header3ForegroundProperty, value);
        }

        #endregion

        #region Header3Margin

        /// <summary>
        /// Identifies the <see cref="Header3Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header3MarginProperty =
            DependencyProperty.Register(
                nameof(Header3Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 10, 0, 10)));

        /// <summary>
        /// Gets or sets the font weight to use for level 3 headers.
        /// </summary>
        public Thickness Header3Margin
        {
            get => (Thickness)GetValue(Header3MarginProperty);
            set => SetValue(Header3MarginProperty, value);
        }

        #endregion

        #region Header4FontWeight

        /// <summary>
        /// Identifies the <see cref="Header4FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header4FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header4FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Gets or sets the font weight to use for level 4 headers.
        /// </summary>
        public FontWeight Header4FontWeight
        {
            get => (FontWeight)GetValue(Header4FontWeightProperty);
            set => SetValue(Header4FontWeightProperty, value);
        }

        #endregion

        #region Header4FontSize

        /// <summary>
        /// Identifies the <see cref="Header4FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header4FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header4FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(17d));

        /// <summary>
        /// Gets or sets the font weight to use for level 4 headers.
        /// </summary>
        public double Header4FontSize
        {
            get => (double)GetValue(Header4FontSizeProperty);
            set => SetValue(Header4FontSizeProperty, value);
        }

        #endregion

        #region Header4Foreground

        /// <summary>
        /// Identifies the <see cref="Header4Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header4ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header4Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 4 headers.
        /// </summary>
        public Brush Header4Foreground
        {
            get => (Brush)GetValue(Header4ForegroundProperty);
            set => SetValue(Header4ForegroundProperty, value);
        }

        #endregion

        #region Header4Margin

        /// <summary>
        /// Identifies the <see cref="Header4Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header4MarginProperty =
            DependencyProperty.Register(
                nameof(Header4Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 10, 0, 10)));

        /// <summary>
        /// Gets or sets the font weight to use for level 4 headers.
        /// </summary>
        public Thickness Header4Margin
        {
            get => (Thickness)GetValue(Header4MarginProperty);
            set => SetValue(Header4MarginProperty, value);
        }

        #endregion

        #region Header5FontWeight

        /// <summary>
        /// Identifies the <see cref="Header5FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header5FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header5FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Bold));

        /// <summary>
        /// Gets or sets the font weight to use for level 5 headers.
        /// </summary>
        public FontWeight Header5FontWeight
        {
            get => (FontWeight)GetValue(Header5FontWeightProperty);
            set => SetValue(Header5FontWeightProperty, value);
        }

        #endregion

        #region Header5FontSize

        /// <summary>
        /// Identifies the <see cref="Header5FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header5FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header5FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(15d));

        /// <summary>
        /// Gets or sets the font weight to use for level 5 headers.
        /// </summary>
        public double Header5FontSize
        {
            get => (double)GetValue(Header5FontSizeProperty);
            set => SetValue(Header5FontSizeProperty, value);
        }

        #endregion

        #region Header5Foreground

        /// <summary>
        /// Identifies the <see cref="Header5Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header5ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header5Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 5 headers.
        /// </summary>
        public Brush Header5Foreground
        {
            get => (Brush)GetValue(Header5ForegroundProperty);
            set => SetValue(Header5ForegroundProperty, value);
        }

        #endregion

        #region Header5Margin

        /// <summary>
        /// Identifies the <see cref="Header5Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header5MarginProperty =
            DependencyProperty.Register(
                nameof(Header5Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 10, 0, 5)));

        /// <summary>
        /// Gets or sets the font weight to use for level 5 headers.
        /// </summary>
        public Thickness Header5Margin
        {
            get => (Thickness)GetValue(Header5MarginProperty);
            set => SetValue(Header5MarginProperty, value);
        }

        #endregion

        #region Header6FontWeight

        /// <summary>
        /// Identifies the <see cref="Header6FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header6FontWeightProperty =
            DependencyProperty.Register(
                nameof(Header6FontWeight),
                typeof(FontWeight),
                typeof(TextBlockEx),
                new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Gets or sets the font weight to use for level 6 headers.
        /// </summary>
        public FontWeight Header6FontWeight
        {
            get => (FontWeight)GetValue(Header6FontWeightProperty);
            set => SetValue(Header6FontWeightProperty, value);
        }

        #endregion

        #region Header6FontSize

        /// <summary>
        /// Identifies the <see cref="Header6FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header6FontSizeProperty =
            DependencyProperty.Register(
                nameof(Header6FontSize),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(15d));

        /// <summary>
        /// Gets or sets the font weight to use for level 6 headers.
        /// </summary>
        public double Header6FontSize
        {
            get => (double)GetValue(Header6FontSizeProperty);
            set => SetValue(Header6FontSizeProperty, value);
        }

        #endregion

        #region Header6Foreground

        /// <summary>
        /// Identifies the <see cref="Header6Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header6ForegroundProperty =
            DependencyProperty.Register(
                nameof(Header6Foreground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the foreground brush for level 6 headers.
        /// </summary>
        public Brush Header6Foreground
        {
            get => (Brush)GetValue(Header6ForegroundProperty);
            set => SetValue(Header6ForegroundProperty, value);
        }

        #endregion

        #region Header6Margin

        /// <summary>
        /// Identifies the <see cref="Header6Margin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Header6MarginProperty =
            DependencyProperty.Register(
                nameof(Header6Margin),
                typeof(Thickness),
                typeof(TextBlockEx),
                new PropertyMetadata(new Thickness(0, 10, 0, 0)));

        /// <summary>
        /// Gets or sets the font weight to use for level 6 headers.
        /// </summary>
        public Thickness Header6Margin
        {
            get => (Thickness)GetValue(Header6MarginProperty);
            set => SetValue(Header6MarginProperty, value);
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

        #region WrapCodeBlock

        /// <summary>
        /// Identifies the <see cref="WrapCodeBlock"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapCodeBlockProperty =
            DependencyProperty.Register(
                nameof(WrapCodeBlock),
                typeof(bool),
                typeof(TextBlockEx),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether to Wrap the Code Block or use a Horizontal Scroll.
        /// </summary>
        public bool WrapCodeBlock
        {
            get => (bool)GetValue(WrapCodeBlockProperty);
            set => SetValue(WrapCodeBlockProperty, value);
        }

        #endregion

        #region ContainerFontFamily

        /// <summary>
        /// Identifies the <see cref="ContainerFontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContainerFontFamilyProperty =
            DependencyProperty.Register(
                "ContainerFontFamily",
                typeof(FontFamily),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets the font used to display the text inside the container.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>The font used to display the text inside the container.</returns>
        public static FontFamily GetContainerFontFamily(DependencyObject control)
        {
            return (FontFamily)control.GetValue(ContainerFontFamilyProperty);
        }

        /// <summary>
        /// Sets the font used to display the text inside the container.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetContainerFontFamily(DependencyObject control, FontFamily value)
        {
            control.SetValue(ContainerFontFamilyProperty, value);
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
