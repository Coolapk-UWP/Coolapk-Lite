using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Converters;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Controls
{
    public partial class TextBlockEx
    {
        private ImmutableArray<ImageModel>.Builder imageArrayBuilder = ImmutableArray.CreateBuilder<ImageModel>();

        private Paragraph RenderNewRun(Paragraph paragraph, string item)
        {
            paragraph.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(item) });
            return paragraph;
        }

        private Paragraph RenderNewLine()
        {
            Paragraph paragraph = new Paragraph();
            RichTextBlock.Blocks.Add(paragraph);
            return paragraph;
        }

        private void RenderTextBlock()
        {
            if (RichTextBlock == null) { return; }

            RichTextBlock.Blocks.Clear();
            HtmlDocument doc = new HtmlDocument();
            Paragraph paragraph = RenderNewLine();
            doc.LoadHtml(Text.Replace("<!--break-->", string.Empty));

            HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
            paragraph = RenderHtmlNodes(paragraph, nodes);

            if (paragraph.Inlines.Count < 1) { RichTextBlock.Blocks.Remove(paragraph); }

            ImmutableArray<ImageModel> array = imageArrayBuilder.ToImmutable();
            foreach (ImageModel item in array)
            {
                item.ContextArray = array;
            }
            imageArrayBuilder.Clear();
        }

        private Paragraph RenderHtmlNodes(Paragraph paragraph, HtmlNodeCollection nodes)
        {
            foreach (HtmlNode node in nodes)
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Text:
                        paragraph= RenderText(paragraph, node);
                        break;
                    case HtmlNodeType.Element:
                        paragraph= RenderElement(paragraph, node);
                        break;
                }
            }
            return paragraph;
        }

        private Paragraph RenderText(Paragraph paragraph, HtmlNode node)
        {
            Regex emojis = new Regex(@"(\[\S*?\]|#\(\S*?\))");
            string[] list = emojis.Split(node.InnerText);
            foreach (string item in list)
            {
                if (string.IsNullOrEmpty(item)) { continue; }
                switch (item[0])
                {
                    case '#':
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
                                viewBox.SetBinding(WidthProperty, new Binding
                                {
                                    Path = new PropertyPath(nameof(FontSize)),
                                    Source = this,
                                    Converter = new NumMultConverter(),
                                    ConverterParameter = 4d / 3d,
                                    Mode = BindingMode.OneWay
                                });
                                container.Child = viewBox;
                                paragraph.Inlines.Add(container);
                            }
                            else
                            {
                                paragraph = RenderNewRun(paragraph, item);
                            }
                        }
                        break;
                    case '[':
                        {
                            if (SettingsHelper.Get<bool>("IsUseOldEmojiMode") && EmojiHelper.OldEmojis.Contains(item))
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
                                viewBox.SetBinding(WidthProperty, new Binding
                                {
                                    Path = new PropertyPath(nameof(FontSize)),
                                    Source = this,
                                    Converter = new NumMultConverter(),
                                    ConverterParameter = 4d / 3d,
                                    Mode = BindingMode.OneWay
                                });
                                container.Child = viewBox;
                                paragraph.Inlines.Add(container);
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
                                viewBox.SetBinding(WidthProperty, new Binding
                                {
                                    Path = new PropertyPath(nameof(FontSize)),
                                    Source = this,
                                    Converter = new NumMultConverter(),
                                    ConverterParameter = 4d / 3d,
                                    Mode = BindingMode.OneWay
                                });
                                container.Child = viewBox;
                                paragraph.Inlines.Add(container);
                            }
                            else
                            {
                                paragraph = RenderNewRun(paragraph, item);
                            }
                            break;
                        }
                    default:
                        paragraph = RenderNewRun(paragraph, item);
                        break;
                }
            }
            return paragraph;
        }

        private Paragraph RenderElement(Paragraph paragraph, HtmlNode node)
        {
            switch (node.OriginalName.ToLowerInvariant())
            {
                case "a":
                    paragraph = RenderLink(paragraph, node);
                    break;
                case "img":
                    paragraph = RenderImage(paragraph, node);
                    break;
                case "div":
                    paragraph = RenderBlock(paragraph, node);
                    break;
                case "b":
                case "strong":
                    Bold bold = new Bold();
                    bold.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(node.InnerText) });
                    paragraph.Inlines.Add(bold);
                    break;
                case "i":
                case "em":
                    Italic italic = new Italic();
                    italic.Inlines.Add(new Run { Text = WebUtility.HtmlDecode(node.InnerText) });
                    paragraph.Inlines.Add(italic);
                    break;
                case "p":
                    paragraph = paragraph.Inlines.Count > 0 ? RenderNewLine() : paragraph;
                    _ = RenderHtmlNodes(paragraph, node.ChildNodes);
                    paragraph = RenderNewLine();
                    break;
                case "br":
                    paragraph.Inlines.Add(new LineBreak());
                    if (node.ChildNodes.Count > 0)
                    {
                        paragraph = RenderHtmlNodes(paragraph, node.ChildNodes);
                        paragraph.Inlines.Add(new LineBreak());
                    }
                    break;
                default:
                    paragraph = RenderHtmlNodes(paragraph, node.ChildNodes);
                    break;
            }
            return paragraph;
        }

        private Paragraph RenderLink(Paragraph paragraph, HtmlNode node)
        {
            string content = node.InnerText;
            string tag = node.GetAttributeValue("t", string.Empty);
            string href = node.GetAttributeValue("href", string.Empty);
            string type = node.GetAttributeValue("type", string.Empty);
            Hyperlink hyperlink = new Hyperlink { UnderlineStyle = UnderlineStyle.None };
            if (!string.IsNullOrEmpty(href))
            {
                ToolTipService.SetToolTip(hyperlink, new ToolTip { Content = href });
                hyperlink.Click += (sender, e) =>
                {
                    if (LinkClicked == null)
                    {
                        _ = this.OpenLinkAsync(href);
                    }
                    else
                    {
                        LinkClicked.Invoke(this, href);
                    }
                };
            }
            if (!content.StartsWith("@") && !content.StartsWith("#") && !(type == "user-detail"))
            {
                Run run2 = new Run { Text = "\uE167", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                hyperlink.Inlines.Add(run2);
            }
            else if (content == "查看图片" && (href.IndexOf("http://image.coolapk.com", StringComparison.Ordinal) == 0 || href.IndexOf("https://image.coolapk.com", StringComparison.Ordinal) == 0))
            {
                content = "查看图片";
                Run run2 = new Run { Text = "\uE158", FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"] };
                hyperlink.Inlines.Add(run2);
            }
            Run run3 = new Run { Text = WebUtility.HtmlDecode(content) };
            hyperlink.Inlines.Add(run3);
            paragraph.Inlines.Add(hyperlink);
            return paragraph;
        }

        private Paragraph RenderImage(Paragraph paragraph, HtmlNode node)
        {
            string alt = node.GetAttributeValue("alt", string.Empty);
            string src = node.GetAttributeValue("src", string.Empty);
            int width = Convert.ToInt32(node.GetAttributeValue("width", "-1"));
            int height = Convert.ToInt32(node.GetAttributeValue("height", "-1"));

            if (string.IsNullOrEmpty(src))
            {
                if (!string.IsNullOrEmpty(alt))
                {
                    paragraph = RenderNewRun(paragraph, alt);
                }
                return paragraph;
            }

            ImageModel imageModel;
            Image image = new Image();
            InlineUIContainer container = new InlineUIContainer();

            imageModel = new ImageModel(src, SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture) ? ImageType.OriginImage : ImageType.SmallImage);
            image.SetBinding(Image.SourceProperty, new Binding
            {
                Path = new PropertyPath(nameof(imageModel.Pic)),
                Source = imageModel,
                Mode = BindingMode.OneWay
            });
            if (!string.IsNullOrEmpty(alt))
            {
                ToolTipService.SetToolTip(image, new ToolTip { Content = alt });
            }

            if (src.Contains("emoticons"))
            {
                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                viewBox.SetBinding(WidthProperty, new Binding
                {
                    Path = new PropertyPath(nameof(FontSize)),
                    Source = this,
                    Converter = new NumMultConverter(),
                    ConverterParameter = 4d / 3d,
                    Mode = BindingMode.OneWay
                });
                container.Child = viewBox;
                paragraph.Inlines.Add(container);
            }
            else
            {
                imageArrayBuilder.Add(imageModel);

                Grid Grid = new Grid { Padding = new Thickness(0, 0, 0, 8) };

                StackPanel IsGIFPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel PicSizePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                Border GIFBorder = new Border
                {
                    Child = new TextBlock
                    {
                        Text = _loader.GetString("GIF"),
                        Margin = new Thickness(2, 0, 2, 0),
                        Foreground = new SolidColorBrush(Colors.White)
                    },
                    Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                };
                GIFBorder.SetBinding(VisibilityProperty, new Binding
                {
                    Source = imageModel,
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    Path = new PropertyPath(nameof(imageModel.IsGif))
                });

                IsGIFPanel.Children.Add(GIFBorder);

                Border WidePicBorder = new Border
                {
                    Child = new TextBlock
                    {
                        Margin = new Thickness(2, 0, 2, 0),
                        Text = _loader.GetString("WidePicText"),
                        Foreground = new SolidColorBrush(Colors.White)
                    },
                    Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                };
                WidePicBorder.SetBinding(VisibilityProperty, new Binding
                {
                    Path = new PropertyPath(nameof(imageModel.IsWidePic)),
                    Source = imageModel,
                    Converter = new BoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay
                });

                Border LongPicTextBorder = new Border
                {
                    Child = new TextBlock
                    {
                        Margin = new Thickness(2, 0, 2, 0),
                        Text = _loader.GetString("LongPicText"),
                        Foreground = new SolidColorBrush(Colors.White)
                    },
                    Background = new SolidColorBrush(Color.FromArgb(255, 15, 157, 88))
                };
                LongPicTextBorder.SetBinding(VisibilityProperty, new Binding
                {
                    Path = new PropertyPath(nameof(imageModel.IsLongPic)),
                    Source = imageModel,
                    Converter = new BoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay
                });

                PicSizePanel.Children.Add(WidePicBorder);
                PicSizePanel.Children.Add(LongPicTextBorder);

                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };
                if (width != -1) { viewBox.MaxWidth = width; }
                if (height != -1) { viewBox.MaxHeight = height; }

                Grid.Children.Add(viewBox);
                Grid.Children.Add(PicSizePanel);
                Grid.Tapped += (sender, args) =>
                {
                    if (args.Handled) { return; }
                    if (ImageClicked == null)
                    {
                        _ = this.ShowImageAsync(imageModel);
                    }
                    else
                    {
                        ImageClicked.Invoke(this, imageModel);
                    }
                    args.Handled = true;
                };

                container.Child = Grid;

                paragraph = paragraph = paragraph.Inlines.Count > 0 ? RenderNewLine() : paragraph;
                paragraph.TextAlignment = TextAlignment.Center;
                paragraph.Inlines.Add(container);

                if (!string.IsNullOrEmpty(alt))
                {
                    Paragraph paragraph1 = new Paragraph
                    {
                        LineHeight = FontSize + 10,
                        TextAlignment = TextAlignment.Center,
                        Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                    };
                    Run run = new Run { Text = WebUtility.HtmlDecode(alt) };
                    paragraph1.Inlines.Add(run);
                    RichTextBlock.Blocks.Add(paragraph1);
                }

                paragraph = RenderNewLine();
            }
            return paragraph;
        }

        private Paragraph RenderBlock(Paragraph paragraph, HtmlNode node)
        {
            if (node.GetAttributeValue("class", string.Empty) == "author-border")
            {
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
                paragraph.Inlines.Add(container);
            }
            else
            {
                paragraph = RenderHtmlNodes(paragraph, node.ChildNodes);
            }
            return paragraph;
        }
    }
}
