using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public sealed class StrongWriter : HtmlWriter
    {
        public override string[] TargetTags => new[] { "strong" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            return new Bold();
        }
    }
}
