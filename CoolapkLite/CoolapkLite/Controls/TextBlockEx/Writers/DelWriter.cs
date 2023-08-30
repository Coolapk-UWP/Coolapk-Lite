using HtmlAgilityPack;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml;
using Windows.UI.Text;

namespace CoolapkLite.Controls.Writers
{
    public class DelWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "del", "s", "strike" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Span { TextDecorations = TextDecorations.Strikethrough };
        }
    }
}
