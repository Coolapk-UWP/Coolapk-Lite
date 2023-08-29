using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class IWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "i" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            return new Italic();
        }
    }
}
