using HtmlAgilityPack;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls.Writers
{
    public class PreWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "pre" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollMode = ScrollMode.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            scrollViewer.SetBinding(Control.BackgroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeBackground)));
            scrollViewer.SetBinding(Control.BorderBrushProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeBorderBrush)));
            scrollViewer.SetBinding(Control.BorderThicknessProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeBorderThickness)));
            scrollViewer.SetBinding(Control.PaddingProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodePadding)));
            scrollViewer.SetBinding(FrameworkElement.MarginProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeMargin)));
            scrollViewer.SetBinding(TextBlockEx.ContainerForegroundProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeForeground)));
            scrollViewer.SetBinding(TextBlockEx.ContainerFontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.CodeFontFamily)));

            long token = 0;
            scrollViewer.Loaded += (sender, e) =>
            token = SetBinding(textBlockEx, TextBlockEx.WrapCodeBlockProperty, () =>
            {
                if (textBlockEx.WrapCodeBlock)
                {
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                }
                else
                {
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.HorizontalScrollMode = ScrollMode.Auto;
                }
            });
            scrollViewer.Unloaded += (sender, e) => UnsetBinding(textBlockEx, TextBlockEx.WrapCodeBlockProperty, token);

            return scrollViewer;
        }
    }
}
