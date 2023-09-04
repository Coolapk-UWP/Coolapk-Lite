using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class InsWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "ins", "u" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Underline();
        }
    }
}
