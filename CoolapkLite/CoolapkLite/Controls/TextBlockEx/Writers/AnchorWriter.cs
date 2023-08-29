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

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            if (fragment.NodeType == HtmlNodeType.Element)
            {
                HtmlNode node = fragment;
                if (node != null)
                {
                    Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };

                    string content = node.InnerText;
                    if (content == "查看图片")
                    {
                        Run run = new Run { Text = "\uE158", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                        hyperlink.Inlines.Add(run);
                    }
                    else if (!content.StartsWith("@") && !content.StartsWith("#") && !(node.GetAttributeValue("type", null) == "user-detail"))
                    {
                        Run run = new Run { Text = "\uE167", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                        hyperlink.Inlines.Add(run);
                    }

                    if (node.GetAttributeValue("href", null) is string href && !string.IsNullOrEmpty(href))
                    {
                        ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                        hyperlink.Click += (sender, e) => _ = hyperlink.OpenLinkAsync(href);
                    }

                    return hyperlink;
                }
            }
            return null;
        }
    }
}
