using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Controls.Writers
{
    public class TextWriter : HtmlWriter
    {
        public override string[] TargetTags => throw new NotImplementedException();

        public override bool Match(HtmlNode fragment)
        {
            return fragment.NodeType == HtmlNodeType.Text;
        }

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode text = fragment;
            if (text != null && !string.IsNullOrEmpty(text.InnerText))
            {
                Span span = new Span();
                if (textBlockEx.IsEnableMarkdown)
                {
                    using (StringReader reader = new StringReader(text.InnerText))
                    {
                        while (reader.ReadLine() is string line)
                        {
                            try
                            {
                                if (line.Length == 0)
                                {
                                    continue;
                                }
                                else if (line.Length < 2)
                                {
                                    goto fallback;
                                }
                                else
                                {
                                    if (line[0] == '#')
                                    {
                                        int level = 1;
                                        while (level < line.Length && line[level] == '#')
                                        {
                                            if (++level > 3)
                                            {
                                                break;
                                            }
                                        }
                                        if (level + 1 < line.Length && line[level] == ' ')
                                        {
                                            string headingText = line.Substring(level + 1);
                                            Span head = new Span();
                                            switch (level)
                                            {
                                                case 1:
                                                    BindingOperations.SetBinding(head, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontSize)));
                                                    BindingOperations.SetBinding(head, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header1FontWeight)));
                                                    break;
                                                case 2:
                                                    BindingOperations.SetBinding(head, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontSize)));
                                                    BindingOperations.SetBinding(head, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header2FontWeight)));
                                                    break;
                                                case 3:
                                                    BindingOperations.SetBinding(head, TextElement.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontSize)));
                                                    BindingOperations.SetBinding(head, TextElement.FontWeightProperty, CreateBinding(textBlockEx, nameof(textBlockEx.Header3FontWeight)));
                                                    break;
                                                default:
                                                    head = span;
                                                    break;
                                            }
                                            ParseEmoji(headingText, head);
                                            if (head != span && head.Inlines.Count > 0)
                                            {
                                                span.Inlines.Add(head);
                                            }
                                        }
                                        else
                                        {
                                            goto fallback;
                                        }
                                    }
                                    else
                                    {
                                        string[] list = Regex.Split(line, @"(\*\*.*\*\*)");
                                        foreach (string item in list)
                                        {
                                            if (item.StartsWith("**") && item.EndsWith("**"))
                                            {
                                                string boldText = item.Substring(2, item.Length - 4);
                                                Span boldSpan = new Span { FontWeight = FontWeights.Bold };
                                                ParseEmoji(boldText, boldSpan);
                                                if (boldSpan.Inlines.Count > 0) { span.Inlines.Add(boldSpan); }
                                            }
                                            else
                                            {
                                                ParseEmoji(item, span);
                                            }
                                        }
                                    }
                                }
                                continue;
                            fallback:
                                span.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(line) });
                            }
                            finally
                            {
                                if (reader.Peek() != -1)
                                {
                                    span.Inlines.Add(new LineBreak());
                                }
                            }
                        }
                    }
                }
                else
                {
                    ParseEmoji(text.InnerText, span);
                }
                if (span.Inlines.Count > 0) { return span; }
            }
            return null;
        }

        private void ParseEmoji(string line, Span span)
        {
            string[] list = Regex.Split(line, @"(\[\S*?\]|#\(\S*?\))");
            foreach (string item in list)
            {
                if (GetInline(item) is Inline inline)
                {
                    span.Inlines.Add(inline);
                }
            }
        }

        private Inline GetInline(string item)
        {
            if (string.IsNullOrEmpty(item)) { return null; }
            switch (item[0])
            {
                case '[':
                    return GetEmoji(item);
                case '#':
                    return item.Length > 2 && item[1] == '('
                        ? GetOldEmoji(item)
                        : new Run { Text = WebUtility.HtmlDecode(item) };
                default:
                    return new Run { Text = WebUtility.HtmlDecode(item) };
            }
        }

        private Inline GetOldEmoji(string item)
        {
            string str = item.Substring(1);
            if (EmojiHelper.Emojis.Contains(str))
            {
                InlineUIContainer container = new InlineUIContainer();
                Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{str}.png")) };
                ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                viewBox.SetBinding(FrameworkElement.WidthProperty, CreateBinding(container, nameof(container.FontSize), new NumMultConverter(), 4d / 3d));
                container.Child = viewBox;
                return container;
            }
            else
            {
                return new Run { Text = WebUtility.HtmlDecode(item) };
            }
        }

        private Inline GetEmoji(string item)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsUseOldEmojiMode) && EmojiHelper.OldEmojis.Contains(item))
            {
                InlineUIContainer container = new InlineUIContainer();
                Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                viewBox.SetBinding(FrameworkElement.WidthProperty, CreateBinding(container, nameof(container.FontSize), new NumMultConverter(), 4d / 3d));
                container.Child = viewBox;
                return container;
            }
            else if (EmojiHelper.Emojis.Contains(item))
            {
                InlineUIContainer container = new InlineUIContainer();
                Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                viewBox.SetBinding(FrameworkElement.WidthProperty, CreateBinding(container, nameof(container.FontSize), new NumMultConverter(), 4d / 3d));
                container.Child = viewBox;
                return container;
            }
            else
            {
                return new Run { Text = WebUtility.HtmlDecode(item) };
            }
        }
    }
}
