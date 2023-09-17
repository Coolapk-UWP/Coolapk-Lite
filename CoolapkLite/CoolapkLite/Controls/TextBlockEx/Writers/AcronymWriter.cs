using HtmlAgilityPack;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public class AcronymWriter : HtmlWriter
    {
        public override HashSet<string> TargetTags => new HashSet<string> { "acronym", "abbr" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            Hyperlink hyperlink = new Hyperlink
            {
                IsTabStop = false
            };
            BindingOperations.SetBinding(hyperlink, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Foreground)));
            HtmlNode node = fragment;
            if (node.GetAttributeValue("title", null) is string title)
            {
                ToolTipService.SetToolTip(hyperlink, title);
            }
            return hyperlink;
        }
    }
}
