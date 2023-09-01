using CoolapkLite.Helpers;
using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    public class AnchorWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "a" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode node = fragment;
            if (node != null)
            {
                Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };

                string content = node.InnerText;
                if (content == "查看图片")
                {
                    hyperlink.Inlines.Add(CreateSymbolRun("\uEB9F", textBlockEx));
                }
                else if (!content.StartsWith("@") && !content.StartsWith("#") && !(node.GetAttributeValue("type", null) == "user-detail"))
                {
                    hyperlink.Inlines.Add(CreateSymbolRun("\uE167", textBlockEx));
                }

                if (node.GetAttributeValue("href", null) is string href && !string.IsNullOrEmpty(href))
                {
                    ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                    hyperlink.Click += (sender, e) => textBlockEx.OnLinkClicked(href);
                }

                return hyperlink;
            }
            return null;
        }
    }
}
