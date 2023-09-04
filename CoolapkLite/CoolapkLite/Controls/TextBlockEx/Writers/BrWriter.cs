using HtmlAgilityPack;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class BrWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "br" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            //LineBreak doesn't work with hyperlink
            return new Run
            {
                Text = Environment.NewLine
            };
        }
    }
}
