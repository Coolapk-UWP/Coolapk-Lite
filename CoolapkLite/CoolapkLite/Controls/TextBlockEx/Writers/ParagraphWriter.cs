using HtmlAgilityPack;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class ParagraphWriter : HtmlWriter
    {
        public override HashSet<string> TargetTags => new HashSet<string> { "p" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Paragraph();
        }
    }
}
