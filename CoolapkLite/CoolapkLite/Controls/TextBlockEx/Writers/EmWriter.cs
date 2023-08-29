using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class EmWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "em" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            return new Italic();
        }
    }
}
