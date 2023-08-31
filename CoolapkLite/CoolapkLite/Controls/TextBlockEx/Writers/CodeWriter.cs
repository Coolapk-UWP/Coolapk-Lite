using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class CodeWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "code" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            Span span = new Span();
            BindingOperations.SetBinding(span, TextElement.FontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeFontFamily)));
            return span;
        }
    }
}
