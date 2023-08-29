using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class StrongWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "strong", "b" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            return new Bold();
        }
    }
}
