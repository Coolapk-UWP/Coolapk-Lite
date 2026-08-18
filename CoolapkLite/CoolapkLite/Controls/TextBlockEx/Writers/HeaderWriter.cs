using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class HeaderWriter : HtmlWriter
    {
        public override string[] TargetTags => new[] { "h1", "h2", "h3" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            Span span = new Span();
            switch (fragment.Name[fragment.Name.Length - 1])
            {
                case '1':
                    BindingOperations.SetBinding(span, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontSize)));
                    BindingOperations.SetBinding(span, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontWeight)));
                    break;
                case '2':
                    BindingOperations.SetBinding(span, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontSize)));
                    BindingOperations.SetBinding(span, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontWeight)));
                    break;
                case '3':
                    BindingOperations.SetBinding(span, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontSize)));
                    BindingOperations.SetBinding(span, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontWeight)));
                    break;
            }
            return span;
        }
    }
}
