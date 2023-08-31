using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Containers
{
    public class StackPanelDocumentContainer : DocumentContainer<StackPanel>
    {
        public StackPanelDocumentContainer(StackPanel element) : base(element)
        {
        }

        public override bool CanContain(DependencyObject element)
        {
            return element is Inline || element is Block || element is FrameworkElement;
        }

        protected override void Add(DependencyObject element)
        {
            if (element is FrameworkElement frameworkElement)
            {
                AddChild(frameworkElement);
            }
            else if (element is Block block)
            {
                RichTextBlock textBlock = FindOrCreateTextBlock(Control);
                textBlock.Blocks.Add(block);
            }
            else if (element is Inline inline)
            {
                RichTextBlock textBlock = FindOrCreateTextBlock(Control);
                Paragraph paragraph = FindOrCreateParagraph(textBlock);
                paragraph.Inlines.Add(inline);
            }
        }

        private static Paragraph FindOrCreateParagraph(RichTextBlock textBlock)
        {
            Paragraph paragraph = textBlock.Blocks.OfType<Paragraph>().LastOrDefault();
            if (paragraph == null)
            {
                paragraph = new Paragraph();
                textBlock.Blocks.Add(paragraph);
            }
            return paragraph;
        }

        private RichTextBlock FindOrCreateTextBlock(DependencyObject element)
        {
            if (!(Control.Children.LastOrDefault() is RichTextBlock textBlock))
            {
                textBlock = new RichTextBlock();

                if (FindAscendant<RichTextBlock>() is DocumentContainer<RichTextBlock> container)
                {
                    textBlock.SetBinding(RichTextBlock.IsTextSelectionEnabledProperty, CreateBinding(container.Control, nameof(container.Control.IsTextSelectionEnabled)));
                }

                if (element.GetValue(TextBlockEx.ContainerForegroundProperty) is Brush brush && brush != DependencyProperty.UnsetValue)
                {
                    long token = 0;
                    textBlock.Loaded += (sender, e) =>
                    token = SetBinding(element, TextBlockEx.ContainerForegroundProperty, () =>
                    textBlock.Foreground = TextBlockEx.GetContainerForeground(element));
                    textBlock.Unloaded += (sender, e) =>
                    UnsetBinding(element, TextBlockEx.ContainerForegroundProperty, token);
                }

                if (element.GetValue(TextBlockEx.ContainerFontFamilyProperty) is FontFamily fontFamily && fontFamily != DependencyProperty.UnsetValue)
                {
                    long token = 0;
                    textBlock.Loaded += (sender, e) =>
                    token = SetBinding(element, TextBlockEx.ContainerFontFamilyProperty, () =>
                    textBlock.FontFamily = TextBlockEx.GetContainerFontFamily(element));
                    textBlock.Unloaded += (sender, e) =>
                    UnsetBinding(element, TextBlockEx.ContainerFontFamilyProperty, token);
                }

                AddChild(textBlock);
            }
            return textBlock;
        }

        private void AddChild(FrameworkElement element)
        {
            Control.Children.Add(element);
        }
    }
}
