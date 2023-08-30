using CoolapkLite.Controls.Containers;
using CoolapkLite.Controls.Writers;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using HtmlAgilityPack;
using System.Collections.Immutable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    /// <summary>
    /// 基于 Coolapk UWP 项目的 <seealso href="https://github.com/Coolapk-UWP/Coolapk-UWP/blob/master/CoolapkUWP/CoolapkUWP/Controls/TextBlockEx.xaml.cs">TextBlockEx</seealso> 控件、
    /// Windows App Studio 项目的 <seealso href="https://github.com/wasteam/waslibs/tree/master/src/AppStudio.Uwp/Controls/HtmlBlock">HtmlBlock</seealso> 控件和
    /// Cyenoch 的 <seealso href="https://github.com/Cyenoch/Coolapk-UWP/blob/master/Controls/MyRichTextBlock.xaml.cs">MyRichTextBlock</seealso> 控件重制
    /// </summary>
    [ContentProperty(Name = nameof(Text))]
    [TemplatePart(Name = RichTextBlockName, Type = typeof(RichTextBlock))]
    public partial class TextBlockEx : Control
    {
        private const string RichTextBlockName = "PART_RichTextBlock";

        public const string AuthorBorder = "<div class=\"author-border\"/>";

        private RichTextBlock RichTextBlock;

        public ImmutableArray<ImageModel>.Builder ImageArrayBuilder { get; } = ImmutableArray.CreateBuilder<ImageModel>();

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

        private void RenderTextBlock()
        {
            if (RichTextBlock != null)
            {
                RichTextBlock.Blocks.Clear();

                if (string.IsNullOrEmpty(Text)) { return; }

                RichTextBlockDocumentContainer container = new RichTextBlockDocumentContainer(RichTextBlock);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(Text);

                HtmlNode body = doc?.DocumentNode;

                WriteFragments(body, container);

                ImmutableArray<ImageModel> array = ImageArrayBuilder.ToImmutable();
                foreach (ImageModel item in array)
                {
                    item.ContextArray = array;
                }
                ImageArrayBuilder.Clear();
            }
        }

        private void WriteFragments(HtmlNode fragment, DocumentContainer parentContainer)
        {
            if (parentContainer != null)
            {
                foreach (HtmlNode childFragment in fragment.ChildNodes)
                {
                    HtmlWriter writer = HtmlWriterFactory.Create(childFragment);

                    DependencyObject element = writer?.GetControl(childFragment, this);

                    if (element != null)
                    {
                        if (!parentContainer.CanContain(element))
                        {
                            DocumentContainer ascendantContainer = parentContainer.FindAscendant(element);

                            if (ascendantContainer == null)
                            {
                                continue;
                            }
                            else
                            {
                                parentContainer = ascendantContainer;
                            }
                        }

                        DocumentContainer currentContainer = parentContainer.Append(element);

                        WriteFragments(childFragment, currentContainer);
                    }
                }
            }
        }

        public void OnLinkClicked(string href)
        {
            if (LinkClicked == null)
            {
                _ = this.OpenLinkAsync(href);
            }
            else
            {
                LinkClicked.Invoke(this, href);
            }
        }

        public void OnImageClicked(ImageModel imageModel, TappedRoutedEventArgs args)
        {
            if (args.Handled) { return; }
            if (LinkClicked == null)
            {
                _ = this.ShowImageAsync(imageModel);
            }
            else
            {
                ImageClicked.Invoke(this, imageModel);
            }
            args.Handled = true;
        }
    }
}
