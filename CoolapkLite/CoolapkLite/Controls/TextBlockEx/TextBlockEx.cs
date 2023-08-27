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
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    /// <summary>
    /// 基于 Cyenoch 的 MyRichTextBlock 控件和 Coolapk UWP 项目的 TextBlockEx 控件重制
    /// </summary>
    [ContentProperty(Name = nameof(Text))]
    [TemplatePart(Name = RichTextBlockName, Type = typeof(RichTextBlock))]
    public partial class TextBlockEx : Control
    {
        private const string RichTextBlockName = "PART_RichTextBlock";
        public const string AuthorBorder = "<div class=\"author-border\"/>";

        private RichTextBlock RichTextBlock;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Feed");

        /// <summary>
        /// Creates a new instance of the <see cref="TextBlockEx"/> class.
        /// </summary>
        public TextBlockEx() => DefaultStyleKey = typeof(TextBlockEx);

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RichTextBlock = GetTemplateChild(RichTextBlockName) as RichTextBlock;
            RenderTextBlock();
        }
    }
}
