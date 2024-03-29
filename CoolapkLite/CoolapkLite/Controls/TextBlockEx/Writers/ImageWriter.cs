﻿using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Converters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    public class ImageWriter : HtmlWriter
    {
        public override HashSet<string> TargetTags => new HashSet<string> { "img" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode node = fragment;
            string src = GetImageSrc(node);
            if (node != null && !string.IsNullOrEmpty(src))
            {
                try
                {
                    return CreateImage(node, src, textBlockEx);
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

        private static DependencyObject CreateImage(HtmlNode node, string src, TextBlockEx textBlockEx)
        {
            ImageModel imageModel;
            ImageControl image = new ImageControl { EnableLazyLoading = false };

            imageModel = new ImageModel(src, SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture) ? ImageType.OriginImage : ImageType.SmallImage);
            image.Source = imageModel;

            string alt = node.GetAttributeValue("alt", string.Empty);
            if (!string.IsNullOrEmpty(alt))
            {
                ToolTipService.SetToolTip(image, new ToolTip { Content = alt });
            }

            if (src.Contains("emoticons", StringComparison.OrdinalIgnoreCase))
            {
                InlineUIContainer container = new InlineUIContainer();
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
                textBlockEx.ImageArrayBuilder.Add(imageModel);
                image.Tapped += (sender, args) => textBlockEx.OnImageClicked(imageModel, args);

                Grid Grid = new Grid { Padding = new Thickness(0, 12, 0, 12) };

                UIElementHelper.SetContextFlyout(Grid, CreateMenuFlyout(imageModel, textBlockEx));

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
                GIFBorder.SetBinding(UIElement.VisibilityProperty, CreateBinding(imageModel, nameof(imageModel.IsGif), new BoolToVisibilityConverter()));

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
                WidePicBorder.SetBinding(UIElement.VisibilityProperty, CreateBinding(imageModel, nameof(imageModel.IsWidePic), new BoolToVisibilityConverter()));

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
                LongPicTextBorder.SetBinding(UIElement.VisibilityProperty, CreateBinding(imageModel, nameof(imageModel.IsLongPic), new BoolToVisibilityConverter()));

                PicSizePanel.Children.Add(WidePicBorder);
                PicSizePanel.Children.Add(LongPicTextBorder);

                Viewbox viewBox = new Viewbox
                {
                    Child = image,
                    Margin = new Thickness(0, 0, 0, -4),
                    VerticalAlignment = VerticalAlignment.Center
                };

                double imgHeight = GetAttributeValue(node, "height", 0);
                double width = GetAttributeValue(node, "width", 0);

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

                if (!string.IsNullOrEmpty(alt))
                {
                    Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = WebUtility.HtmlDecode(alt),
                        Margin = new Thickness(0, 6, 0, 0),
                        TextAlignment = TextAlignment.Center,
                        Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                    };
                    textBlock.SetBinding(TextBlock.IsTextSelectionEnabledProperty, CreateBinding(textBlockEx, nameof(textBlockEx.IsTextSelectionEnabled)));
                    textBlock.SetBinding(TextBlock.FontSizeProperty, CreateBinding(textBlockEx, nameof(textBlockEx.FontSize)));
                    textBlock.SetValue(Grid.RowProperty, 1);
                    Grid.Children.Add(textBlock);
                }

                return Grid;
            }
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

        private static MenuFlyout CreateMenuFlyout(ImageModel image, TextBlockEx textBlockEx)
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            MenuFlyoutItem refreshButton = new MenuFlyoutItem { Text = "刷新" };
            FlyoutBaseHelper.SetIcon(refreshButton, CreateFontIcon("\uE72C", textBlockEx));
            refreshButton.Click += (sender, args) => _ = image.Refresh(refreshButton.Dispatcher);
            menuFlyout.Items.Add(refreshButton);

            MenuFlyoutItem copyButton = new MenuFlyoutItem { Text = "复制" };
            FlyoutBaseHelper.SetIcon(copyButton, CreateFontIcon("\uE8C8", textBlockEx));
            copyButton.Click += (sender, args) => image.CopyPic();
            menuFlyout.Items.Add(copyButton);

            MenuFlyoutItem shareButton = new MenuFlyoutItem { Text = "分享" };
            FlyoutBaseHelper.SetIcon(shareButton, CreateFontIcon("\uE72D", textBlockEx));
            shareButton.Click += (sender, args) => image.SharePic();
            menuFlyout.Items.Add(shareButton);

            MenuFlyoutItem saveButton = new MenuFlyoutItem { Text = "保存" };
            FlyoutBaseHelper.SetIcon(saveButton, CreateFontIcon("\uE74E", textBlockEx));
            saveButton.Click += (sender, args) => image.SavePic();
            menuFlyout.Items.Add(saveButton);

            MenuFlyoutItem originButton = new MenuFlyoutItem { Text = "查看原图" };
            FlyoutBaseHelper.SetIcon(originButton, CreateFontIcon("\uEB9F", textBlockEx));
            originButton.Click += (sender, args) => image.Type &= (ImageType)0xFE;
            menuFlyout.Items.Add(originButton);

            return menuFlyout;
        }
    }
}
