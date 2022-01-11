using CoolapkLite.Helpers;
using CoolapkLite.Helpers.ValueConverters;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    /// <summary>
    /// 基于 Cyenoch 的 MyRichTextBlock 控件和 Coolapk UWP 项目的 TextBlockEx 控件重制
    /// </summary>
    public sealed partial class TextBlockEx : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(TextBlockEx),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged))
        );

        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
            "MaxLines",
            typeof(int),
            typeof(TextBlockEx),
            null
        );

        public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyProperty.Register(
            "IsTextSelectionEnabled",
            typeof(bool),
            typeof(TextBlockEx),
            new PropertyMetadata(false, null)
        );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public bool IsTextSelectionEnabled
        {
            get => (bool)GetValue(IsTextSelectionEnabledProperty);
            set => SetValue(IsTextSelectionEnabledProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextBlockEx).GetTextBlock();
        }

        public TextBlockEx() => InitializeComponent();

        private void GetTextBlock()
        {
            RichTextBlock.Blocks.Clear();
            Paragraph paragraph = new Paragraph();
            HtmlDocument doc = new HtmlDocument();
            Regex emojis = new Regex(@"(\[\S*?\]|#\(\S*?\))");
            doc.LoadHtml(Text.Replace("<!--break-->", string.Empty));
            void NewLine()
            {
                RichTextBlock.Blocks.Add(paragraph);
                paragraph = new Paragraph();
            }
            void AddText(string item) => paragraph.Inlines.Add(new Run() { Text = WebUtility.HtmlDecode(item) });
            HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
            foreach (HtmlNode node in nodes)
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Text:
                        string[] list = emojis.Split(node.InnerText);
                        foreach (string item in list)
                        {
                            if (string.IsNullOrEmpty(item)) { continue; }
                            switch (item[0])
                            {
                                case '#':
                                    {
                                        string s = item.Substring(1, item.Length - 2);
                                        if (EmojiHelper.emojis.Contains(s))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToLineHeight(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else { AddText(item); }
                                    }
                                    break;
                                case '[':
                                    {
                                        if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.oldEmojis.Contains(item))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToLineHeight(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else if (EmojiHelper.emojis.Contains(item))
                                        {
                                            InlineUIContainer container = new InlineUIContainer();
                                            Image image = new Image { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png")) };
                                            ToolTipService.SetToolTip(image, new ToolTip { Content = item });
                                            Viewbox viewbox = new Viewbox
                                            {
                                                Child = image,
                                                Margin = new Thickness(0, 0, 0, -4),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            viewbox.SetBinding(WidthProperty, new Binding
                                            {
                                                Source = this,
                                                Mode = BindingMode.OneWay,
                                                Converter = new FontSizeToLineHeight(),
                                                Path = new PropertyPath(nameof(FontSize))
                                            });
                                            container.Child = viewbox;
                                            paragraph.Inlines.Add(container);
                                        }
                                        else { AddText(item); }
                                        break;
                                    }
                                default: AddText(item); break;
                            }
                        }
                        break;
                    case HtmlNodeType.Element:
                        string content = node.InnerText;
                        if (node.OriginalName == "a")
                        {
                            string tag = node.GetAttributeValue("t", string.Empty);
                            string href = node.GetAttributeValue("href", string.Empty);
                            string type = node.GetAttributeValue("type", string.Empty);
                            Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
                            if (!string.IsNullOrEmpty(href))
                            {
                                ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                            }
                            if (content.IndexOf('@') != 0 && content.IndexOf('#') != 0 && !(type == "user-detail"))
                            {
                                Run run2 = new Run { Text = "\uE167", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                hyperlink.Inlines.Add(run2);
                            }
                            else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
                            {
                                content = "查看图片";
                                Run run2 = new Run { Text = "\uE158", FontFamily = new FontFamily("Segoe MDL2 Assets") };
                                hyperlink.Inlines.Add(run2);
                            }
                            else
                            {
                                hyperlink.Click += (sender, e) => UIHelper.OpenLinkAsync(href);
                            }
                            Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
                            hyperlink.Inlines.Add(run3);
                            paragraph.Inlines.Add(hyperlink);
                        }
                        else if (node.OriginalName == "img")
                        {
                            string alt = node.GetAttributeValue("alt", string.Empty);
                            string src = node.GetAttributeValue("src", string.Empty);
                            int width = Convert.ToInt32(node.GetAttributeValue("width", "-1").Replace("\"", string.Empty));
                            int height = Convert.ToInt32(node.GetAttributeValue("height", "-1").Replace("\"", string.Empty));

                            Image image = new Image();
                            InlineUIContainer container = new InlineUIContainer();

                            if (!string.IsNullOrEmpty(src))
                            {
                                ImageModel imageModel = new ImageModel(src, ImageType.OriginImage);
                                image.SetBinding(Image.SourceProperty, new Binding
                                {
                                    Source = imageModel,
                                    Mode = BindingMode.OneWay,
                                    Path = new PropertyPath(nameof(imageModel.Pic))
                                });
                                ToolTipService.SetToolTip(image, new ToolTip { Content = string.IsNullOrEmpty(alt) ? content : alt });
                            }

                            if (src.Contains("emoticons"))
                            {
                                Viewbox viewbox = new Viewbox
                                {
                                    Child = image,
                                    Margin = new Thickness(0, 0, 0, -4),
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                viewbox.SetBinding(WidthProperty, new Binding
                                {
                                    Source = this,
                                    Mode = BindingMode.OneWay,
                                    Converter = new FontSizeToLineHeight(),
                                    Path = new PropertyPath(nameof(FontSize))
                                });
                                container.Child = viewbox;
                            }
                            else
                            {
                                Viewbox viewbox = new Viewbox
                                {
                                    Child = image,
                                    Margin = new Thickness(0, 0, 0, -4),
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                if (width != -1) { viewbox.MaxWidth = width; }
                                if (height != -1) { viewbox.MaxHeight = height; }
                                container.Child = viewbox;
                            }
                            paragraph.Inlines.Add(container);
                        }
                        break;
                }
            }
            RichTextBlock.Blocks.Add(paragraph);
            Content = RichTextBlock;
        }
    }
}
