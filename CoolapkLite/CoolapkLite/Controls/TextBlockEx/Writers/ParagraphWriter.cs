using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public sealed class ParagraphWriter : HtmlWriter
    {
        public override string[] TargetTags => new[] { "p" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Paragraph();
        }
    }
}
