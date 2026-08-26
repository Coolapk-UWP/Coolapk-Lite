using CoolapkLite.Helpers.Converters;
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
        public override string[] TargetTags => new[] { "div" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode node = fragment;
            if (node.GetAttributeValue("class", string.Empty) == "author-border")
            {
                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");
                InlineUIContainer container = new InlineUIContainer();

                Border border = new Border
                {
                    Margin = new Thickness(4, 0, 4, -4),
                    Padding = new Thickness(2, 0, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"]
                };

                TextBlock textBlock = new TextBlock
                {
                    Margin = new Thickness(1),
                    Text = _loader.GetString("FeedAuthorText"),
                    Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAltChromeWhiteBrush"]
                };

                textBlock.SetBinding(TextBlock.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.FontSize), new NumAddConverter(), -2));
                textBlock.SetBinding(TextBlock.IsTextSelectionEnabledProperty, CreateBinding(textBlockEx, nameof(textBlockEx.IsTextSelectionEnabled)));

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
