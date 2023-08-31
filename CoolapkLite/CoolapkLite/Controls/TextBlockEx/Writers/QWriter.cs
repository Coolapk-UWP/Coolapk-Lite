using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class QWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "q" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode node = fragment;
            if (node != null)
            {
                fragment.PrependChild(HtmlNode.CreateNode("\""));
                fragment.AppendChild(HtmlNode.CreateNode("\""));
            }
            return new Span();
        }
    }
}
