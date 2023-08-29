using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class SpanWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "span" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            return new Span();
        }
    }
}
