using HtmlAgilityPack;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

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
