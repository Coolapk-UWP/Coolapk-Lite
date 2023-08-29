using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Converters;
using System;
using System.Net;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    public class ImageWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "img" };

        public override DependencyObject GetControl(HtmlNode fragment)
        {
            HtmlNode node = fragment;
            string src = GetImageSrc(node);
            if (node != null && !string.IsNullOrEmpty(src))
            {
                try
                {
                    return CreateImage(node, src);
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(ImageWriter)).Error(ex.ExceptionToMessage(), ex);
                }
            }
            return null;
        }

        private static bool IsInline(HtmlNode fragment)
        {
            return fragment.ParentNode != null && fragment.ParentNode.Name == "p";
        }

        private static DependencyObject CreateImage(HtmlNode node, string src)
        {
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

            string alt = node.GetAttributeValue("alt", string.Empty);
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
                viewBox.SetBinding(FrameworkElement.WidthProperty, new Binding
                {
                    Path = new PropertyPath(nameof(container.FontSize)),
                    Source = container,
                    Converter = new NumMultConverter(),
                    ConverterParameter = 4d / 3d,
                    Mode = BindingMode.OneWay
                });
                container.Child = viewBox;
            }
            else
            {
                Grid Grid = new Grid { Padding = new Thickness(0, 12, 0, 12) };

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

                ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");

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
                GIFBorder.SetBinding(UIElement.VisibilityProperty, new Binding
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
                WidePicBorder.SetBinding(UIElement.VisibilityProperty, new Binding
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
                LongPicTextBorder.SetBinding(UIElement.VisibilityProperty, new Binding
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

                int imgHeight = node.GetAttributeValue("height", 0);
                int width = node.GetAttributeValue("width", 0);

                if (imgHeight > 0)
                {
                    viewBox.MaxHeight = imgHeight;
                }
                if (width > 0)
                {
                    viewBox.MaxWidth = width;
                }

                Grid.Children.Add(viewBox);
                Grid.Children.Add(PicSizePanel);
                Grid.Tapped += (sender, args) =>
                {
                    if (args.Handled) { return; }
                    _ = Grid.ShowImageAsync(imageModel);
                    args.Handled = true;
                };

                if (!string.IsNullOrEmpty(alt))
                {
                    Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    TextBlock textBlock = new TextBlock
                    {
                        IsTextSelectionEnabled = true,
                        Text = WebUtility.HtmlDecode(alt),
                        Margin = new Thickness(0, 6, 0, 0),
                        TextAlignment = TextAlignment.Center,
                        Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                    };
                    textBlock.SetValue(Grid.RowProperty, 1);
                    Grid.Children.Add(textBlock);
                }

                container.Child = Grid;
            }
            return container;
        }

        private static string GetImageSrc(HtmlNode img)
        {
            if (img.GetAttributeValue("src", null) is string src && !string.IsNullOrEmpty(src))
            {
                return src;
            }
            else if (img.GetAttributeValue("srcset", null) is string srcset && !string.IsNullOrEmpty(srcset))
            {
                Regex regex = new Regex(@"(?:(?<src>[^\""'\s,]+)\s*(?:\s+\d+[wx])(?:,\s*)?)");
                MatchCollection matches = regex.Matches(srcset);

                if (matches.Count > 0)
                {
                    Match m = matches[matches.Count - 1];
                    return m?.Groups["src"].Value;
                }
            }
            return null;
        }
    }
}
