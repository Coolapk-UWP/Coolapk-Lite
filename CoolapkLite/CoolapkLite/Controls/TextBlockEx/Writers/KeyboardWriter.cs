using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    public class KeyboardWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "kbd" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            // Avoid a crash if the current inline is inside an hyperlink.
            // This happens when using inline code blocks like [`SomeCode`](https://www.foo.bar).
            if (IsInline(fragment))
            {
                Span span = new Span();
                BindingOperations.SetBinding(span, TextElement.FontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeFontFamily)));
                BindingOperations.SetBinding(span, TextElement.ForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeForeground)));
                return span;
            }
            else
            {
                StackPanel border = new StackPanel
                {
                    // Aligns content in InlineUI, see https://social.msdn.microsoft.com/Forums/silverlight/en-US/48b5e91e-efc5-4768-8eaf-f897849fcf0b/richtextbox-inlineuicontainer-vertical-alignment-issue?forum=silverlightarchieve
                    RenderTransform = new TranslateTransform { Y = 4 }
                };
                border.SetBinding(StackPanel.BorderThicknessProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeBorderThickness)));
                border.SetBinding(StackPanel.BorderBrushProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeBorderBrush)));
                border.SetBinding(Panel.BackgroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeBackground)));
                border.SetBinding(StackPanel.PaddingProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodePadding)));
                border.SetBinding(FrameworkElement.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeMargin)));
                border.SetBinding(TextBlockEx.ContainerFontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.InlineCodeFontFamily)));
                return border;
            }
        }

        private static bool IsInline(HtmlNode fragment)
        {
            return fragment.ParentNode != null && fragment.ParentNode.Name == "a";
        }
    }
}
