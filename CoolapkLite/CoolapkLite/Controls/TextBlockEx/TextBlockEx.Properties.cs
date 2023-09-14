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

        #region LineHeight

        /// <summary>
        /// Identifies the <see cref="LineHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register(
                nameof(LineHeight),
                typeof(double),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the height of each line of content.
        /// </summary>
        public double LineHeight
        {
            get => (double)GetValue(LineHeightProperty);
            private set => SetValue(LineHeightProperty, value);
        }

        #endregion

        #region LineSpacing

        /// <summary>
        /// Identifies the <see cref="LineSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LineSpacingProperty =
            DependencyProperty.Register(
                nameof(LineSpacing),
                typeof(double),
                typeof(TextBlockEx),
                new PropertyMetadata(0d, OnLineSpacingPropertyChanged));

        /// <summary>
        /// Gets or sets the spacing of each line of content.
        /// </summary>
        public double LineSpacing
        {
            get => (double)GetValue(LineSpacingProperty);
            set => SetValue(LineSpacingProperty, value);
        }

        private static void OnLineSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextBlockEx)d).UpdateLineHeight();
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
                new PropertyMetadata(new Thickness(2, 0, 2, -4)));

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
                new PropertyMetadata(new Thickness(4, 2, 4, 2)));

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

        #region LinkForeground

        /// <summary>
        /// Gets the dependency property for <see cref="LinkForeground"/>.
        /// </summary>
        public static readonly DependencyProperty LinkForegroundProperty =
            DependencyProperty.Register(
                nameof(LinkForeground),
                typeof(Brush),
                typeof(TextBlockEx),
                null);

        /// <summary>
        /// Gets or sets the brush used to render links.
        /// </summary>
        public Brush LinkForeground
        {
            get => (Brush)GetValue(LinkForegroundProperty);
            set => SetValue(LinkForegroundProperty, value);
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
