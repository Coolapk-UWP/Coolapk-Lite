using HtmlAgilityPack;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class HeaderWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "h1", "h2", "h3", "h4", "h5", "h6" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            Paragraph paragraph = new Paragraph();
            switch (fragment.Name.ToLowerInvariant())
            {
                case "h1":
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1Foreground)));
                    break;
                case "h2":
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2Foreground)));
                    break;
                case "h3":
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3Foreground)));
                    break;
                case "h4":
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header4Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header4FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header4FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header4Foreground)));
                    break;
                case "h5":
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header5Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header5FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header5FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header5Foreground)));
                    break;
                case "h6":
                    paragraph.TextDecorations = TextDecorations.Underline;
                    BindingOperations.SetBinding(paragraph, Block.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header6Margin)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header6FontSize)));
                    BindingOperations.SetBinding(paragraph, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header6FontWeight)));
                    BindingOperations.SetBinding(paragraph, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header6Foreground)));
                    break;
            }
            return paragraph;
        }
    }
}
