using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
                string[] list = Regex.Split(text.InnerText, @"(\[\S*?\]|#\(\S*?\))");
                Span span = new Span();
                foreach (string item in list)
                {
                    if (GetInline(item) is Inline inline)
                    {
                        span.Inlines.Add(inline);
                    }
                }
                if (span.Inlines.Count > 0) { return span; }
            }
            return null;
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
