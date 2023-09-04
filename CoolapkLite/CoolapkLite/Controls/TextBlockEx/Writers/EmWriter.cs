using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class EmWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "em", "i", "dfn", "var", "cite", "address" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Italic();
        }
    }
}
