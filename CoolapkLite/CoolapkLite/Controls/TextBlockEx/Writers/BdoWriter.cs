using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls.Writers
{
    public class BdoWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "bdo" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            RichTextBlock richTextBlock = new RichTextBlock();
            HtmlNode node = fragment;
            if (node.GetAttributeValue("dir", null) is string dir)
            {
                switch (dir)
                {
                    case "ltr":
                        richTextBlock.FlowDirection = FlowDirection.LeftToRight;
                        break;
                    case "rtl":
                        richTextBlock.FlowDirection = FlowDirection.RightToLeft;
                        break;
                }
            }
            return richTextBlock;
        }
    }
}
