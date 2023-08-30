using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls.Writers
{
    public class BlockQuoteWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "blockquote" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.SetBinding(FrameworkElement.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuoteMargin)));
            stackPanel.SetBinding(Panel.BackgroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuoteBackground)));
            stackPanel.SetBinding(StackPanel.BorderBrushProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuoteBorderBrush)));
            stackPanel.SetBinding(StackPanel.BorderThicknessProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuoteBorderThickness)));
            stackPanel.SetBinding(StackPanel.PaddingProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuotePadding)));
            stackPanel.SetBinding(TextBlockEx.ContainerForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.QuoteForeground)));
            return stackPanel;
        }
    }
}
