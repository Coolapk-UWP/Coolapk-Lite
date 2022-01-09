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
        private const string AuthorBorder = "<div class=\"author-border\"></div>";

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(TextBlockEx),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged))
        );

        /// <summary>
        /// 用于Hyperlink传递href参数
        /// </summary>
        public static readonly DependencyProperty HrefClickParam = DependencyProperty.Register(
            "HrefParam",
            typeof(string),
            typeof(Hyperlink),
            null
        );

        public event RoutedEventHandler LinkClicked;
        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args) => LinkClicked?.Invoke(sender, args);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlockEx rtb = d as TextBlockEx;
            rtb.GetTextBlock();
        }

        public TextBlockEx() => InitializeComponent();

        private void GetTextBlock()
        {
            Paragraph paragraph = new Paragraph();
            HtmlDocument doc = new HtmlDocument();
            RichTextBlock block = new RichTextBlock();
            Regex emojis = new Regex(@"(\[\S*?\]|#\(\S*?\))");
            doc.LoadHtml(Text.Replace("<!--break-->", string.Empty));
            void NewLine()
            {
                block.Blocks.Add(paragraph);
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
                            if (string.IsNullOrEmpty(item)) { break; }
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
                        if (node.OriginalName == "a")
                        {
                            Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
                            string href = node.GetAttributeValue("href", string.Empty);
                            string type = node.GetAttributeValue("type", string.Empty);
                            string content = node.InnerText;
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
                            else if (href.Contains("emoticons") && (href.EndsWith(".png") || href.EndsWith(".jpg") || href.EndsWith(".jpeg") || href.EndsWith(".gif") || href.EndsWith(".bmp") || href.EndsWith(".PNG") || href.EndsWith(".JPG") || href.EndsWith(".JPEG") || href.EndsWith(".GIF") || href.EndsWith(".BMP")))
                            {
                                InlineUIContainer container = new InlineUIContainer();
                                ImageModel imageModel = new ImageModel(href, ImageType.OriginImage);

                                Image image = new Image();
                                image.SetBinding(Image.SourceProperty, new Binding
                                {
                                    Source = imageModel,
                                    Mode = BindingMode.OneWay,
                                    Path = new PropertyPath(nameof(imageModel.Pic))
                                });

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

                                ToolTipService.SetToolTip(image, new ToolTip { Content = content });
                                container.Child = viewbox;
                                paragraph.Inlines.Add(container);
                            }
                            Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
                            hyperlink.Inlines.Add(run3);
                            hyperlink.Click += Hyperlink_Click;
                            hyperlink.SetValue(HrefClickParam, href);
                            paragraph.Inlines.Add(hyperlink);
                        }
                        break;
                }
            }
            block.Blocks.Add(paragraph);
            Content = block;
        }
    }
}
