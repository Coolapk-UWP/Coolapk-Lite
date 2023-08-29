using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Containers
{
    public class RichTextBlockDocumentContainer : DocumentContainer<RichTextBlock>
    {
        public RichTextBlockDocumentContainer(RichTextBlock element) : base(element)
        {
        }

        public override bool CanContain(DependencyObject element)
        {
            return element is Inline || element is Block || element is UIElement;
        }

        protected override void Add(DependencyObject element)
        {
            if (element is UIElement)
            {
                AddChild(element as UIElement);
            }
            else if (element is Block)
            {
                Control.Blocks.Add(element as Block);
            }
            else if (element is Inline)
            {
                Paragraph paragraph = FindOrCreateParagraph(Control);
                paragraph.Inlines.Add(element as Inline);
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

        private void AddChild(UIElement element)
        {
            InlineUIContainer container = new InlineUIContainer
            {
                Child = element
            };
            Paragraph paragraph = FindOrCreateParagraph(Control);
            paragraph.Inlines.Add(container);
        }
    }
}
