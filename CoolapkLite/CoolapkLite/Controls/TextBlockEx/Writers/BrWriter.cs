using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class BrWriter : HtmlWriter
    {
        public override HashSet<string> TargetTags => new HashSet<string> { "br" };

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
