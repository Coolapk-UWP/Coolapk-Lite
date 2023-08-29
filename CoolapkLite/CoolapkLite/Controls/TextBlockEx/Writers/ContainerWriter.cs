using HtmlAgilityPack;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    public class ContainerWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "div", "ul", "ol", "dl", "section", "article", "header", "footer", "main", "figure", "details", "summary", "tbody" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            HtmlNode node = fragment;
            if (node.Name.ToLowerInvariant() == "div" && node.GetAttributeValue("class", string.Empty) == "author-border")
            {
                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");
                InlineUIContainer container = new InlineUIContainer();

                Border border = new Border
                {
                    Margin = new Thickness(4, 0, 4, -4),
                    CornerRadius = new CornerRadius(4),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"],
                };
                TextBlock textBlock = new TextBlock
                {
                    FontSize = 12,
                    Margin = new Thickness(1),
                    IsTextSelectionEnabled = true,
                    Text = _loader.GetString("FeedAuthorText"),
                    Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"],
                };

                border.Child = textBlock;
                container.Child = border;
                return container;
            }
            else
            {
                return new Span();
            }
        }
    }
}
