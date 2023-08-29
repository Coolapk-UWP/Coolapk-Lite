using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class ParagraphWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "p" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            return new Paragraph();
        }
    }
}
